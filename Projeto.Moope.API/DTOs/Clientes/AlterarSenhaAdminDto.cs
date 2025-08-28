using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.API.DTOs.Clientes
{
    /// <summary>
    /// DTO para alteração de senha por administradores e vendedores
    /// </summary>
    public class AlterarSenhaAdminDto
    {
        [Required(ErrorMessage = "O ID do cliente é obrigatório")]
        public Guid ClienteId { get; set; }

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
