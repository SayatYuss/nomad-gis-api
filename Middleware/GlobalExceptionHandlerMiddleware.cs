using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using nomad_gis_V2.Exceptions;

namespace nomad_gis_V2.Middleware
{
    public class ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
    }

    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

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
