using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.DTOs;

namespace Groups.Service.Models;

public class Group
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public bool IsDeleted { get; set; }

    // Navegación
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
}

public class GroupMember
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int GroupId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public GroupMemberRole Role { get; set; } = GroupMemberRole.Member;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navegación
    [ForeignKey(nameof(GroupId))]
    public Group Group { get; set; } = null!;
}
