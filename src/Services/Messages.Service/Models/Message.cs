using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.DTOs;

namespace Messages.Service.Models;

public class Message
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int SenderId { get; set; }

    public int? ReceiverId { get; set; } // Para mensajes 1:1

    public int? GroupId { get; set; } // Para mensajes grupales

    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;

    public MessageType Type { get; set; } = MessageType.Text;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public bool IsDeleted { get; set; }
}

public class ChatParticipant
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public bool IsOnline { get; set; }

    public DateTime? LastSeen { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class MessageRead
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int MessageId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    public Message? Message { get; set; }
}
