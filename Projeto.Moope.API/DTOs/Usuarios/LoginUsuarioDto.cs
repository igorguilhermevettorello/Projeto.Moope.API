namespace Projeto.Moope.API.DTOs.Usuarios
{
    public class LoginUsuarioDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public IEnumerable<ClaimDto> Claims { get; set; }
        public string Perfil { get; set; }
    }
}
