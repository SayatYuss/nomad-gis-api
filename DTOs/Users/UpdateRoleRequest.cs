using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.DTOs.Users
{
    /// <summary>
    /// DTO запроса на изменение роли пользователя (только для администраторов).
    /// </summary>
    public class UpdateRoleRequest
    {
        /// <summary>
        /// Новая роль пользователя (User или Admin).
        /// </summary>
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}