using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.API.DTOs.Clientes
{
    /// <summary>
    /// DTO para alteração de senha pelos próprios clientes
    /// </summary>
    public class AlterarSenhaClienteDto
    {
        [Required(ErrorMessage = "A senha atual é obrigatória")]
        [DataType(DataType.Password)]
        public required string SenhaAtual { get; set; }

        [Required(ErrorMessage = "A nova senha é obrigatória")]
        [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres")]
        [DataType(DataType.Password)]
        public required string NovaSenha { get; set; }

        [Required(ErrorMessage = "A confirmação da nova senha é obrigatória")]
        [Compare("NovaSenha", ErrorMessage = "A confirmação da senha não confere com a nova senha")]
        [DataType(DataType.Password)]
        public required string ConfirmarNovaSenha { get; set; }
    }
}
