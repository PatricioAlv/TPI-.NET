using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Messages.Service.Data;
using Messages.Service.Models;
using Shared.DTOs;
using System.Security.Claims;

namespace Messages.Service.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly MessagesDbContext _context;
    private readonly ILogger<ChatHub> _logger;
    private static readonly Dictionary<int, string> _userConnections = new();

    public ChatHub(MessagesDbContext context, ILogger<ChatHub> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            _userConnections[userId.Value] = Context.ConnectionId;
            _logger.LogInformation("Usuario {UserId} conectado con ConnectionId {ConnectionId}", userId, Context.ConnectionId);

            // Notificar a otros usuarios que este usuario está en línea
            await Clients.Others.SendAsync("UserOnline", new { UserId = userId.Value, ConnectionId = Context.ConnectionId });
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue && _userConnections.ContainsKey(userId.Value))
        {
            _userConnections.Remove(userId.Value);
            _logger.LogInformation("Usuario {UserId} desconectado", userId);

            // Notificar a otros usuarios que este usuario está desconectado
            await Clients.Others.SendAsync("UserOffline", new { UserId = userId.Value, LastSeen = DateTime.UtcNow });
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(SendMessageDto messageDto)
    {
        var senderId = GetUserId();
        if (!senderId.HasValue)
        {
            await Clients.Caller.SendAsync("Error", "Usuario no autenticado");
            return;
        }

        // Validar que sea mensaje 1:1 o grupal (no ambos)
        if ((messageDto.ReceiverId.HasValue && messageDto.GroupId.HasValue) ||
            (!messageDto.ReceiverId.HasValue && !messageDto.GroupId.HasValue))
        {
            await Clients.Caller.SendAsync("Error", "Debe especificar ReceiverId o GroupId, no ambos");
            return;
        }

        // Crear mensaje
        var message = new Message
        {
            SenderId = senderId.Value,
            ReceiverId = messageDto.ReceiverId,
            GroupId = messageDto.GroupId,
            Content = messageDto.Content,
            Type = messageDto.Type,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Obtener información del remitente
        var sender = await _context.ChatParticipants.FirstOrDefaultAsync(p => p.UserId == senderId.Value);
        
        var messageResponse = new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderUsername = sender?.Username ?? "Unknown",
            SenderDisplayName = sender?.DisplayName,
            ReceiverId = message.ReceiverId,
            GroupId = message.GroupId,
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead,
            Type = message.Type
        };

        // Enviar mensaje según el tipo de chat
        if (messageDto.ReceiverId.HasValue)
        {
            // Mensaje 1:1
            if (_userConnections.TryGetValue(messageDto.ReceiverId.Value, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", messageResponse);
            }
            // Confirmar al remitente
            await Clients.Caller.SendAsync("MessageSent", messageResponse);
        }
        else if (messageDto.GroupId.HasValue)
        {
            // Mensaje grupal - enviar a todos en el grupo
            await Clients.Group($"Group_{messageDto.GroupId}").SendAsync("ReceiveMessage", messageResponse);
        }

        _logger.LogInformation("Mensaje {MessageId} enviado por usuario {UserId}", message.Id, senderId);
    }

    public async Task JoinChat(int chatId, bool isGroup)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        string groupName = isGroup ? $"Group_{chatId}" : $"Chat_{Math.Min(userId.Value, chatId)}_{Math.Max(userId.Value, chatId)}";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Usuario {UserId} se unió a {GroupName}", userId, groupName);
    }

    public async Task LeaveChat(int chatId, bool isGroup)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        string groupName = isGroup ? $"Group_{chatId}" : $"Chat_{Math.Min(userId.Value, chatId)}_{Math.Max(userId.Value, chatId)}";
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Usuario {UserId} salió de {GroupName}", userId, groupName);
    }

    public async Task NotifyTyping(int chatId, bool isGroup, bool isTyping)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        var sender = await _context.ChatParticipants.FirstOrDefaultAsync(p => p.UserId == userId.Value);
        
        var notification = new TypingNotificationDto
        {
            ChatId = chatId,
            UserId = userId.Value,
            Username = sender?.Username ?? "Unknown",
            IsTyping = isTyping,
            IsGroup = isGroup
        };

        string groupName = isGroup ? $"Group_{chatId}" : $"Chat_{chatId}";
        
        // Enviar a todos excepto al remitente
        await Clients.OthersInGroup(groupName).SendAsync("UserTyping", notification);
    }

    public async Task MarkMessageAsRead(int messageId)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        var message = await _context.Messages.FindAsync(messageId);
        if (message == null) return;

        // Verificar que el usuario puede marcar este mensaje como leído
        bool canRead = false;
        if (message.GroupId.HasValue)
        {
            // Para grupos, cualquier miembro puede marcar como leído
            canRead = true;
        }
        else if (message.ReceiverId == userId.Value)
        {
            // Para 1:1, solo el receptor
            canRead = true;
        }

        if (!canRead) return;

        // Verificar si ya existe registro de lectura
        var existingRead = await _context.MessageReads
            .FirstOrDefaultAsync(mr => mr.MessageId == messageId && mr.UserId == userId.Value);

        if (existingRead == null)
        {
            var messageRead = new MessageRead
            {
                MessageId = messageId,
                UserId = userId.Value,
                ReadAt = DateTime.UtcNow
            };

            _context.MessageReads.Add(messageRead);
            await _context.SaveChangesAsync();

            // Notificar al remitente del mensaje
            if (_userConnections.TryGetValue(message.SenderId, out var senderConnectionId))
            {
                await Clients.Client(senderConnectionId).SendAsync("MessageRead", new
                {
                    messageId = messageId,
                    readByUserId = userId.Value,
                    readAt = messageRead.ReadAt
                });
            }

            // Si es grupo, notificar a todos los miembros
            if (message.GroupId.HasValue)
            {
                await Clients.Group($"Group_{message.GroupId.Value}").SendAsync("MessageRead", new
                {
                    messageId = messageId,
                    readByUserId = userId.Value,
                    readAt = messageRead.ReadAt
                });
            }

            _logger.LogInformation("Mensaje {MessageId} marcado como leído por usuario {UserId}", messageId, userId);
        }
    }

    private int? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
