namespace Shared.DTOs;

public class GroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AvatarUrl { get; set; }
    public int MembersCount { get; set; }
    public List<GroupMemberDto> Members { get; set; } = new();
}

public class CreateGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> MemberIds { get; set; } = new(); // IDs de usuarios a agregar
}

public class UpdateGroupDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
}

public class GroupMemberDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public GroupMemberRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsOnline { get; set; }
}

public class AddGroupMemberDto
{
    public int UserId { get; set; }
}

public enum GroupMemberRole
{
    Member = 0,
    Admin = 1,
    Owner = 2
}
