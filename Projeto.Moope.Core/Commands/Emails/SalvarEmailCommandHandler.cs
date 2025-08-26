using Projeto.Moope.Core.Commands.Base;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Notifications;

namespace Projeto.Moope.Core.Commands.Emails
{
    /// <summary>
    /// Handler para processar o comando de salvar email
    /// </summary>
    public class SalvarEmailCommandHandler : ICommandHandler<SalvarEmailCommand, Result<Guid>>
    {
        private readonly IEmailRepository _emailRepository;
        private readonly IUsuarioService _usuarioService;
        private readonly IClienteService _clienteService;
        private readonly IEmailService _emailService;
        private readonly INotificador _notificador;

        public SalvarEmailCommandHandler(
            IEmailRepository emailRepository,
            IUsuarioService usuarioService,
            IClienteService clienteService,
            IEmailService emailService,
            INotificador notificador)
        {
            _emailRepository = emailRepository;
            _usuarioService = usuarioService;
            _clienteService = clienteService;
            _emailService = emailService;
            _notificador = notificador;
        }

        public async Task<Result<Guid>> Handle(SalvarEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validar dados relacionados
                var validacao = await ValidarDadosRelacionados(request);
                if (!validacao.IsValid)
                {
                    foreach (var erro in validacao.Errors)
                    {
                        _notificador.Handle(new Notificacao
                        {
                            Campo = erro.Campo,
                            Mensagem = erro.Mensagem
                        });
                    }

                    return new Result<Guid>
                    {
                        Status = false,
                        Mensagem = "Dados inválidos para criação do email"
                    };
                }

                // Criar entidade Email
                var email = CriarEmail(request);

                // Definir status inicial
                email.Status = DeterminarStatusInicial(request);

                // Salvar no banco
                await _emailRepository.SalvarAsync(email);

                // Se configurado para envio imediato, processar envio
                if (request.EnviarImediatamente && email.Status == StatusEmail.Pendente)
                {
                    await ProcessarEnvioImediato(email);
                }

                return new Result<Guid>
                {
                    Status = true,
                    Mensagem = "Email salvo com sucesso",
                    Dados = email.Id
                };
            }
            catch (Exception ex)
            {
                _notificador.Handle(new Notificacao
                {
                    Campo = "Erro",
                    Mensagem = $"Erro ao salvar email: {ex.Message}"
                });

                return new Result<Guid>
                {
                    Status = false,
                    Mensagem = "Erro interno ao salvar email"
                };
            }
        }

        #region Métodos Privados

        private async Task<ValidationResult> ValidarDadosRelacionados(SalvarEmailCommand request)
        {
            var result = new ValidationResult();

            // Validar usuário se informado
            if (request.UsuarioId.HasValue)
            {
                var usuario = await _usuarioService.BuscarPorIdAsNotrackingAsync(request.UsuarioId.Value);
                if (usuario == null)
                {
                    result.AddError("UsuarioId", "Usuário não encontrado");
                }
            }

            // Validar cliente se informado
            if (request.ClienteId.HasValue)
            {
                var cliente = await _clienteService.BuscarPorIdAsNotrackingAsync(request.ClienteId.Value);
                if (cliente == null)
                {
                    result.AddError("ClienteId", "Cliente não encontrado");
                }
            }

            // Validar emails de cópia se informados
            if (!string.IsNullOrWhiteSpace(request.Copia))
            {
                if (!ValidarListaEmails(request.Copia))
                {
                    result.AddError("Copia", "Um ou mais emails de cópia são inválidos");
                }
            }

            // Validar emails de cópia oculta se informados
            if (!string.IsNullOrWhiteSpace(request.CopiaOculta))
            {
                if (!ValidarListaEmails(request.CopiaOculta))
                {
                    result.AddError("CopiaOculta", "Um ou mais emails de cópia oculta são inválidos");
                }
            }

            // Validar data programada
            if (request.DataProgramada.HasValue && request.DataProgramada.Value <= DateTime.UtcNow)
            {
                result.AddError("DataProgramada", "Data programada deve ser no futuro");
            }

            return result;
        }

        private bool ValidarListaEmails(string emails)
        {
            var listaEmails = emails.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return listaEmails.All(email => IsValidEmail(email.Trim()));
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private Email CriarEmail(SalvarEmailCommand request)
        {
            return new Email
            {
                Remetente = request.Remetente,
                NomeRemetente = request.NomeRemetente,
                Destinatario = request.Destinatario,
                NomeDestinatario = request.NomeDestinatario,
                Copia = request.Copia,
                CopiaOculta = request.CopiaOculta,
                Assunto = request.Assunto,
                Corpo = request.Corpo,
                EhHtml = request.EhHtml,
                Prioridade = request.Prioridade,
                UsuarioId = request.UsuarioId,
                ClienteId = request.ClienteId,
                Tipo = request.Tipo,
                DadosAdicionais = request.DadosAdicionais,
                DataProgramada = request.DataProgramada,
                TentativasEnvio = 0,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
        }

        private StatusEmail DeterminarStatusInicial(SalvarEmailCommand request)
        {
            if (request.DataProgramada.HasValue && request.DataProgramada.Value > DateTime.UtcNow)
            {
                return StatusEmail.Agendado;
            }

            return StatusEmail.Pendente;
        }

        private async Task ProcessarEnvioImediato(Email email)
        {
            try
            {
                // Registrar tentativa de envio
                await _emailRepository.RegistrarTentativaEnvioAsync(email.Id);

                // Tentar enviar via service de email (se existir)
                if (_emailService != null)
                {
                    var sucesso = await _emailService.EnviarEmailAsync(email);
                    
                    if (sucesso)
                    {
                        await _emailRepository.MarcarComoEnviadoAsync(email.Id);
                    }
                    else
                    {
                        await _emailRepository.AtualizarStatusAsync(email.Id, StatusEmail.Falha, "Falha no envio imediato");
                    }
                }
                else
                {
                    // Se não há service de email, manter como pendente para processamento posterior
                    await _emailRepository.AtualizarStatusAsync(email.Id, StatusEmail.Pendente);
                }
            }
            catch (Exception ex)
            {
                await _emailRepository.AtualizarStatusAsync(email.Id, StatusEmail.Falha, ex.Message);
            }
        }

        #endregion
    }

    /// <summary>
    /// Classe auxiliar para validação
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<ValidationError> Errors { get; } = new();

        public void AddError(string campo, string mensagem)
        {
            Errors.Add(new ValidationError { Campo = campo, Mensagem = mensagem });
        }
    }

    public class ValidationError
    {
        public string Campo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
    }
}
