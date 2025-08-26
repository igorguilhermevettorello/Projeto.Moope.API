namespace Projeto.Moope.API.Utils
{
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public int ExpiracaoHoras { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
