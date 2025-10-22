using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Auth;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtService _jwtService;
    private readonly int RefreshTokenExpirationDays;
    private readonly int AccessTokenExpirationMinutes;

    public AuthService(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, JwtService jwtService, IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        RefreshTokenExpirationDays = configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
        AccessTokenExpirationMinutes = configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes", 15);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        string Identifier = request.Identifier;
        string password = request.Password;
        string DeviceId = request.DeviceId;

        // Загружаем пользователя вместе с RefreshTokens
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == Identifier || u.Username == Identifier);

        if (user == null)
            throw new Exception("Пользователь не найден.");

        var verifyPassword = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verifyPassword == PasswordVerificationResult.Failed)
            throw new Exception("Неверный пароль.");

        // Отзываем старые токены для этого устройства
        foreach (var token in user.RefreshTokens
                    .Where(t => t.DeviceId == DeviceId && t.RevorkedAt == null))
        {
            token.RevorkedAt = DateTime.UtcNow;
        }

        // Создаём новый refresh токен
        (string accessToken, string refreshToken) = _jwtService.GenerateTokens(user);
        var newTokenEntity = TokenEntity(refreshToken, DeviceId, user);
        _context.RefreshTokens.Add(newTokenEntity);

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Experience = user.Experience,
                Level = user.Level
            }
        };
    }


    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        string email = request.Email;
        string username = request.Username;
        string password = request.Password;
        string deviceId = request.DeviceId;

        if (await _context.Users.AnyAsync(u => u.Email == email))
            throw new Exception("Пользователь с таким email уже существует.");

        if (await _context.Users.AnyAsync(u => u.Username == username))
            throw new Exception("Пользователь с таким именем уже существует.");

        var user = new User
        {
            Email = email,
            Username = username,
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        (string accessToken, string refreshToken) = _jwtService.GenerateTokens(user);

        var newTokenEntity = TokenEntity(refreshToken, deviceId, user);

        user.RefreshTokens.Add(newTokenEntity);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Experience = user.Experience,
                Level = user.Level
            }
        };
    }

    public async Task<bool> LogoutAsync(LogoutRequest request)
    {
        // Находим refresh токен по данным запроса
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt =>
                rt.Token == request.RefreshToken &&
                rt.DeviceId == request.DeviceId &&
                rt.UserId == request.UserId &&
                rt.RevorkedAt == null);

        if (refreshToken == null)
            throw new Exception("Токен не найден или уже отозван.");

        // Помечаем токен как отозванный
        refreshToken.RevorkedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        Guid userId = request.UserId;
        string deviceId = request.DeviceId;
        string refreshTokenValue = request.RefreshToken;

        var existingToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt =>
                rt.Token == refreshTokenValue &&
                rt.DeviceId == deviceId &&
                rt.UserId == userId);

        if (existingToken == null)
            throw new Exception("Refresh-токен не найден.");

        if (existingToken.RevorkedAt != null)
            throw new Exception("Токен уже был отозван.");

        if (existingToken.Expires <= DateTime.UtcNow)
            throw new Exception("Срок действия токена истёк.");

        var user = existingToken.User;

        // Отзываем старый токен
        existingToken.RevorkedAt = DateTime.UtcNow;

        // Генерируем новую пару токенов
        (string accessToken, string newRefreshToken) = _jwtService.GenerateTokens(user);

        // Создаём новую запись в таблице RefreshTokens
        var newTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            DeviceId = deviceId,
            Expires = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        };

        // Добавляем в контекст
        _context.RefreshTokens.Add(newTokenEntity);

        await _context.SaveChangesAsync();

        // Возвращаем клиенту новые токены
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Experience = user.Experience,
                Level = user.Level
            }
        };
    }


    private RefreshToken TokenEntity(string refreshToken, string DeviceId, User user)
    {
        return new RefreshToken
        {
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            DeviceId = DeviceId,
            UserId = user.Id,
            User = user,
        };
    }
}