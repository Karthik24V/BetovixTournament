using Serilog;
using System.Net;
using System.Text.Json;

namespace Tournament.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception occurred.");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new { message = "An unexpected error occurred. Please try again later." };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}
