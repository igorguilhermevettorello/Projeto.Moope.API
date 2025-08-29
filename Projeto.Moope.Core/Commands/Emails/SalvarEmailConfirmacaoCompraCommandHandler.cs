using Projeto.Moope.Core.Commands.Base;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Notifications;
using System.Text.Json;
using Projeto.Moope.Core.Utils;

namespace Projeto.Moope.Core.Commands.Emails
{
    /// <summary>
    /// Handler para processar o comando de salvar email de confirmação de compra
    /// </summary>
    public class SalvarEmailConfirmacaoCompraCommandHandler : ICommandHandler<SalvarEmailConfirmacaoCompraCommand, Result<Guid>>
    {
        private readonly IEmailRepository _emailRepository;
        private readonly IClienteService _clienteService;
        private readonly IPlanoService _planoService;
        private readonly IVendedorService _vendedorService;
        private readonly INotificador _notificador;

        public SalvarEmailConfirmacaoCompraCommandHandler(
            IEmailRepository emailRepository,
            IClienteService clienteService,
            IPlanoService planoService,
            IVendedorService vendedorService,
            INotificador notificador)
        {
            _emailRepository = emailRepository;
            _clienteService = clienteService;
            _planoService = planoService;
            _vendedorService = vendedorService;
            _notificador = notificador;
        }

        public async Task<Result<Guid>> Handle(SalvarEmailConfirmacaoCompraCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validar se o cliente existe
                var cliente = await _clienteService.BuscarPorIdAsNotrackingAsync(request.ClienteId);
                if (cliente == null)
                {
                    _notificador.Handle(new Notificacao
                    {
                        Campo = "ClienteId",
                        Mensagem = "Cliente não encontrado"
                    });

                    return new Result<Guid>
                    {
                        Status = false,
                        Mensagem = "Cliente não encontrado"
                    };
                }

                // Validar se o plano existe
                var plano = await _planoService.BuscarPorIdAsNotrackingAsync(request.PlanoId);
                if (plano == null)
                {
                    _notificador.Handle(new Notificacao
                    {
                        Campo = "PlanoId",
                        Mensagem = "Plano não encontrado"
                    });

                    return new Result<Guid>
                    {
                        Status = false,
                        Mensagem = "Plano não encontrado"
                    };
                }

                // Validar vendedor se informado
                if (request.VendedorId.HasValue)
                {
                    var vendedor = await _vendedorService.BuscarPorIdAsNotrackingAsync(request.VendedorId.Value);
                    if (vendedor == null)
                    {
                        _notificador.Handle(new Notificacao
                        {
                            Campo = "VendedorId",
                            Mensagem = "Vendedor não encontrado"
                        });

                        return new Result<Guid>
                        {
                            Status = false,
                            Mensagem = "Vendedor não encontrado"
                        };
                    }
                }

                // Criar o email de confirmação
                var email = await CriarEmailConfirmacaoCompra(request, cliente, plano);

                // Salvar o email no banco
                await _emailRepository.SalvarAsync(email);

                return new Result<Guid>
                {
                    Status = true,
                    Mensagem = "Email de confirmação de compra salvo com sucesso",
                    Dados = email.Id
                };
            }
            catch (Exception ex)
            {
                _notificador.Handle(new Notificacao
                {
                    Campo = "Erro",
                    Mensagem = $"Erro ao salvar email de confirmação: {ex.Message}"
                });

                return new Result<Guid>
                {
                    Status = false,
                    Mensagem = "Erro interno ao salvar email de confirmação"
                };
            }
        }

