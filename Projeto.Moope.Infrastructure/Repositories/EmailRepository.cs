using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Infrastructure.Data;
using Projeto.Moope.Infrastructure.Repositories.Base;

namespace Projeto.Moope.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação do repositório de emails
    /// </summary>
    public class EmailRepository : Repository<Email>, IEmailRepository
    {
        private readonly AppDbContext _context;
        public EmailRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Busca emails por status
        /// </summary>
        public async Task<IEnumerable<Email>> BuscarPorStatusAsync(StatusEmail status)
        {
            return await _context.Emails
                .Where(e => e.Status == status)
                .OrderBy(e => e.Created)
                .ToListAsync();
        }

        /// <summary>
        /// Busca emails pendentes para envio
        /// </summary>
        public async Task<IEnumerable<Email>> BuscarPendentesAsync()
        {
            return await _context.Emails
                .Where(e => e.Status == StatusEmail.Pendente)
                .OrderBy(e => e.Prioridade)
                .ThenBy(e => e.Created)
                .ToListAsync();
        }

        /// <summary>
        /// Busca emails agendados que devem ser enviados
        /// </summary>
        public async Task<IEnumerable<Email>> BuscarAgendadosParaEnvioAsync()
        {
            var agora = DateTime.UtcNow;
            return await _context.Emails
                .Where(e => e.Status == StatusEmail.Agendado && 
                           e.DataProgramada.HasValue && 
                           e.DataProgramada.Value <= agora)
                .OrderBy(e => e.Prioridade)
                .ThenBy(e => e.DataProgramada)
                .ToListAsync();
        }

        /// <summary>
        /// Busca emails por destinatário
        /// </summary>
        public async Task<IEnumerable<Email>> BuscarPorDestinatarioAsync(string destinatario)
        {
            return await _context.Emails
                .Where(e => e.Destinatario.ToLower() == destinatario.ToLower())
                .OrderByDescending(e => e.Created)
                .ToListAsync();
        }

        /// <summary>
        /// Busca emails por usuário
        /// </summary>
        public async Task<IEnumerable<Email>> BuscarPorUsuarioAsync(Guid usuarioId)
        {
            return await _context.Emails
                .Where(e => e.UsuarioId == usuarioId)
                .OrderByDescending(e => e.Created)
                .ToListAsync();
        }

        /// <summary>
        /// Busca emails por cliente
        /// </summary>
        public async Task<IEnumerable<Email>> BuscarPorClienteAsync(Guid clienteId)
        {
            return await _context.Emails
                .Where(e => e.ClienteId == clienteId)
                .OrderByDescending(e => e.Created)
                .ToListAsync();
        }

        /// <summary>
        /// Busca emails por tipo
        /// </summary>
        public async Task<IEnumerable<Email>> BuscarPorTipoAsync(string tipo)
        {
            return await _context.Emails
                .Where(e => e.Tipo == tipo)
                .OrderByDescending(e => e.Created)
                .ToListAsync();
        }

        /// <summary>
        /// Busca emails em um período específico
        /// </summary>
        public async Task<IEnumerable<Email>> BuscarPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _context.Emails
                .Where(e => e.Created >= dataInicio && e.Created <= dataFim)
                .OrderByDescending(e => e.Created)
                .ToListAsync();
        }

        /// <summary>
        /// Busca emails com falha que podem ser reprocessados
        /// </summary>
        public async Task<IEnumerable<Email>> BuscarFalhasParaReprocessarAsync(int maxTentativas = 3)
        {
            return await _context.Emails
                .Where(e => e.Status == StatusEmail.Falha && 
                           e.TentativasEnvio < maxTentativas)
                .OrderBy(e => e.UltimaTentativa)
                .ToListAsync();
        }

        /// <summary>
        /// Atualiza o status de um email
        /// </summary>
        public async Task AtualizarStatusAsync(Guid id, StatusEmail status, string? mensagemErro = null)
        {
            var email = await _context.Emails.FindAsync(id);
            if (email != null)
            {
                email.Status = status;
                email.Updated = DateTime.UtcNow;
                
                if (!string.IsNullOrEmpty(mensagemErro))
                {
                    email.MensagemErro = mensagemErro;
                }

                if (status == StatusEmail.Enviado)
                {
                    email.DataEnvio = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Registra tentativa de envio
        /// </summary>
        public async Task RegistrarTentativaEnvioAsync(Guid id)
        {
            var email = await _context.Emails.FindAsync(id);
            if (email != null)
            {
                email.TentativasEnvio++;
                email.UltimaTentativa = DateTime.UtcNow;
                email.Status = StatusEmail.Processando;
                email.Updated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Marca email como enviado
        /// </summary>
        public async Task MarcarComoEnviadoAsync(Guid id, DateTime? dataEnvio = null)
        {
            var email = await _context.Emails.FindAsync(id);
            if (email != null)
            {
                email.Status = StatusEmail.Enviado;
                email.DataEnvio = dataEnvio ?? DateTime.UtcNow;
                email.Updated = DateTime.UtcNow;
                email.MensagemErro = null; // Limpar erro anterior

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Conta emails por status
        /// </summary>
        public async Task<int> ContarPorStatusAsync(StatusEmail status)
        {
            return await _context.Emails.CountAsync(e => e.Status == status);
        }

        /// <summary>
        /// Busca estatísticas de emails
        /// </summary>
        public async Task<EmailEstatisticas> BuscarEstatisticasAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            var query = _context.Emails.AsQueryable();

            if (dataInicio.HasValue)
            {
                query = query.Where(e => e.Created >= dataInicio.Value);
            }

            if (dataFim.HasValue)
            {
                query = query.Where(e => e.Created <= dataFim.Value);
            }

            var estatisticas = await query
                .GroupBy(e => 1) // Agrupa todos os registros
                .Select(g => new EmailEstatisticas
                {
                    TotalEmails = g.Count(),
                    EmailsEnviados = g.Count(e => e.Status == StatusEmail.Enviado),
                    EmailsPendentes = g.Count(e => e.Status == StatusEmail.Pendente),
                    EmailsComFalha = g.Count(e => e.Status == StatusEmail.Falha),
                    EmailsAgendados = g.Count(e => e.Status == StatusEmail.Agendado),
                    EmailsCancelados = g.Count(e => e.Status == StatusEmail.Cancelado)
                })
                .FirstOrDefaultAsync();

            return estatisticas ?? new EmailEstatisticas();
        }
    }
}
