using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Service.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public bool IsOnline { get; set; }

    public DateTime? LastSeen { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navegación
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class RefreshToken
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public int UserId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRevoked { get; set; }

    public DateTime? RevokedAt { get; set; }

    // Navegación
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
