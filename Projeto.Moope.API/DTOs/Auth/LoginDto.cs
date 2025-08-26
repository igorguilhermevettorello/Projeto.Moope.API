using System.ComponentModel.DataAnnotations;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.API.DTOs.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "O E-mail é obrigatério.")]
        [EmailAddress(ErrorMessage = "E-mail invélido.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "A {0} é obrigatéria.")]
        public string? Senha { get; set; }

        [Required(ErrorMessage = "O captcha é obrigatório.")]
        public string? RecaptchaToken { get; set; }
        public TipoUsuario? TipoUsuario { get; set; }
    }
}