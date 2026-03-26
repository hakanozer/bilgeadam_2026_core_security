using System.Security.Claims;
using System.Text.Json;

public class CustomRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitService _rateLimitService;

    public CustomRateLimitMiddleware(RequestDelegate next, RateLimitService rateLimitService)
    {
        _next = next;
        _rateLimitService = rateLimitService;
    }

    public async Task Invoke(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
        var path = context.Request.Path.ToString().ToLower();

        var user = context.User;
        var role = user?.FindFirst(ClaimTypes.Role)?.Value;

        string key;
        int limit = 100;
        var period = TimeSpan.FromMinutes(1);

        if (path.Contains("/api/auth/login"))
        {
            limit = 5;
            key = $"login:{ip}";
        }
        else if (path.Contains("/api/auth/register"))
        {
            limit = 3;
            key = $"register:{ip}";
        }
        else if (user?.Identity?.IsAuthenticated == true)
        {
            if (role == "Product") limit = 90;
            else if (role == "Note") limit = 95;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
            key = $"user:{userId}";
        }
        else
        {
            limit = 100;
            key = $"ip:{ip}";
        }

        var result = await _rateLimitService.IsAllowedAsync(key, limit, period);

        if (!result.Allowed)
        {
            context.Response.StatusCode = 429;
            context.Response.ContentType = "application/json";

            context.Response.Headers["Retry-After"] = result.RetryAfterSeconds.ToString();

            var json = JsonSerializer.Serialize(new
            {
                message = "Too many requests",
                retryAfterSeconds = result.RetryAfterSeconds
            });

            await context.Response.WriteAsync(json);
            return;
        }

        await _next(context);
    }
}