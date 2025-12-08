using nomad_gis_V2.DTOs.Auth;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace nomad_gis_V2.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Регистрация пользователя.
    /// </summary>
    /// <param name="request">Данные нового пользователя</param>
    /// <returns>Токены + профиль пользователя</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var httpReq = HttpContext;
        var result = await _authService.RegisterAsync(request, httpReq);
        return Ok(result);
    }

    /// <summary>
    /// Вход в существующий аккаунт
    /// </summary>
    /// <param name="request">Данные для входа</param>
    /// <returns></returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        // try-catch убран.
        var result = await _authService.RefreshTokenAsync(request);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        // try-catch убран.
        bool success = await _authService.LogoutAsync(request);
        if (!success) return NotFound(new { message = "Refresh token not found" });

        return Ok(new { message = "Logged out successfully" });
    }
}