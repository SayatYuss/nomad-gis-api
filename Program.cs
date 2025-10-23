using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using nomad_gis_V2.Data;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Middleware;
using nomad_gis_V2.Models;
using nomad_gis_V2.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========== PostgreSQL ==========
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========== JWT Authentication ==========
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // выключи, если используешь HTTPS — тогда поставь true
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

// ========== DI ==========
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IMapPointService, MapPointService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IGameService, GameService>();

// ========== CORS ==========
// Добавляем политику CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdminPanel",
        builder =>
        {
            builder.AllowAnyOrigin() // Для разработки. В production укажи конкретный домен
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});


// ========== Контроллеры и Swagger ==========
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nomad GIS API", Version = "v1" });

    // Добавляем поддержку JWT в Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Введите JWT токен так: Bearer {your token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

// ========== 2. ВЫЗОВ DATA SEEDER ==========
// (Лучше запускать только в Development-режиме, 
// чтобы не проверять БД каждый раз в production)
if (app.Environment.IsDevelopment()) 
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            // Вызываем наш статический метод сидера
            await DataSeeder.SeedAdminUser(services, configuration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during admin user seeding.");
        }
    }
}

// ========== Middleware ==========
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAdminPanel");

app.UseStaticFiles();

// Включаем аутентификацию и авторизацию
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
