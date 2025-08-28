using Microsoft.AspNetCore.Mvc;

namespace Projeto.Moope.API.Configurations
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:4200") 
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
                
                // Política mais permissiva para desenvolvimento
                options.AddPolicy("DevelopmentCorsPolicy", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddHealthChecks();

            //services.AddHealthChecksUI(options =>
            //{
            //    options.SetEvaluationTimeInSeconds(15);
            //    options.MaximumHistoryEntriesPerEndpoint(60);
            //    //options.AddHealthCheckEndpoint("API", "/health");
            //    options.AddHealthCheckEndpoint("API", "https://localhost:7067/health");
            //}).AddInMemoryStorage();

            return services;
        }
    }
}
