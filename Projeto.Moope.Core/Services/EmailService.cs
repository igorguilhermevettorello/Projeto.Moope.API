using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Interfaces.UnitOfWork;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Services.Base;
using System.Net;
using System.Net.Mail;

namespace Projeto.Moope.Core.Services
{
    public class EmailService : BaseService, IEmailService
    {
        private readonly IEmailRepository _emailRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IEmailRepository emailRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<EmailService> logger,
            INotificador notificador) : base(notificador)
        {
            _emailRepository = emailRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> EnviarEmailAsync(Email email)
        {
            try
            {
                if (email == null)
                {
                    Notificar("Email", "Email não pode ser nulo");
                    return false;
                }

                // Valida o email
                if (!ValidarEmail(email))
                {
                    return false;
                }

                // Atualiza status para processando
                email.Status = StatusEmail.Processando;
                email.UltimaTentativa = DateTime.UtcNow;
                email.TentativasEnvio++;

                await _emailRepository.AtualizarAsync(email);
                await _unitOfWork.CommitAsync();

                // Tenta enviar o email
                var enviado = await EnviarEmailSmtpAsync(email);

                if (enviado)
                {
                    email.Status = StatusEmail.Enviado;
                    email.DataEnvio = DateTime.UtcNow;
                    email.MensagemErro = null;
                    _logger.LogInformation("Email enviado com sucesso para {Destinatario}. ID: {EmailId}", 
                        email.Destinatario, email.Id);
                }
                else
                {
                    email.Status = StatusEmail.Falha;
                    email.MensagemErro = "Falha no envio via SMTP";
                    _logger.LogWarning("Falha no envio de email para {Destinatario}. ID: {EmailId}", 
                        email.Destinatario, email.Id);
                }

                await _emailRepository.AtualizarAsync(email);
                await _unitOfWork.CommitAsync();

                return enviado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email ID: {EmailId}", email?.Id);
                
                if (email != null)
                {
                    email.Status = StatusEmail.Falha;
                    email.MensagemErro = ex.Message;
                    await _emailRepository.AtualizarAsync(email);
                    await _unitOfWork.CommitAsync();
                }

                return false;
            }
        }

        public async Task<bool> EnviarEmailSimplesAsync(string destinatario, string assunto, string corpo, bool ehHtml = true)
        {
            try
            {
                var configuracaoEmail = ObterConfiguracaoEmail();
                
                var email = new Email
                {
                    Remetente = configuracaoEmail.RemetenteEmail,
                    NomeRemetente = configuracaoEmail.RemetenteNome,
                    Destinatario = destinatario,
                    Assunto = assunto,
                    Corpo = corpo,
                    EhHtml = ehHtml,
                    Prioridade = Prioridade.Normal,
                    Status = StatusEmail.Pendente,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                };

                await _emailRepository.SalvarAsync(email);
                await _unitOfWork.CommitAsync();

                return await EnviarEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email simples para {Destinatario}", destinatario);
                Notificar("Email", $"Erro ao enviar email: {ex.Message}");
                return false;
            }
        }

        public async Task<int> ProcessarEmailsPendentesAsync()
        {
            try
            {
                var emailsPendentes = await _emailRepository.BuscarPendentesAsync();
                var processados = 0;

                foreach (var email in emailsPendentes)
                {
                    var enviado = await EnviarEmailAsync(email);
                    if (enviado)
                    {
                        processados++;
                    }
                }

                _logger.LogInformation("Processados {ProcessadosCount} emails pendentes de {TotalCount}", 
                    processados, emailsPendentes.Count());

                return processados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar emails pendentes");
                return 0;
            }
        }

        public async Task<int> ProcessarEmailsAgendadosAsync()
        {
            try
            {
                var emailsAgendados = await _emailRepository.BuscarAgendadosParaEnvioAsync();
                var processados = 0;

                foreach (var email in emailsAgendados)
                {
                    email.Status = StatusEmail.Pendente;
                    await _emailRepository.AtualizarAsync(email);
                    
                    var enviado = await EnviarEmailAsync(email);
                    if (enviado)
                    {
                        processados++;
                    }
                }

                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Processados {ProcessadosCount} emails agendados de {TotalCount}", 
                    processados, emailsAgendados.Count());

                return processados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar emails agendados");
                return 0;
            }
        }

