using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestApi.Middleware
{
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
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {

            // logger details
            _logger.LogError(exception, 
            "Global exception caught: {Message} | Path: {Path} | Method: {Method} | Agent: {UserAgent} | Ip: {IpAddress} | Type: {ExceptionType}", 
            exception.Message, 
            context.Request.Path, 
            context.Request.Method, 
            context.Request.Headers["User-Agent"].ToString(),
            context.Connection.RemoteIpAddress?.ToString(),
            exception.GetType().Name
            );

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = new
            {
                statusCode = context.Response.StatusCode,
                message = "İç sunucu hatası oluştu",
                detail = exception.Message,
                timestamp = DateTime.UtcNow
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}