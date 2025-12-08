using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using nomad_gis_V2.Exceptions;

namespace nomad_gis_V2.Middleware
{
    /// <summary>
    /// DTO для ответа об ошибке при обработке исключений.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Статус успеха операции (всегда false для ошибок).
        /// </summary>
        public bool Success { get; set; } = false;

        /// <summary>
        /// Сообщение об ошибке для клиента.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Глобальное middleware для обработки всех необработанных исключений.
    /// Преобразует исключения в стандартные HTTP ответы с кодами ошибок.
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр middleware для обработки исключений.
        /// </summary>
        /// <param name="next">Следующее middleware в конвейере</param>
        /// <param name="logger">Логгер для записи ошибок</param>
        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Вызывает следующее middleware и обрабатывает выброшенные исключения.
        /// </summary>
        /// <param name="context">Контекст HTTP запроса</param>
        /// <returns>Задача асинхронного выполнения</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response already started, cannot write error response.");
                return;
            }

            var (status, message) = exception switch
            {
                ValidationException => (HttpStatusCode.BadRequest, exception.Message),
                NotFoundException => (HttpStatusCode.NotFound, exception.Message),
                DuplicateException => (HttpStatusCode.Conflict, exception.Message),
                UnauthorizedException => (HttpStatusCode.Unauthorized, exception.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),

                _ => (HttpStatusCode.InternalServerError,
                      "Internal Server Error. Please try again later.")
            };

            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";

            _logger.LogError(exception, "[GlobalException] {Message}", exception.Message);

            var response = new ErrorResponse
            {
                Success = false,
                Message = message
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
