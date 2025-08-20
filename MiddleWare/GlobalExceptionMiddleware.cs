using System.Net;
using System.Text.Json;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // Continue down the pipeline
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");

            context.Response.ContentType = "application/json";

            // Default status code
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Set status codes for known exceptions
            if (ex is ArgumentException) context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            else if (ex is KeyNotFoundException) context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            else if (ex is UnauthorizedAccessException) context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            var errorResponse = new { message = ex.Message };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}
