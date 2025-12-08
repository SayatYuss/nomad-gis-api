using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Data;

/// <summary>
/// Контекст базы данных приложения Nomad GIS.
/// Управляет всеми сущностями: пользователи, точки, ачивки, сообщения и т.д.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Инициализирует новый экземпляр класса ApplicationDbContext.
    /// </summary>
    /// <param name="options">Опции конфигурации контекста базы данных</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    /// <summary>
    /// Получает или задает набор пользователей в базе данных.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Получает или задает набор ачивок в базе данных.
    /// </summary>
    public DbSet<Achievement> Achievements { get; set; } = null!;

    /// <summary>
    /// Получает или задает набор связей между пользователями и ачивками.
    /// </summary>
    public DbSet<UserAchievement> UserAchievements { get; set; } = null!;

    /// <summary>
    /// Получает или задает набор записей о прогрессе пользователей по открытию точек на карте.
    /// </summary>
    public DbSet<UserMapProgress> UserMapProgress { get; set; } = null!;

    /// <summary>
    /// Получает или задает набор точек интереса на карте.
    /// </summary>
    public DbSet<MapPoint> MapPoints { get; set; } = null!;

    /// <summary>
    /// Получает или задает набор сообщений и комментариев на точках.
    /// </summary>
    public DbSet<Message> Messages { get; set; } = null!;

    /// <summary>
    /// Получает или задает набор refresh токенов для аутентификации.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    /// <summary>
    /// Получает или задает набор лайков на сообщениях.
    /// </summary>
    public DbSet<MessageLike> MessageLikes { get; set; } = null!;

    /// <summary>
    /// Конфигурирует модель данных и устанавливает отношения между сущностями.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели для конфигурирования сущностей</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- USER ----
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Username).IsRequired().HasMaxLength(100);
            b.Property(u => u.Email).IsRequired().HasMaxLength(200);

            b.HasIndex(u => u.Email).IsUnique();
            b.HasIndex(u => u.Username).IsUnique();

            b.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- ACHIEVEMENTS ----
        modelBuilder.Entity<Achievement>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Code).IsRequired().HasMaxLength(100);
            b.Property(a => a.Title).IsRequired().HasMaxLength(150);
        });

        // ---- USER-ACHIEVEMENTS (M:M) ----
        modelBuilder.Entity<UserAchievement>(b =>
        {
            b.HasKey(ua => new { ua.UserId, ua.AchievementId });

            b.HasOne(ua => ua.User)
                .WithMany(u => u.UserAchievements)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ua => ua.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(ua => ua.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- USER MAP PROGRESS ----
        modelBuilder.Entity<UserMapProgress>(b =>
        {
            b.HasKey(ump => new { ump.UserId, ump.MapPointId });

            b.HasOne(ump => ump.User)
                .WithMany(u => u.MapProgress)
                .HasForeignKey(ump => ump.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ump => ump.MapPoint)
                .WithMany(mp => mp.UserProgress) // <-- здесь всё правильно
                .HasForeignKey(ump => ump.MapPointId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // ---- MESSAGES ----
        modelBuilder.Entity<Message>(b =>
        {
            b.HasKey(m => m.Id);

            b.HasOne(m => m.User)
                .WithMany(u => u.Messages) // <-- Указываем обратную связь
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(m => m.Point)
                .WithMany(p => p.Messages) // <-- ИЗМЕНЕНО (вместо .WithMany())
                .HasForeignKey(m => m.MapPointId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(m => m.Likes)
                .WithOne(l => l.Message)
                .HasForeignKey(l => l.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- MESSAGE LIKES ----
        modelBuilder.Entity<MessageLike>(b =>
        {
            b.HasOne(l => l.User)
                .WithMany() // У User нет обратной коллекции лайков, поэтому WithMany() пустой
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении пользователя - удалить все его лайки
        });

        // ---- REFRESH TOKENS ----
        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasKey(rt => rt.Id);
            b.Property(rt => rt.Token).IsRequired().HasMaxLength(512);
            b.Property(rt => rt.DeviceId).IsRequired().HasMaxLength(200);
        });
    }

    /// <summary>
    /// Асинхронно сохраняет все изменения в базу данных.
    /// Автоматически устанавливает метки времени для создания и обновления.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции</param>
    /// <returns>Количество строк, затронутых операцией сохранения</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is User &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            ((User)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                ((User)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
