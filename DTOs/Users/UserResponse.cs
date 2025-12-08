namespace nomad_gis_V2.DTOs.Users
{
    /// <summary>
    /// DTO ответа с базовой информацией о пользователе.
    /// Используется в списках пользователей администратором.
    /// </summary>
    public class UserResponse
    {
        /// <summary>
        /// Уникальный ID пользователя.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Имя пользователя (логин).
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Электронная почта пользователя.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Роль пользователя (User или Admin).
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Дата и время создания учетной записи.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Флаг активности учетной записи.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// URL аватара пользователя.
        /// </summary>
        public string? AvatarUrl { get; set; }
    }
}