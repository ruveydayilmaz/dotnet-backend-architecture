using System.Text.Json;
using Core.Utilities.Results;

namespace WebAPI.Middleware
{
    public class CustomAuthenticationFailedMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomAuthenticationFailedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Response.StatusCode == 401)
            {
                context.Response.ContentType = "application/json";
                var response = new ErrorResult("Unauthorized: Invalid or expired token");
                var jsonResponse = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(jsonResponse);
            }

            await _next(context);
        }
    }
}
