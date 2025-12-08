namespace nomad_gis_V2.Exceptions;

/// <summary>
/// Для ошибок валидации (400 Bad Request)
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Инициализирует новый экземпляр исключения валидации.
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public ValidationException(string message) : base(message) { }
}

/// <summary>
/// Для ненайденных ресурсов (404 Not Found)
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Инициализирует новый экземпляр исключения "не найдено".
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public NotFoundException(string message) : base(message) { }
}

/// <summary>
/// Для конфликтов (409 Conflict), например, дубликат email
/// </summary>
public class DuplicateException : Exception
{
    /// <summary>
    /// Инициализирует новый экземпляр исключения дубликата.
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public DuplicateException(string message) : base(message) { }
}

/// <summary>
/// Для ошибок авторизации (401 Unauthorized), например, неверный пароль
/// </summary>
public class UnauthorizedException : Exception
{
    /// <summary>
    /// Инициализирует новый экземпляр исключения отсутствия авторизации.
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public UnauthorizedException(string message) : base(message) { }
}

