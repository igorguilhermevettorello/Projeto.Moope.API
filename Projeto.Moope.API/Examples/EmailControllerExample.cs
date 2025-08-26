using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.API.Controllers.Base;
using Projeto.Moope.Core.Commands.Emails;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.API.Examples
{
    /// <summary>
    /// Exemplo de Controller para gerenciamento de emails
    /// Este controller demonstra como usar o sistema de emails implementado
    /// </summary>
    [ApiController]
    [Route("api/examples/email")]
    [Authorize]
    public class EmailControllerExample : MainController
    {
        private readonly IMediator _mediator;
        private readonly IEmailRepository _emailRepository;

        public EmailControllerExample(
            IMediator mediator,
            IEmailRepository emailRepository,
            INotificador notificador,
            IUser user) : base(notificador, user)
        {
            _mediator = mediator;
            _emailRepository = emailRepository;
        }

        /// <summary>
        /// Envia um email simples
        /// </summary>
        [HttpPost("enviar")]
        public async Task<IActionResult> EnviarEmail([FromBody] EnviarEmailRequest request)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            try
            {
                var command = new SalvarEmailCommand
                {
                    Remetente = "sistema@moope.com.br",
                    NomeRemetente = "Sistema Moope",
                    Destinatario = request.Destinatario,
                    NomeDestinatario = request.NomeDestinatario,
                    Assunto = request.Assunto,
                    Corpo = request.Corpo,
                    EhHtml = request.EhHtml,
                    Prioridade = request.Prioridade,
                    Tipo = request.Tipo ?? "GERAL",
                    UsuarioId = UsuarioId,
                    EnviarImediatamente = request.EnviarImediatamente
                };

                var resultado = await _mediator.Send(command);

                if (!resultado.Status)
                    return CustomResponse();

                return Ok(new { 
                    emailId = resultado.Dados, 
                    mensagem = "Email salvo com sucesso",
                    status = request.EnviarImediatamente ? "Enviado" : "Pendente"
                });
            }
            catch (Exception ex)
            {
                NotificarErro("Erro", ex.Message);
                return CustomResponse();
            }
        }

        /// <summary>
        /// Envia email de boas-vindas
        /// </summary>
        [HttpPost("boas-vindas")]
        public async Task<IActionResult> EnviarBoasVindas([FromBody] BoasVindasRequest request)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            try
            {
                var corpoEmail = CriarTemplateBoasVindas(request.Nome, request.SenhaTemporaria);

                var command = new SalvarEmailCommand
                {
                    Remetente = "noreply@moope.com.br",
                    NomeRemetente = "Equipe Moope",
                    Destinatario = request.Email,
                    NomeDestinatario = request.Nome,
                    Assunto = "Bem-vindo ao Sistema Moope!",
                    Corpo = corpoEmail,
                    EhHtml = true,
                    Prioridade = Prioridade.Alta,
                    Tipo = "BOAS_VINDAS",
                    ClienteId = request.ClienteId,
                    EnviarImediatamente = true
                };

                var resultado = await _mediator.Send(command);

                if (!resultado.Status)
                    return CustomResponse();

                return Ok(new { 
                    emailId = resultado.Dados, 
                    mensagem = "Email de boas-vindas enviado com sucesso" 
                });
            }
            catch (Exception ex)
            {
                NotificarErro("Erro", ex.Message);
                return CustomResponse();
            }
        }

        /// <summary>
        /// Agendar email promocional
        /// </summary>
        [HttpPost("agendar-promocional")]
        public async Task<IActionResult> AgendarPromocional([FromBody] EmailPromocionalRequest request)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            try
            {
                var command = new SalvarEmailCommand
                {
                    Remetente = "marketing@moope.com.br",
                    NomeRemetente = "Marketing Moope",
                    Destinatario = request.Destinatario,
                    NomeDestinatario = request.NomeDestinatario,
                    Assunto = request.Assunto,
                    Corpo = request.CorpoHtml,
                    EhHtml = true,
                    Prioridade = Prioridade.Normal,
                    Tipo = "PROMOCIONAL",
                    DataProgramada = request.DataEnvio,
                    EnviarImediatamente = false
                };

                var resultado = await _mediator.Send(command);

                if (!resultado.Status)
                    return CustomResponse();

                return Ok(new { 
                    emailId = resultado.Dados, 
                    mensagem = "Email promocional agendado com sucesso",
                    dataEnvio = request.DataEnvio
                });
            }
            catch (Exception ex)
            {
                NotificarErro("Erro", ex.Message);
                return CustomResponse();
            }
        }

        /// <summary>
        /// Buscar emails por status
        /// </summary>
        [HttpGet("por-status/{status}")]
        public async Task<IActionResult> BuscarPorStatus(StatusEmail status)
        {
            try
            {
                var emails = await _emailRepository.BuscarPorStatusAsync(status);
                
                var resultado = emails.Select(e => new
                {
                    id = e.Id,
                    destinatario = e.Destinatario,
                    assunto = e.Assunto,
                    status = e.Status.ToString(),
                    prioridade = e.Prioridade.ToString(),
                    tentativas = e.TentativasEnvio,
                    criado = e.Created,
                    ultimaTentativa = e.UltimaTentativa,
                    dataEnvio = e.DataEnvio,
                    tipo = e.Tipo
                });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                NotificarErro("Erro", ex.Message);
                return CustomResponse();
            }
        }

        /// <summary>
        /// Buscar estatísticas de emails
        /// </summary>
        [HttpGet("estatisticas")]
        public async Task<IActionResult> BuscarEstatisticas([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
        {
            try
            {
                var estatisticas = await _emailRepository.BuscarEstatisticasAsync(inicio, fim);
                
                return Ok(new
                {
                    periodo = new { inicio, fim },
                    total = estatisticas.TotalEmails,
                    enviados = estatisticas.EmailsEnviados,
                    pendentes = estatisticas.EmailsPendentes,
                    falhas = estatisticas.EmailsComFalha,
                    agendados = estatisticas.EmailsAgendados,
                    cancelados = estatisticas.EmailsCancelados,
                    taxaSucesso = $"{estatisticas.TaxaSucesso:F2}%",
                    taxaFalha = $"{estatisticas.TaxaFalha:F2}%"
                });
            }
            catch (Exception ex)
            {
                NotificarErro("Erro", ex.Message);
                return CustomResponse();
            }
        }

        /// <summary>
        /// Buscar emails por cliente
        /// </summary>
        [HttpGet("cliente/{clienteId:guid}")]
        public async Task<IActionResult> BuscarPorCliente(Guid clienteId)
        {
            try
            {
                var emails = await _emailRepository.BuscarPorClienteAsync(clienteId);
                
                var resultado = emails.Select(e => new
                {
                    id = e.Id,
                    assunto = e.Assunto,
                    status = e.Status.ToString(),
                    tipo = e.Tipo,
                    criado = e.Created,
                    enviado = e.DataEnvio
                });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                NotificarErro("Erro", ex.Message);
                return CustomResponse();
            }
        }

        /// <summary>
        /// Reprocessar emails com falha
        /// </summary>
        [HttpPost("reprocessar-falhas")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ReprocessarFalhas([FromQuery] int maxTentativas = 3)
        {
            try
            {
                var emailsComFalha = await _emailRepository.BuscarFalhasParaReprocessarAsync(maxTentativas);
                int reprocessados = 0;

                foreach (var email in emailsComFalha)
                {
                    await _emailRepository.AtualizarStatusAsync(email.Id, StatusEmail.Pendente);
                    reprocessados++;
                }

                return Ok(new { 
                    reprocessados,
                    mensagem = $"{reprocessados} emails marcados para reprocessamento"
                });
            }
            catch (Exception ex)
            {
                NotificarErro("Erro", ex.Message);
                return CustomResponse();
            }
        }

        /// <summary>
        /// Cancelar email pendente ou agendado
        /// </summary>
        [HttpPut("cancelar/{emailId:guid}")]
        public async Task<IActionResult> CancelarEmail(Guid emailId)
        {
            try
            {
                await _emailRepository.AtualizarStatusAsync(emailId, StatusEmail.Cancelado, "Cancelado pelo usuário");
                
                return Ok(new { mensagem = "Email cancelado com sucesso" });
            }
            catch (Exception ex)
            {
                NotificarErro("Erro", ex.Message);
                return CustomResponse();
            }
        }

        #region Métodos Auxiliares

        private string CriarTemplateBoasVindas(string nome, string senhaTemporaria)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        .container {{ max-width: 600px; margin: 0 auto; font-family: Arial, sans-serif; }}
                        .header {{ background: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .senha {{ background: #f8f9fa; padding: 15px; border-left: 4px solid #007bff; margin: 20px 0; }}
                        .footer {{ background: #f8f9fa; padding: 15px; text-align: center; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Bem-vindo ao Sistema Moope!</h1>
                        </div>
                        <div class='content'>
                            <p>Olá <strong>{nome}</strong>,</p>
                            <p>Sua conta foi criada com sucesso em nosso sistema.</p>
                            
                            <div class='senha'>
                                <strong>Sua senha temporária:</strong><br/>
                                <code style='font-size: 18px;'>{senhaTemporaria}</code>
                            </div>
                            
                            <p><strong>Importante:</strong> Por favor, altere sua senha no primeiro acesso por questões de segurança.</p>
                            
                            <p>Se você não se cadastrou em nosso sistema, entre em contato conosco imediatamente.</p>
                        </div>
                        <div class='footer'>
                            <p>Atenciosamente,<br/>Equipe Moope</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        #endregion

        #region DTOs

        public class EnviarEmailRequest
        {
            [Required] [EmailAddress] public string Destinatario { get; set; } = string.Empty;
            public string? NomeDestinatario { get; set; }
            [Required] public string Assunto { get; set; } = string.Empty;
            [Required] public string Corpo { get; set; } = string.Empty;
            public bool EhHtml { get; set; } = true;
            public Prioridade Prioridade { get; set; } = Prioridade.Normal;
            public string? Tipo { get; set; }
            public bool EnviarImediatamente { get; set; } = true;
        }

        public class BoasVindasRequest
        {
            [Required] public string Nome { get; set; } = string.Empty;
            [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
            [Required] public string SenhaTemporaria { get; set; } = string.Empty;
            public Guid? ClienteId { get; set; }
        }

        public class EmailPromocionalRequest
        {
            [Required] [EmailAddress] public string Destinatario { get; set; } = string.Empty;
            public string? NomeDestinatario { get; set; }
            [Required] public string Assunto { get; set; } = string.Empty;
            [Required] public string CorpoHtml { get; set; } = string.Empty;
            [Required] public DateTime DataEnvio { get; set; }
        }

        #endregion
    }
}
