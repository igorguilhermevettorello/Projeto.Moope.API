namespace Projeto.Moope.Core.DTOs.Pagamentos
{
    /// <summary>
    /// DTO para requisição de autenticação do CelPay
    /// </summary>
    public class CelPayAuthRequestDto
    {
        public string GrantType { get; set; } = "authorization_code";
        public string Scope { get; set; } = "customers.read customers.write plans.read plans.write transactions.read transactions.write webhooks.write cards.read cards.write card-brands.read subscriptions.read subscriptions.write charges.read charges.write boletos.read";
    }

    /// <summary>
    /// DTO para resposta de autenticação do CelPay
    /// </summary>
    public class CelPayAuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = string.Empty;
        public DateTime? ObtidoEm { get; set; }
        
        /// <summary>
        /// Verifica se o token ainda é válido
        /// </summary>
        public bool IsTokenValido => ObtidoEm.HasValue && 
            DateTime.UtcNow < ObtidoEm.Value.AddSeconds(ExpiresIn - 30); // 30 segundos de margem
    }

    /// <summary>
    /// DTO para configuração de autenticação do CelPay
    /// </summary>
    public class CelPayAuthConfigDto
    {
        public string GalaxId { get; set; } = string.Empty;
        public string GalaxHash { get; set; } = string.Empty;
        public string? GalaxIdPartner { get; set; }
        public string? GalaxHashPartner { get; set; }
        public string BaseUrl { get; set; } = string.Empty;
        public bool IsProduction { get; set; }
        public string Scope { get; set; } = string.Empty;
    }
}
