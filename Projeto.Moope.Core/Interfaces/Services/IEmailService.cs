using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Core.Interfaces.Services
{
    /// <summary>
    /// Interface para serviço de envio de emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envia um email
        /// </summary>
        /// <param name="email">Dados do email a ser enviado</param>
        /// <returns>True se enviado com sucesso, False caso contrário</returns>
        Task<bool> EnviarEmailAsync(Email email);

        /// <summary>
        /// Envia email simples
        /// </summary>
        /// <param name="destinatario">Email do destinatário</param>
        /// <param name="assunto">Assunto do email</param>
        /// <param name="corpo">Corpo do email</param>
        /// <param name="ehHtml">Se o corpo é HTML</param>
        /// <returns>True se enviado com sucesso</returns>
        Task<bool> EnviarEmailSimplesAsync(string destinatario, string assunto, string corpo, bool ehHtml = true);

        /// <summary>
        /// Processa emails pendentes
        /// </summary>
        /// <returns>Número de emails processados</returns>
        Task<int> ProcessarEmailsPendentesAsync();

        /// <summary>
        /// Processa emails agendados que devem ser enviados
        /// </summary>
        /// <returns>Número de emails processados</returns>
        Task<int> ProcessarEmailsAgendadosAsync();

        /// <summary>
        /// Reprocessa emails com falha
        /// </summary>
        /// <param name="maxTentativas">Número máximo de tentativas</param>
        /// <returns>Número de emails reprocessados</returns>
        Task<int> ReprocessarEmailsComFalhaAsync(int maxTentativas = 3);

        /// <summary>
        /// Valida configuração do serviço de email
        /// </summary>
        /// <returns>True se configurado corretamente</returns>
        Task<bool> ValidarConfiguracaoAsync();
    }
}
