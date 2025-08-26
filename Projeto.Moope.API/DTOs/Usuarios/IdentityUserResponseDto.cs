namespace Projeto.Moope.API.DTOs.Usuarios
{
    public class IdentityUserResponseDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public bool Sucesso { get; set; }
        public List<string> Erros { get; set; } = new List<string>();
        public string TokenConfirmacaoEmail { get; set; }
    }
}
