namespace Projeto.Moope.API.DTOs.Usuarios
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; }
        public double ExpiresIn { get; set; }
        public LoginUsuarioDto User { get; set; }
    }
}
