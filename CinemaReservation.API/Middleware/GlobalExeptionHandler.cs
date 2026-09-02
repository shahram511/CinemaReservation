using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails()
            {
                Status = StatusCodes.Status500InternalServerError,
                Title =  "An unexpected error occurred.",
                Detail = "Please check the logs or try again later,",
                Instance = httpContext.Request.Path
            };            

            if (exception is KeyNotFoundException)
            {
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Resource not fuond";
                problemDetails.Detail = exception.Message;
            }

            if (exception is KeyNotFoundException) 
            {
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await httpContext.Response.WriteAsJsonAsync(new { Error = exception.Message }, cancellationToken);
                return true;
            }

            if (exception is ArgumentException || exception is InvalidOperationException)
            {
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Invalid Request";
                
                
                problemDetails.Detail = exception.Message;
            }      
            
            if (exception is InvalidOperationException)
            {
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                await httpContext.Response.WriteAsJsonAsync(new {Error = exception.Message}, cancellationToken);
                return true;
            }

            // default fallback for real server crashes
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new { Error = "An unexpected error occurred."}, cancellationToken);
            return true;
        }
    }
}
