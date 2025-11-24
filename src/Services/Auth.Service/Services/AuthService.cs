using Auth.Service.Data;
using Auth.Service.Models;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;

namespace Auth.Service.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
    Task<UserDto?> GetUserProfileAsync(int userId);
    Task<bool> UpdateUserProfileAsync(int userId, UserDto userDto);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task<List<UserDto>> GetAllUsersAsync(int currentUserId);
}

public class AuthService : IAuthService
{
    private readonly AuthDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly Shared.Utilities.JwtSettings _jwtSettings;

    public AuthService(AuthDbContext context, ITokenService tokenService, Shared.Utilities.JwtSettings jwtSettings)
    {
        _context = context;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        // Validar si el usuario ya existe
        if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email))
        {
            return null; // Usuario ya existe
        }

        // Crear nuevo usuario
        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            DisplayName = registerDto.DisplayName ?? registerDto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generar tokens
        return await GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return null; // Credenciales inválidas
        }

        // Actualizar estado de conexión
        user.IsOnline = true;
        user.LastSeen = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (token == null || token.ExpiresAt < DateTime.UtcNow)
        {
            return null; // Token inválido o expirado
        }

        // Revocar el token anterior
        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;

        // Generar nuevos tokens
        var response = await GenerateAuthResponse(token.User);
        await _context.SaveChangesAsync();

        return response;
    }

    public async Task<UserDto?> GetUserProfileAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            IsOnline = user.IsOnline,
            LastSeen = user.LastSeen
        };
    }

    public async Task<bool> UpdateUserProfileAsync(int userId, UserDto userDto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.DisplayName = userDto.DisplayName;
        user.AvatarUrl = userDto.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (token != null)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private async Task<AuthResponseDto> GenerateAuthResponse(User user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Username, user.Email);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Guardar refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt,
                IsOnline = user.IsOnline,
                LastSeen = user.LastSeen
            },
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
        };
    }

    public async Task<List<UserDto>> GetAllUsersAsync(int currentUserId)
    {
        var users = await _context.Users
            .Where(u => u.Id != currentUserId)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                DisplayName = u.DisplayName,
                AvatarUrl = u.AvatarUrl,
                CreatedAt = u.CreatedAt,
                IsOnline = u.IsOnline,
                LastSeen = u.LastSeen
            })
            .OrderBy(u => u.Username)
            .ToListAsync();

        return users;
    }
}
