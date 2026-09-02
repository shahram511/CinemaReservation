using CinemaReservation.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace CinemaReservation.API.Middleware
{
    public class GlobalExeptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExeptionHandler> _logger;
        public GlobalExeptionHandler(ILogger<GlobalExeptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title) = exception switch
            {
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Request"),
                InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid Request"),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            if (statusCode >= 500)
                _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
            else
                _logger.LogWarning(exception, "Request failed: {Message}", exception.Message);

            httpContext.Response.StatusCode = statusCode;

            var detail = statusCode >= 500
                ? "Please check the logs or try again later."
                : exception.Message;

            await httpContext.Response.WriteAsJsonAsync(new { Error = detail, Title = title }, cancellationToken);
            return true;
        }
    }
}