        private async Task<Email> CriarEmailConfirmacaoCompra(
            SalvarEmailConfirmacaoCompraCommand request, 
            Cliente cliente, 
            Plano plano)
        {
            // Gerar assunto do email
            var assunto = request.ClienteNovo 
                ? $"Bem-vindo! Confirmação de compra - {plano.Descricao}"
                : $"Confirmação de compra - {plano.Descricao}";

            // Gerar corpo do email
            var corpo = await GerarCorpoEmail(request, cliente, plano);

            // Criar dados adicionais em JSON
            var dadosAdicionais = new
            {
                PedidoId = request.PedidoId,
                PlanoId = request.PlanoId,
                ValorTotal = request.ValorTotal,
                Quantidade = request.Quantidade,
                StatusAssinatura = request.StatusAssinatura.ToString(),
                StatusPagamento = request.StatusPagamento?.ToString(),
                ClienteNovo = request.ClienteNovo,
                DataCompra = request.DataCompra,
                VendedorId = request.VendedorId,
                NomeVendedor = request.NomeVendedor
            };

            var email = new Email
            {
                Remetente = "noreply@moope.com.br", // Configurar email padrão
                NomeRemetente = "Moope",
                Destinatario = request.EmailCliente,
                NomeDestinatario = request.NomeCliente,
                Assunto = assunto,
                Corpo = corpo,
                EhHtml = true,
                Prioridade = Prioridade.Normal,
                Status = StatusEmail.Pendente,
                ClienteId = request.ClienteId,
                Tipo = "CONFIRMACAO_COMPRA",
                DadosAdicionais = JsonSerializer.Serialize(dadosAdicionais),
                TentativasEnvio = 0,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            return email;
        }

        private async Task<string> GerarCorpoEmail(
            SalvarEmailConfirmacaoCompraCommand request, 
            Cliente cliente, 
            Plano plano)
        {
            var statusAssinaturaDescricao = ObterDescricaoStatusAssinatura(request.StatusAssinatura);
            var statusPagamentoDescricao = request.StatusPagamento.HasValue 
                ? ObterDescricaoStatusPagamento(request.StatusPagamento.Value) 
                : "Não informado";

            var corpo = new System.Text.StringBuilder();

            // Cabeçalho
            corpo.AppendLine("<!DOCTYPE html>");
            corpo.AppendLine("<html>");
            corpo.AppendLine("<head>");
            corpo.AppendLine("<meta charset='utf-8'>");
            corpo.AppendLine("<title>Confirmação de Compra</title>");
            corpo.AppendLine("<style>");
            corpo.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            corpo.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            corpo.AppendLine(".header { background-color: #007bff; color: white; padding: 20px; text-align: center; }");
            corpo.AppendLine(".content { padding: 20px; background-color: #f8f9fa; }");
            corpo.AppendLine(".info-box { background-color: white; padding: 15px; margin: 10px 0; border-left: 4px solid #007bff; }");
            corpo.AppendLine(".footer { text-align: center; padding: 20px; color: #666; font-size: 12px; }");
            corpo.AppendLine(".status-success { color: #28a745; font-weight: bold; }");
            corpo.AppendLine(".status-pending { color: #ffc107; font-weight: bold; }");
            corpo.AppendLine(".status-error { color: #dc3545; font-weight: bold; }");
            corpo.AppendLine("</style>");
            corpo.AppendLine("</head>");
            corpo.AppendLine("<body>");
            corpo.AppendLine("<div class='container'>");

            // Cabeçalho
            corpo.AppendLine("<div class='header'>");
            corpo.AppendLine("<h1>Moope</h1>");
            if (request.ClienteNovo)
            {
                corpo.AppendLine("<h2>Bem-vindo! Sua compra foi processada</h2>");
            }
            else
            {
                corpo.AppendLine("<h2>Confirmação de Compra</h2>");
            }
            corpo.AppendLine("</div>");

            // Conteúdo principal
            corpo.AppendLine("<div class='content'>");

            // Saudação
            corpo.AppendLine($"<p>Olá <strong>{request.NomeCliente}</strong>,</p>");

            if (request.ClienteNovo)
            {
                corpo.AppendLine("<p>Seja bem-vindo ao Moope! Sua conta foi criada com sucesso e sua compra foi processada.</p>");
            }
            else
            {
                corpo.AppendLine("<p>Obrigado por escolher nossos serviços novamente! Sua compra foi processada com sucesso.</p>");
            }

            // Dados da compra
            corpo.AppendLine("<div class='info-box'>");
            corpo.AppendLine("<h3>📋 Detalhes da Compra</h3>");
            corpo.AppendLine($"<p><strong>Pedido:</strong> {request.PedidoId}</p>");
            corpo.AppendLine($"<p><strong>Plano:</strong> {plano.Descricao}</p>");
            corpo.AppendLine($"<p><strong>Quantidade:</strong> {request.Quantidade}</p>");
            corpo.AppendLine($"<p><strong>Valor Unitário:</strong> R$ {request.ValorPlano:F2}</p>");
            corpo.AppendLine($"<p><strong>Valor Total:</strong> R$ {request.ValorTotal:F2}</p>");
            corpo.AppendLine($"<p><strong>Data da Compra:</strong> {request.DataCompra:dd/MM/yyyy HH:mm}</p>");
            corpo.AppendLine("</div>");

            // Status da assinatura
            corpo.AppendLine("<div class='info-box'>");
            corpo.AppendLine("<h3>📊 Status da Assinatura</h3>");
            var statusClass = ObterClasseCssStatus(request.StatusAssinatura);
            corpo.AppendLine($"<p><strong>Status:</strong> <span class='{statusClass}'>{statusAssinaturaDescricao}</span></p>");
            corpo.AppendLine($"<p><strong>Status do Pagamento:</strong> {statusPagamentoDescricao}</p>");
            corpo.AppendLine("</div>");

            // Dados do cliente (apenas para clientes novos)
            if (request.ClienteNovo && request.DadosClienteNovo != null)
            {
                corpo.AppendLine("<div class='info-box'>");
                corpo.AppendLine("<h3>👤 Dados da Sua Conta</h3>");
                corpo.AppendLine($"<p><strong>Nome:</strong> {request.NomeCliente}</p>");
                corpo.AppendLine($"<p><strong>Email:</strong> {request.EmailCliente}</p>");
                
                if (!string.IsNullOrEmpty(request.DadosClienteNovo.Telefone))
                {
                    corpo.AppendLine($"<p><strong>Telefone:</strong> {request.DadosClienteNovo.Telefone}</p>");
                }

                if (request.DadosClienteNovo.TipoPessoa.HasValue)
                {
                    var tipoPessoa = request.DadosClienteNovo.TipoPessoa.Value == TipoPessoa.FISICA ? "Pessoa Física" : "Pessoa Jurídica";
                    corpo.AppendLine($"<p><strong>Tipo:</strong> {tipoPessoa}</p>");
                }

                if (!string.IsNullOrEmpty(request.DadosClienteNovo.CpfCnpj))
                {
                    var documento = request.DadosClienteNovo.TipoPessoa == TipoPessoa.FISICA ? "CPF" : "CNPJ";
                    corpo.AppendLine($"<p><strong>{documento}:</strong> {request.DadosClienteNovo.CpfCnpj}</p>");
                }

                // Endereço se disponível
                if (request.DadosClienteNovo.Endereco != null)
                {
                    corpo.AppendLine("<p><strong>Endereço:</strong></p>");
                    var endereco = request.DadosClienteNovo.Endereco;
                    var enderecoCompleto = new List<string>();
                    
                    if (!string.IsNullOrEmpty(endereco.Logradouro))
                        enderecoCompleto.Add(endereco.Logradouro);
                    if (!string.IsNullOrEmpty(endereco.Numero))
                        enderecoCompleto.Add(endereco.Numero);
                    if (!string.IsNullOrEmpty(endereco.Complemento))
                        enderecoCompleto.Add(endereco.Complemento);
                    if (!string.IsNullOrEmpty(endereco.Bairro))
                        enderecoCompleto.Add(endereco.Bairro);
                    if (!string.IsNullOrEmpty(endereco.Cidade))
                        enderecoCompleto.Add(endereco.Cidade);
                    if (!string.IsNullOrEmpty(endereco.Estado))
                        enderecoCompleto.Add(endereco.Estado);
                    if (!string.IsNullOrEmpty(endereco.Cep))
                        enderecoCompleto.Add($"CEP: {endereco.Cep}");

                    corpo.AppendLine($"<p>{string.Join(", ", enderecoCompleto)}</p>");
                }

                corpo.AppendLine("</div>");
            }

            // Informações do vendedor se disponível
            if (request.VendedorId.HasValue && !string.IsNullOrEmpty(request.NomeVendedor))
            {
                corpo.AppendLine("<div class='info-box'>");
                corpo.AppendLine("<h3>👨‍💼 Seu Vendedor</h3>");
                corpo.AppendLine($"<p><strong>Nome:</strong> {request.NomeVendedor}</p>");
                corpo.AppendLine("<p>Em caso de dúvidas, entre em contato com seu vendedor.</p>");
                corpo.AppendLine("</div>");
            }

            // Próximos passos
            corpo.AppendLine("<div class='info-box'>");
            corpo.AppendLine("<h3>🚀 Próximos Passos</h3>");
            
            if (request.StatusAssinatura == StatusAssinatura.Active)
            {
                corpo.AppendLine("<p>✅ Sua assinatura está ativa! Você já pode começar a usar nossos serviços.</p>");
            }
            else if (request.StatusAssinatura == StatusAssinatura.WaitingPayment)
            {
                corpo.AppendLine("<p>⏳ Aguardando confirmação do pagamento. Você receberá uma notificação assim que o pagamento for confirmado.</p>");
            }
            else
            {
                corpo.AppendLine("<p>❌ Houve um problema com o processamento. Nossa equipe entrará em contato para resolver.</p>");
            }

            corpo.AppendLine("<p>Para mais informações, acesse sua área do cliente ou entre em contato conosco.</p>");
            corpo.AppendLine("</div>");

            corpo.AppendLine("</div>");

            // Rodapé
            corpo.AppendLine("<div class='footer'>");
            corpo.AppendLine("<p>Este é um email automático, por favor não responda.</p>");
            corpo.AppendLine("<p>© 2024 Moope. Todos os direitos reservados.</p>");
            corpo.AppendLine("</div>");

            corpo.AppendLine("</div>");
            corpo.AppendLine("</body>");
            corpo.AppendLine("</html>");

            return corpo.ToString();
        }

        private string ObterDescricaoStatusAssinatura(StatusAssinatura status)
        {
            var transactionStatus = EnumHelper.GetEnumInfo<StatusAssinatura>(status.ToString());
            return transactionStatus?.Description;
            // return status switch
            // {
            //     StatusAssinatura.Active => "Ativa",
            //     StatusAssinatura.WaitingPayment => "Aguardando Pagamento",
            //     StatusAssinatura.Canceled => "Cancelada",
            //     StatusAssinatura.Suspended => "Suspensa",
            //     StatusAssinatura.Expired => "Expirada",
            //     _ => status.ToString()
            // };
        }

        private string ObterDescricaoStatusPagamento(StatusPagamento status)
        {
            var pagemaentoStatus = EnumHelper.GetEnumInfo<StatusPagamento>(status.ToString());
            return pagemaentoStatus?.Description;
            // return status switch
            // {
            //     StatusPagamento.Paid => "Pago",
            //     StatusPagamento.Pending => "Pendente",
            //     StatusPagamento.Canceled => "Cancelado",
            //     StatusPagamento.Failed => "Falhou",
            //     StatusPagamento.Refunded => "Reembolsado",
            //     _ => status.ToString()
            // };
        }

        private string ObterClasseCssStatus(StatusAssinatura status)
        {
            var assinaturaStatus = EnumHelper.GetEnumInfo<StatusAssinatura>(status.ToString());
            return assinaturaStatus?.Description;
            // return status switch
            // {
            //     StatusAssinatura.Active => "status-success",
            //     StatusAssinatura.WaitingPayment => "status-pending",
            //     StatusAssinatura.Canceled or StatusAssinatura.Suspended or StatusAssinatura.Expired => "status-error",
            //     _ => "status-pending"
            // };
        }
    }
}
