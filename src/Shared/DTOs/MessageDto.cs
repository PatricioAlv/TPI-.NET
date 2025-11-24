namespace Shared.DTOs;

public class MessageDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public string? SenderDisplayName { get; set; }
    public int? ReceiverId { get; set; } // Para mensajes 1:1
    public int? GroupId { get; set; } // Para mensajes grupales
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public MessageType Type { get; set; }
}

public class SendMessageDto
{
    public int? ReceiverId { get; set; } // Para chat 1:1
    public int? GroupId { get; set; } // Para chat grupal
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
}

public class MessageListDto
{
    public List<MessageDto> Messages { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage { get; set; }
}

public class TypingNotificationDto
{
    public int ChatId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsTyping { get; set; }
    public bool IsGroup { get; set; }
}

public class MessageReadDto
{
    public int MessageId { get; set; }
    public int ReadByUserId { get; set; }
    public DateTime ReadAt { get; set; }
}

public enum MessageType
{
    Text = 0,
    Image = 1,
    File = 2,
    Audio = 3,
    Video = 4
}
