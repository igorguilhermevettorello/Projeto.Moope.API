using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Core.Models
{
    /// <summary>
    /// Entidade para armazenar dados de emails enviados/recebidos
    /// </summary>
    public class Email : Entity
    {
        /// <summary>
        /// Endereço de email do remetente
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Remetente { get; set; } = string.Empty;

        /// <summary>
        /// Nome do remetente (opcional)
        /// </summary>
        [StringLength(255)]
        public string? NomeRemetente { get; set; }

        /// <summary>
        /// Endereço de email do destinatário
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Destinatario { get; set; } = string.Empty;

        /// <summary>
        /// Nome do destinatário (opcional)
        /// </summary>
        [StringLength(255)]
        public string? NomeDestinatario { get; set; }

        /// <summary>
        /// Endereços de cópia (CC) separados por vírgula
        /// </summary>
        [StringLength(1000)]
        public string? Copia { get; set; }

        /// <summary>
        /// Endereços de cópia oculta (BCC) separados por vírgula
        /// </summary>
        [StringLength(1000)]
        public string? CopiaOculta { get; set; }

        /// <summary>
        /// Assunto do email
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Assunto { get; set; } = string.Empty;

        /// <summary>
        /// Corpo do email (texto ou HTML)
        /// </summary>
        [Required]
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
        /// Status atual do email
        /// </summary>
        public StatusEmail Status { get; set; } = StatusEmail.Pendente;

        /// <summary>
        /// Número de tentativas de envio
        /// </summary>
        public int TentativasEnvio { get; set; } = 0;
        /// <summary>
        /// Data e hora da última tentativa de envio
        /// </summary>
        public DateTime? UltimaTentativa { get; set; }
        /// <summary>
        /// Data e hora em que o email foi enviado com sucesso
        /// </summary>
        public DateTime? DataEnvio { get; set; }
        /// <summary>
        /// Mensagem de erro em caso de falha no envio
        /// </summary>
        public string? MensagemErro { get; set; }
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
        [StringLength(100)]
        public string? Tipo { get; set; }
        /// <summary>
        /// Dados adicionais em formato JSON (opcional)
        /// </summary>
        public string? DadosAdicionais { get; set; }
        /// <summary>
        /// Data programada para envio (para emails agendados)
        /// </summary>
        public DateTime? DataProgramada { get; set; }
        // Relacionamentos
        /// <summary>
        /// Usuário relacionado
        /// </summary>
        public virtual Usuario? Usuario { get; set; }
        /// <summary>
        /// Cliente relacionado
        /// </summary>
        public virtual Cliente? Cliente { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
