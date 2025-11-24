using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Shared.DTOs;
using Auth.Service.Services;
using System.Security.Claims;

namespace Auth.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Validar contraseña fuerte
        if (registerDto.Password.Length < 8)
            return BadRequest(new { message = "La contraseña debe tener al menos 8 caracteres" });

        var response = await _authService.RegisterAsync(registerDto);
        if (response == null)
            return BadRequest(new { message = "El usuario o email ya existe" });

        _logger.LogInformation("Usuario registrado: {Username}", registerDto.Username);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.LoginAsync(loginDto);
        if (response == null)
            return Unauthorized(new { message = "Credenciales inválidas" });

        _logger.LogInformation("Usuario autenticado: {Username}", loginDto.Username);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
            return BadRequest(new { message = "Refresh token requerido" });

        var response = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
        if (response == null)
            return Unauthorized(new { message = "Token inválido o expirado" });

        return Ok(response);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
            return Unauthorized();

        var profile = await _authService.GetUserProfileAsync(userId.Value);
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UserDto userDto)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
            return Unauthorized();

        var success = await _authService.UpdateUserProfileAsync(userId.Value, userDto);
        if (!success)
            return NotFound();

        return Ok(new { message = "Perfil actualizado exitosamente" });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (!string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
        {
            await _authService.RevokeRefreshTokenAsync(refreshTokenDto.RefreshToken);
        }

        return Ok(new { message = "Sesión cerrada exitosamente" });
    }

    [Authorize]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var currentUserId = GetUserIdFromClaims();
        if (currentUserId == null)
            return Unauthorized();

        var users = await _authService.GetAllUsersAsync(currentUserId.Value);
        return Ok(users);
    }

    private int? GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