        public async Task<int> ReprocessarEmailsComFalhaAsync(int maxTentativas = 3)
        {
            try
            {
                var emailsComFalha = await _emailRepository.BuscarFalhasParaReprocessarAsync(maxTentativas);
                var reprocessados = 0;

                foreach (var email in emailsComFalha)
                {
                    email.Status = StatusEmail.Pendente;
                    email.MensagemErro = null;
                    await _emailRepository.AtualizarAsync(email);
                    
                    var enviado = await EnviarEmailAsync(email);
                    if (enviado)
                    {
                        reprocessados++;
                    }
                }

                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Reprocessados {ReprocessadosCount} emails com falha de {TotalCount}", 
                    reprocessados, emailsComFalha.Count());

                return reprocessados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao reprocessar emails com falha");
                return 0;
            }
        }

        public async Task<bool> ValidarConfiguracaoAsync()
        {
            try
            {
                var configuracao = ObterConfiguracaoEmail();
                
                if (string.IsNullOrEmpty(configuracao.SmtpHost) ||
                    configuracao.SmtpPort <= 0 ||
                    string.IsNullOrEmpty(configuracao.Usuario) ||
                    string.IsNullOrEmpty(configuracao.Senha) ||
                    string.IsNullOrEmpty(configuracao.RemetenteEmail))
                {
                    return false;
                }

                // Testa conexão SMTP
                using var client = new SmtpClient(configuracao.SmtpHost, configuracao.SmtpPort)
                {
                    EnableSsl = configuracao.UsarSsl,
                    Credentials = new NetworkCredential(configuracao.Usuario, configuracao.Senha)
                };

                await client.SendMailAsync(new MailMessage());
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidarEmail(Email email)
        {
            if (string.IsNullOrEmpty(email.Destinatario))
            {
                Notificar("Destinatario", "Destinatário é obrigatório");
                return false;
            }

            if (string.IsNullOrEmpty(email.Assunto))
            {
                Notificar("Assunto", "Assunto é obrigatório");
                return false;
            }

            if (string.IsNullOrEmpty(email.Corpo))
            {
                Notificar("Corpo", "Corpo do email é obrigatório");
                return false;
            }

            if (!IsValidEmail(email.Destinatario))
            {
                Notificar("Destinatario", "Email do destinatário não é válido");
                return false;
            }

            if (!string.IsNullOrEmpty(email.Remetente) && !IsValidEmail(email.Remetente))
            {
                Notificar("Remetente", "Email do remetente não é válido");
                return false;
            }

            return true;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> EnviarEmailSmtpAsync(Email email)
        {
            try
            {
                var configuracao = ObterConfiguracaoEmail();

                using var client = new SmtpClient(configuracao.SmtpHost, configuracao.SmtpPort)
                {
                    EnableSsl = configuracao.UsarSsl,
                    Credentials = new NetworkCredential(configuracao.Usuario, configuracao.Senha)
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(email.Remetente, email.NomeRemetente),
                    Subject = email.Assunto,
                    Body = email.Corpo,
                    IsBodyHtml = email.EhHtml
                };

                mailMessage.To.Add(new MailAddress(email.Destinatario, email.NomeDestinatario));

                // Adiciona cópias se especificadas
                if (!string.IsNullOrEmpty(email.Copia))
                {
                    var copias = email.Copia.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var copia in copias)
                    {
                        if (IsValidEmail(copia.Trim()))
                        {
                            mailMessage.CC.Add(copia.Trim());
                        }
                    }
                }

                // Adiciona cópias ocultas se especificadas
                if (!string.IsNullOrEmpty(email.CopiaOculta))
                {
                    var copiasOcultas = email.CopiaOculta.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var copiaOculta in copiasOcultas)
                    {
                        if (IsValidEmail(copiaOculta.Trim()))
                        {
                            mailMessage.Bcc.Add(copiaOculta.Trim());
                        }
                    }
                }

                // Define prioridade
                mailMessage.Priority = email.Prioridade switch
                {
                    Prioridade.Baixa => MailPriority.Low,
                    Prioridade.Alta => MailPriority.High,
                    Prioridade.Urgente => MailPriority.High,
                    _ => MailPriority.Normal
                };

                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no envio SMTP para {Destinatario}", email.Destinatario);
                return false;
            }
        }

        private EmailConfiguracao ObterConfiguracaoEmail()
        {
            return new EmailConfiguracao
            {
                SmtpHost = _configuration["Email:SmtpHost"] ?? "localhost",
                SmtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                UsarSsl = bool.Parse(_configuration["Email:UsarSsl"] ?? "true"),
                Usuario = _configuration["Email:Usuario"] ?? "",
                Senha = _configuration["Email:Senha"] ?? "",
                RemetenteEmail = _configuration["Email:RemetenteEmail"] ?? "noreply@moope.com",
                RemetenteNome = _configuration["Email:RemetenteNome"] ?? "Sistema Moope"
            };
        }
    }

    public class EmailConfiguracao
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public bool UsarSsl { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string RemetenteEmail { get; set; } = string.Empty;
        public string RemetenteNome { get; set; } = string.Empty;
    }
}
