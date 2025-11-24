using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Groups.Service.Data;
using Groups.Service.Models;
using Shared.DTOs;
using System.Security.Claims;

namespace Groups.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly GroupsDbContext _context;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(GroupsDbContext context, ILogger<GroupsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserGroups()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var groups = await _context.Groups
            .Include(g => g.Members)
            .Where(g => !g.IsDeleted && g.Members.Any(m => m.UserId == userId && m.IsActive))
            .Select(g => new GroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                CreatedByUserId = g.CreatedByUserId,
                CreatedAt = g.CreatedAt,
                AvatarUrl = g.AvatarUrl,
                MembersCount = g.Members.Count(m => m.IsActive),
                Members = g.Members.Where(m => m.IsActive).Select(m => new GroupMemberDto
                {
                    UserId = m.UserId,
                    Username = m.Username,
                    DisplayName = m.DisplayName,
                    AvatarUrl = m.AvatarUrl,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt,
                    IsOnline = false // Esto se actualizaría desde el servicio de mensajes
                }).ToList()
            })
            .ToListAsync();

        return Ok(groups);
    }

    [HttpGet("{groupId}")]
    public async Task<IActionResult> GetGroup(int groupId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);

        if (group == null)
            return NotFound();

        // Verificar que el usuario es miembro del grupo
        if (!group.Members.Any(m => m.UserId == userId && m.IsActive))
            return Forbid();

        var groupDto = new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            CreatedByUserId = group.CreatedByUserId,
            CreatedAt = group.CreatedAt,
            AvatarUrl = group.AvatarUrl,
            MembersCount = group.Members.Count(m => m.IsActive),
            Members = group.Members.Where(m => m.IsActive).Select(m => new GroupMemberDto
            {
                UserId = m.UserId,
                Username = m.Username,
                DisplayName = m.DisplayName,
                AvatarUrl = m.AvatarUrl,
                Role = m.Role,
                JoinedAt = m.JoinedAt,
                IsOnline = false
            }).ToList()
        };

        return Ok(groupDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto createGroupDto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(createGroupDto.Name))
            return BadRequest("El nombre del grupo es requerido");

        var group = new Group
        {
            Name = createGroupDto.Name,
            Description = createGroupDto.Description,
            CreatedByUserId = userId.Value,
            CreatedAt = DateTime.UtcNow
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Obtener información del creador desde el claim
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

        // Agregar al creador como Owner
        var creatorMember = new GroupMember
        {
            GroupId = group.Id,
            UserId = userId.Value,
            Username = username,
            Role = GroupMemberRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        _context.GroupMembers.Add(creatorMember);

        // Agregar otros miembros
        foreach (var memberId in createGroupDto.MemberIds.Where(id => id != userId.Value))
        {
            var member = new GroupMember
            {
                GroupId = group.Id,
                UserId = memberId,
                Username = $"User{memberId}", // Esto debería obtenerse del Auth Service
                Role = GroupMemberRole.Member,
                JoinedAt = DateTime.UtcNow
            };
            _context.GroupMembers.Add(member);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Grupo {GroupId} creado por usuario {UserId}", group.Id, userId);

        // Recargar el grupo con sus miembros para retornar
        var createdGroup = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == group.Id);

        var groupDto = new GroupDto
        {
            Id = createdGroup.Id,
            Name = createdGroup.Name,
            Description = createdGroup.Description,
            CreatedByUserId = createdGroup.CreatedByUserId,
            CreatedAt = createdGroup.CreatedAt,
            AvatarUrl = createdGroup.AvatarUrl,
            MembersCount = createdGroup.Members.Count(m => m.IsActive),
            Members = createdGroup.Members.Where(m => m.IsActive).Select(m => new GroupMemberDto
            {
                UserId = m.UserId,
                Username = m.Username,
                DisplayName = m.DisplayName,
                AvatarUrl = m.AvatarUrl,
                Role = m.Role,
                JoinedAt = m.JoinedAt,
                IsOnline = false
            }).ToList()
        };

        return CreatedAtAction(nameof(GetGroup), new { groupId = group.Id }, groupDto);
    }

    [HttpPut("{groupId}")]
    public async Task<IActionResult> UpdateGroup(int groupId, [FromBody] UpdateGroupDto updateGroupDto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);

        if (group == null)
            return NotFound();

        // Verificar que el usuario es admin u owner
        var member = group.Members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
        if (member == null || (member.Role != GroupMemberRole.Owner && member.Role != GroupMemberRole.Admin))
            return Forbid();

        if (!string.IsNullOrWhiteSpace(updateGroupDto.Name))
            group.Name = updateGroupDto.Name;

        if (updateGroupDto.Description != null)
            group.Description = updateGroupDto.Description;

        if (updateGroupDto.AvatarUrl != null)
            group.AvatarUrl = updateGroupDto.AvatarUrl;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Grupo actualizado exitosamente" });
    }

    [HttpDelete("{groupId}")]
    public async Task<IActionResult> DeleteGroup(int groupId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);

        if (group == null)
            return NotFound();

        // Solo el creador puede eliminar el grupo
        if (group.CreatedByUserId != userId)
            return Forbid();

        group.IsDeleted = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Grupo {GroupId} eliminado por usuario {UserId}", groupId, userId);

        return Ok(new { message = "Grupo eliminado exitosamente" });
    }

    [HttpPost("{groupId}/members")]
    public async Task<IActionResult> AddMember(int groupId, [FromBody] AddGroupMemberDto addMemberDto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);

        if (group == null)
            return NotFound();

        // Verificar que el usuario es admin u owner
        var requester = group.Members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
        if (requester == null || (requester.Role != GroupMemberRole.Owner && requester.Role != GroupMemberRole.Admin))
            return Forbid();

        // Verificar si el usuario ya es miembro
        if (group.Members.Any(m => m.UserId == addMemberDto.UserId && m.IsActive))
            return BadRequest("El usuario ya es miembro del grupo");

        var newMember = new GroupMember
        {
            GroupId = groupId,
            UserId = addMemberDto.UserId,
            Username = $"User{addMemberDto.UserId}", // Obtener del Auth Service
            Role = GroupMemberRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        _context.GroupMembers.Add(newMember);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {MemberId} agregado al grupo {GroupId} por usuario {UserId}", 
            addMemberDto.UserId, groupId, userId);

        return Ok(new { message = "Miembro agregado exitosamente" });
    }

    [HttpDelete("{groupId}/members/{memberId}")]
    public async Task<IActionResult> RemoveMember(int groupId, int memberId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);

        if (group == null)
            return NotFound();

        var member = group.Members.FirstOrDefault(m => m.UserId == memberId && m.IsActive);
        if (member == null)
            return NotFound("Miembro no encontrado");

        // Verificar permisos: solo admins/owners pueden quitar, o el usuario puede salirse
        var requester = group.Members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
        if (userId != memberId && (requester == null || 
            (requester.Role != GroupMemberRole.Owner && requester.Role != GroupMemberRole.Admin)))
            return Forbid();

        // No permitir que el owner se vaya si hay otros miembros
        if (member.Role == GroupMemberRole.Owner && group.Members.Count(m => m.IsActive) > 1)
            return BadRequest("El propietario debe transferir la propiedad antes de salir");

        member.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {MemberId} removido del grupo {GroupId}", memberId, groupId);

        return Ok(new { message = "Miembro removido exitosamente" });
    }

    [HttpGet("{groupId}/members")]
    public async Task<IActionResult> GetGroupMembers(int groupId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);

        if (group == null)
            return NotFound();

        // Verificar que el usuario es miembro
        if (!group.Members.Any(m => m.UserId == userId && m.IsActive))
            return Forbid();

        var members = group.Members
            .Where(m => m.IsActive)
            .Select(m => new GroupMemberDto
            {
                UserId = m.UserId,
                Username = m.Username,
                DisplayName = m.DisplayName,
                AvatarUrl = m.AvatarUrl,
                Role = m.Role,
                JoinedAt = m.JoinedAt,
                IsOnline = false
            })
            .ToList();

        return Ok(members);
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
