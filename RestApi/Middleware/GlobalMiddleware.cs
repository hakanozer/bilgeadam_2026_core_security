using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;

namespace RestApi.Middleware
{
    public class GlobalMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalMiddleware> _logger;

        public GlobalMiddleware(RequestDelegate next, ILogger<GlobalMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestLog = new
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                Url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
                QueryString = context.Request.QueryString.Value,
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                Username = context.User?.Identity?.Name,
                Roles = context.User?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList(),
                JwtToken = ExtractJwtToken(context.Request),
                RequestBody = await ReadRequestBodyAsync(context.Request)
            };

            _logger.LogInformation($"Request Log: {System.Text.Json.JsonSerializer.Serialize(requestLog)}");

            await _next(context);
        }

        private string ExtractJwtToken(HttpRequest request)
        {
            // Önce Authorization header kontrol et
            var authHeader = request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length);
            }

            // Cookie'den al
            if (request.Cookies.TryGetValue("jwt", out var jwt))
            {
                return jwt;
            }

            return null;
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }
    }
}