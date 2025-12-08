using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Services;

/// <summary>
/// Сервис для управления JWT токенами (access и refresh).
/// Генерирует, валидирует и декодирует JWT токены для аутентификации.
/// </summary>
public class JwtService
{
    private readonly string _jwtSecret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    /// <summary>
    /// Инициализирует новый экземпляр сервиса JWT.
    /// </summary>
    /// <param name="configuration">Конфигурация приложения</param>
    public JwtService(IConfiguration configuration)
    {
        _jwtSecret = configuration["Jwt:Secret"] ?? throw new Exception("Jwt:Secret not configured");
        _issuer = configuration["Jwt:Issuer"] ?? "nomad_gis_V2";
        _audience = configuration["Jwt:Audience"] ?? "nomad_gis_users";

        // --- ИЗМЕНЕНО ДЛЯ БЕЗОПАСНОСТИ ---
        if (!int.TryParse(configuration["Jwt:AccessTokenExpirationMinutes"], out _accessTokenExpirationMinutes))
        {
            _accessTokenExpirationMinutes = 15; // Значение по умолчанию
        }

        if (!int.TryParse(configuration["Jwt:RefreshTokenExpirationDays"], out _refreshTokenExpirationDays))
        {
            _refreshTokenExpirationDays = 7; // Значение по умолчанию
        }
        // --- КОНЕЦ ИЗМЕНЕНИЙ ---
    }

    /// <summary>
    /// Генерирует пару access и refresh токенов для пользователя.
    /// </summary>
    /// <param name="user">Пользователь, для которого генерируются токены</param>
    /// <returns>Кортеж с access и refresh токенами</returns>
    public (string AccessToken, string RefreshToken) GenerateTokens(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role), // <-- 2. Читаем роль из объекта user
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessToken = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: creds
        );

        string accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

        string refreshToken = GenerateRefreshToken();

        return (accessTokenString, refreshToken);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
