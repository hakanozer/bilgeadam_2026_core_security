using StackExchange.Redis;

public class RedisService
{
    private readonly IConfiguration _configuration;
    private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

    public RedisService(IConfiguration configuration)
    {
        _configuration = configuration;
        _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            var host = _configuration["Redis:Host"];
            var port = _configuration["Redis:Port"];
            var password = _configuration["Redis:Password"];
            var options = new ConfigurationOptions
            {
                EndPoints = { $"{host}:{port}" },
                Password = string.IsNullOrEmpty(password) ? null : password,
                AbortOnConnectFail = false
            };
            return ConnectionMultiplexer.Connect(options);
        });
    }

    public IDatabase GetDatabase() => _lazyConnection.Value.GetDatabase();
}