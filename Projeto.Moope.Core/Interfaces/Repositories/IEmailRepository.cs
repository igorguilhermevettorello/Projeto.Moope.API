using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Repositories.Base;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Core.Interfaces.Repositories
{
    /// <summary>
    /// Interface para repositório de emails
    /// </summary>
    public interface IEmailRepository : IRepository<Email>
    {
        /// <summary>
        /// Busca emails por status
        /// </summary>
        /// <param name="status">Status dos emails</param>
        /// <returns>Lista de emails com o status especificado</returns>
        Task<IEnumerable<Email>> BuscarPorStatusAsync(StatusEmail status);

        /// <summary>
        /// Busca emails pendentes para envio
        /// </summary>
        /// <returns>Lista de emails pendentes</returns>
        Task<IEnumerable<Email>> BuscarPendentesAsync();

        /// <summary>
        /// Busca emails agendados que devem ser enviados
        /// </summary>
        /// <returns>Lista de emails agendados para envio</returns>
        Task<IEnumerable<Email>> BuscarAgendadosParaEnvioAsync();

        /// <summary>
        /// Busca emails por destinatário
        /// </summary>
        /// <param name="destinatario">Email do destinatário</param>
        /// <returns>Lista de emails enviados para o destinatário</returns>
        Task<IEnumerable<Email>> BuscarPorDestinatarioAsync(string destinatario);

        /// <summary>
        /// Busca emails por usuário
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de emails relacionados ao usuário</returns>
        Task<IEnumerable<Email>> BuscarPorUsuarioAsync(Guid usuarioId);

        /// <summary>
        /// Busca emails por cliente
        /// </summary>
        /// <param name="clienteId">ID do cliente</param>
        /// <returns>Lista de emails relacionados ao cliente</returns>
        Task<IEnumerable<Email>> BuscarPorClienteAsync(Guid clienteId);

        /// <summary>
        /// Busca emails por tipo
        /// </summary>
        /// <param name="tipo">Tipo do email</param>
        /// <returns>Lista de emails do tipo especificado</returns>
        Task<IEnumerable<Email>> BuscarPorTipoAsync(string tipo);

        /// <summary>
        /// Busca emails em um período específico
        /// </summary>
        /// <param name="dataInicio">Data de início</param>
        /// <param name="dataFim">Data de fim</param>
        /// <returns>Lista de emails no período</returns>
        Task<IEnumerable<Email>> BuscarPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);

        /// <summary>
        /// Busca emails com falha que podem ser reprocessados
        /// </summary>
        /// <param name="maxTentativas">Número máximo de tentativas</param>
        /// <returns>Lista de emails que falharam mas podem ser reprocessados</returns>
        Task<IEnumerable<Email>> BuscarFalhasParaReprocessarAsync(int maxTentativas = 3);

        /// <summary>
        /// Atualiza o status de um email
        /// </summary>
        /// <param name="id">ID do email</param>
        /// <param name="status">Novo status</param>
        /// <param name="mensagemErro">Mensagem de erro (opcional)</param>
        /// <returns>Task</returns>
        Task AtualizarStatusAsync(Guid id, StatusEmail status, string? mensagemErro = null);

        /// <summary>
        /// Registra tentativa de envio
        /// </summary>
        /// <param name="id">ID do email</param>
        /// <returns>Task</returns>
        Task RegistrarTentativaEnvioAsync(Guid id);

        /// <summary>
        /// Marca email como enviado
        /// </summary>
        /// <param name="id">ID do email</param>
        /// <param name="dataEnvio">Data e hora do envio</param>
        /// <returns>Task</returns>
        Task MarcarComoEnviadoAsync(Guid id, DateTime? dataEnvio = null);

        /// <summary>
        /// Conta emails por status
        /// </summary>
        /// <param name="status">Status a contar</param>
        /// <returns>Número de emails com o status</returns>
        Task<int> ContarPorStatusAsync(StatusEmail status);

        /// <summary>
        /// Busca estatísticas de emails
        /// </summary>
        /// <param name="dataInicio">Data de início (opcional)</param>
        /// <param name="dataFim">Data de fim (opcional)</param>
        /// <returns>Estatísticas de emails</returns>
        Task<EmailEstatisticas> BuscarEstatisticasAsync(DateTime? dataInicio = null, DateTime? dataFim = null);
    }

    /// <summary>
    /// Classe para estatísticas de emails
    /// </summary>
    public class EmailEstatisticas
    {
        public int TotalEmails { get; set; }
        public int EmailsEnviados { get; set; }
        public int EmailsPendentes { get; set; }
        public int EmailsComFalha { get; set; }
        public int EmailsAgendados { get; set; }
        public int EmailsCancelados { get; set; }
        public decimal TaxaSucesso => TotalEmails > 0 ? (decimal)EmailsEnviados / TotalEmails * 100 : 0;
        public decimal TaxaFalha => TotalEmails > 0 ? (decimal)EmailsComFalha / TotalEmails * 100 : 0;
    }
}
