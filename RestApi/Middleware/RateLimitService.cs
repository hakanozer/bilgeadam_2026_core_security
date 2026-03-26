using StackExchange.Redis;
using System.Text.Json;

public class RateLimitService
{
    private readonly IDatabase _db;

    public RateLimitService(RedisService redisService)
    {
        _db = redisService.GetDatabase();
    }

    public async Task<(bool Allowed, int RetryAfterSeconds)> IsAllowedAsync(
        string key, int limit, TimeSpan period)
    {
        var value = await _db.StringGetAsync(key);

        int count = 0;
        DateTime expireAt = DateTime.UtcNow.Add(period);

        if (value.HasValue)
        {
            var data = JsonSerializer.Deserialize<RateLimitEntry>(value!);

            if (data != null)
            {
                count = data.Count;
                expireAt = data.ExpireAt;
            }
        }

        if (expireAt < DateTime.UtcNow)
        {
            // süre dolmuş → reset
            count = 1;
            expireAt = DateTime.UtcNow.Add(period);

            await _db.StringSetAsync(
                key,
                JsonSerializer.Serialize(new RateLimitEntry
                {
                    Count = count,
                    ExpireAt = expireAt
                }),
                period // TTL SADECE BURADA
            );
        }
        else
        {
            count++;

            await _db.StringSetAsync(
                key,
                JsonSerializer.Serialize(new RateLimitEntry
                {
                    Count = count,
                    ExpireAt = expireAt
                })
                // TTL YOK → resetlenmiyor
            );
        }

        if (count > limit)
        {
            var retryAfter = (int)Math.Max(0, (expireAt - DateTime.UtcNow).TotalSeconds);
            return (false, retryAfter);
        }

        return (true, 0);
    }
}