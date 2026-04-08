using SellingFootballTickets_API.exceptions;
using System.Net;
using System.Text.Json;

namespace SellingFootballTickets_API.middleware
{
    public class ExceptionMiddleware
    {
        public readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            httpContext.Response.ContentType = "application/json";

            int statusCode = 500;

            string message = "Internal Server Error";

            if (ex is BaseException baseEx) {
            
                statusCode = baseEx.StatusCode;
                message = baseEx.Message;
            }

            httpContext.Response.StatusCode = statusCode;

            var result = JsonSerializer.Serialize(new { error = message, status = statusCode});

            await httpContext.Response.WriteAsync(result);
        }
    }
}
