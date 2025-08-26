using System.ComponentModel.DataAnnotations;
using Projeto.Moope.Core.Commands.Base;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Commands.Emails
{
    /// <summary>
    /// Command para salvar um email
    /// </summary>
    public class SalvarEmailCommand : ICommand<Result<Guid>>
    {
        /// <summary>
        /// Endereço de email do remetente
        /// </summary>
        [Required(ErrorMessage = "O campo Remetente é obrigatório")]
        [EmailAddress(ErrorMessage = "Remetente deve ter um formato de email válido")]
        public string Remetente { get; set; } = string.Empty;

        /// <summary>
        /// Nome do remetente (opcional)
        /// </summary>
        public string? NomeRemetente { get; set; }

        /// <summary>
        /// Endereço de email do destinatário
        /// </summary>
        [Required(ErrorMessage = "O campo Destinatario é obrigatório")]
        [EmailAddress(ErrorMessage = "Destinatario deve ter um formato de email válido")]
        public string Destinatario { get; set; } = string.Empty;

        /// <summary>
        /// Nome do destinatário (opcional)
        /// </summary>
        public string? NomeDestinatario { get; set; }

        /// <summary>
        /// Endereços de cópia (CC) separados por vírgula
        /// </summary>
        public string? Copia { get; set; }

        /// <summary>
        /// Endereços de cópia oculta (BCC) separados por vírgula
        /// </summary>
        public string? CopiaOculta { get; set; }

        /// <summary>
        /// Assunto do email
        /// </summary>
        [Required(ErrorMessage = "O campo Assunto é obrigatório")]
        public string Assunto { get; set; } = string.Empty;

        /// <summary>
        /// Corpo do email (texto ou HTML)
        /// </summary>
        [Required(ErrorMessage = "O campo Corpo é obrigatório")]
        public string Corpo { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o email é HTML (true) ou texto plano (false)
        /// </summary>
        public bool EhHtml { get; set; } = true;

        /// <summary>
        /// Prioridade do email
        /// </summary>
        public Prioridade Prioridade { get; set; } = Prioridade.Normal;

        /// <summary>
        /// ID do usuário relacionado (opcional)
        /// </summary>
        public Guid? UsuarioId { get; set; }

        /// <summary>
        /// ID do cliente relacionado (opcional)
        /// </summary>
        public Guid? ClienteId { get; set; }

        /// <summary>
        /// Tipo/categoria do email
        /// </summary>
        public string? Tipo { get; set; }

        /// <summary>
        /// Dados adicionais em formato JSON (opcional)
        /// </summary>
        public string? DadosAdicionais { get; set; }

        /// <summary>
        /// Data programada para envio (opcional, para emails agendados)
        /// </summary>
        public DateTime? DataProgramada { get; set; }

        /// <summary>
        /// Indica se o email deve ser enviado imediatamente após salvar
        /// </summary>
        public bool EnviarImediatamente { get; set; } = true;
    }
}
