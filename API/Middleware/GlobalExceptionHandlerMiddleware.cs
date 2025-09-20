using System.Net;
using System.Text.Json;

namespace API.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
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

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An internal error occurred.";

            // Customize based on exception type
            if (exception.Message.Contains("not found")) statusCode = HttpStatusCode.NotFound;
            else if (exception.Message.Contains("Invalid credentials") || exception.Message.Contains("Invalid refresh token")) statusCode = HttpStatusCode.Unauthorized;
            else if (exception.Message.Contains("linked products")) statusCode = HttpStatusCode.Conflict;

            if (statusCode != HttpStatusCode.InternalServerError) message = exception.Message;

            context.Response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new { error = message });
            return context.Response.WriteAsync(result);
        }
    }
}
