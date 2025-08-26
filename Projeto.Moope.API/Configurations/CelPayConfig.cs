using Projeto.Moope.Core.Interfaces.Pagamentos;
using Projeto.Moope.Infrastructure.Services.Pagamentos;

namespace Projeto.Moope.API.Configurations
{
    public static class CelPayConfig
    {
        public static IServiceCollection AddCelPayServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar HttpClient para CelPay
            services.AddHttpClient<IPaymentGatewayStrategy, CelPayGatewayStrategy>(client =>
            {
                client.BaseAddress = new Uri(configuration["CelPay:BaseUrl"] ?? "https://api.celpayments.com.br");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Registrar a estratégia CelPay como implementação padrão
            services.AddScoped<IPaymentGatewayStrategy, CelPayGatewayStrategy>();

            return services;
        }
    }
}
