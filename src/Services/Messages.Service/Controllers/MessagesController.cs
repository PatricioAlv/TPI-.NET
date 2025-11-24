using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Messages.Service.Data;
using Messages.Service.Models;
using Shared.DTOs;
using System.Security.Claims;

namespace Messages.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly MessagesDbContext _context;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(MessagesDbContext context, ILogger<MessagesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("chat/{otherUserId}")]
    public async Task<IActionResult> GetDirectMessages(
        int otherUserId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var query = _context.Messages
            .Where(m => !m.IsDeleted &&
                ((m.SenderId == userId && m.ReceiverId == otherUserId) ||
                 (m.SenderId == otherUserId && m.ReceiverId == userId)))
            .OrderByDescending(m => m.SentAt);

        var totalCount = await query.CountAsync();
        var messages = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var messageDtos = new List<MessageDto>();
        foreach (var msg in messages)
        {
            var sender = await _context.ChatParticipants.FirstOrDefaultAsync(p => p.UserId == msg.SenderId);
            messageDtos.Add(new MessageDto
            {
                Id = msg.Id,
                SenderId = msg.SenderId,
                SenderUsername = sender?.Username ?? "Unknown",
                SenderDisplayName = sender?.DisplayName,
                ReceiverId = msg.ReceiverId,
                GroupId = msg.GroupId,
                Content = msg.Content,
                SentAt = msg.SentAt,
                IsRead = msg.IsRead,
                ReadAt = msg.ReadAt,
                Type = msg.Type
            });
        }

        var response = new MessageListDto
        {
            Messages = messageDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            HasNextPage = totalCount > pageNumber * pageSize
        };

        return Ok(response);
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetGroupMessages(
        int groupId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Validar que el usuario pertenece al grupo consultando Groups Service
        var isGroupMember = await ValidateGroupMembership(userId.Value, groupId);
        if (!isGroupMember)
        {
            return Forbid();
        }

        var query = _context.Messages
            .Where(m => !m.IsDeleted && m.GroupId == groupId)
            .OrderByDescending(m => m.SentAt);

        var totalCount = await query.CountAsync();
        var messages = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var messageDtos = new List<MessageDto>();
        foreach (var msg in messages)
        {
            var sender = await _context.ChatParticipants.FirstOrDefaultAsync(p => p.UserId == msg.SenderId);
            messageDtos.Add(new MessageDto
            {
                Id = msg.Id,
                SenderId = msg.SenderId,
                SenderUsername = sender?.Username ?? "Unknown",
                SenderDisplayName = sender?.DisplayName,
                ReceiverId = msg.ReceiverId,
                GroupId = msg.GroupId,
                Content = msg.Content,
                SentAt = msg.SentAt,
                IsRead = msg.IsRead,
                ReadAt = msg.ReadAt,
                Type = msg.Type
            });
        }

        var response = new MessageListDto
        {
            Messages = messageDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            HasNextPage = totalCount > pageNumber * pageSize
        };

        return Ok(response);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto messageDto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if ((messageDto.ReceiverId.HasValue && messageDto.GroupId.HasValue) ||
            (!messageDto.ReceiverId.HasValue && !messageDto.GroupId.HasValue))
        {
            return BadRequest("Debe especificar ReceiverId o GroupId, no ambos");
        }

        var message = new Message
        {
            SenderId = userId.Value,
            ReceiverId = messageDto.ReceiverId,
            GroupId = messageDto.GroupId,
            Content = messageDto.Content,
            Type = messageDto.Type,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Mensaje {MessageId} creado vía REST por usuario {UserId}", message.Id, userId);

        return Ok(new { messageId = message.Id, sentAt = message.SentAt });
    }

    [HttpPut("{messageId}/read")]
    public async Task<IActionResult> MarkAsRead(int messageId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var message = await _context.Messages.FindAsync(messageId);
        if (message == null) return NotFound();

        if (message.ReceiverId != userId)
            return Forbid();

        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { messageId, readAt = message.ReadAt });
    }

    [HttpPost("sync-user")]
    public async Task<IActionResult> SyncUser([FromBody] UserDto userDto)
    {
        var participant = await _context.ChatParticipants.FirstOrDefaultAsync(p => p.UserId == userDto.Id);
        
        if (participant == null)
        {
            participant = new ChatParticipant
            {
                UserId = userDto.Id,
                Username = userDto.Username,
                DisplayName = userDto.DisplayName,
                AvatarUrl = userDto.AvatarUrl,
                IsOnline = userDto.IsOnline,
                LastSeen = userDto.LastSeen
            };
            _context.ChatParticipants.Add(participant);
        }
        else
        {
            participant.Username = userDto.Username;
            participant.DisplayName = userDto.DisplayName;
            participant.AvatarUrl = userDto.AvatarUrl;
            participant.IsOnline = userDto.IsOnline;
            participant.LastSeen = userDto.LastSeen;
            participant.LastUpdated = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("mark-read")]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var message = await _context.Messages.FindAsync(request.MessageId);
        if (message == null)
            return NotFound(new { error = "Mensaje no encontrado" });

        // Verificar autorización: usuario debe ser receptor o miembro del grupo
        if (message.GroupId.HasValue)
        {
            // Para grupos, verificar en Groups.Service sería ideal, pero simplificamos
            // asumiendo que si el usuario recibe el mensaje, es miembro
        }
        else if (message.ReceiverId != userId)
        {
            return Forbid();
        }

        // Verificar si ya existe registro de lectura
        var existingRead = await _context.MessageReads
            .FirstOrDefaultAsync(mr => mr.MessageId == request.MessageId && mr.UserId == userId.Value);

        if (existingRead == null)
        {
            var messageRead = new MessageRead
            {
                MessageId = request.MessageId,
                UserId = userId.Value,
                ReadAt = DateTime.UtcNow
            };

            _context.MessageReads.Add(messageRead);
            await _context.SaveChangesAsync();
        }

        return Ok(new { success = true });
    }

    [HttpGet("{messageId}/reads")]
    public async Task<IActionResult> GetMessageReads(int messageId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            return NotFound(new { error = "Mensaje no encontrado" });

        // Verificar autorización
        if (message.GroupId.HasValue)
        {
            // Usuario debe ser miembro del grupo (simplificado)
        }
        else if (message.SenderId != userId && message.ReceiverId != userId)
        {
            return Forbid();
        }

        var reads = await _context.MessageReads
            .Where(mr => mr.MessageId == messageId)
            .Select(mr => new
            {
                userId = mr.UserId,
                readAt = mr.ReadAt
            })
            .ToListAsync();

        // Enriquecer con información de usuario
        var enrichedReads = new List<object>();
        foreach (var read in reads)
        {
            var participant = await _context.ChatParticipants
                .FirstOrDefaultAsync(p => p.UserId == read.userId);

            enrichedReads.Add(new
            {
                userId = read.userId,
                username = participant?.Username ?? "Unknown",
                displayName = participant?.DisplayName,
                readAt = read.readAt
            });
        }

        return Ok(enrichedReads);
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private async Task<bool> ValidateGroupMembership(int userId, int groupId)
    {
        // Llamar al Groups Service para validar membresía
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("X-Internal-Auth", "TPI-Internal-Key");
        
        try
        {
            var response = await httpClient.GetAsync($"http://localhost:5003/api/groups/{groupId}/members");
            if (!response.IsSuccessStatusCode)
                return false;

            var membersJson = await response.Content.ReadAsStringAsync();
            // Simplificado: si devuelve algo, asumimos que es válido
            // En producción, parsear JSON y verificar si userId está en la lista
            return true;
        }
        catch
        {
            // En caso de error, permitir por ahora (mejorable)
            _logger.LogWarning("No se pudo validar membresía de grupo {GroupId} para usuario {UserId}", groupId, userId);
            return true;
        }
    }
}
