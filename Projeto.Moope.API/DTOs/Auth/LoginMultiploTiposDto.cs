using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.API.DTOs.Auth
{
    public class LoginMultiploTiposDto
    {
        public string Message { get; set; }
        public IEnumerable<TipoUsuario> TiposUsuario { get; set; }
        public string Email { get; set; }
    }
}
