namespace RestApi.Extensions
{
    public static class CorsExtensions
    {
        public const string CompanyA = "CompanyA";
        public const string CompanyB = "CompanyB";

        public static IServiceCollection AddCustomCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CompanyA, policy =>
                {
                    policy.WithOrigins("https://a-sirketi.com")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });

                options.AddPolicy(CompanyB, policy =>
                {
                    policy.WithOrigins("https://b-sirketi.com")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            return services;
        }
    }
}