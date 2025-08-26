# Sistema de Emails - Implementação Completa

## Visão Geral

O sistema de emails foi implementado seguindo o padrão **Clean Architecture** com **Command/Handler**, fornecendo uma solução robusta para gerenciamento de emails no sistema.

## Arquitetura Implementada

### 🏗️ **Componentes Criados**

1. **Entidade Email** (`Projeto.Moope.Core/Models/Email.cs`)
2. **Enums** (`StatusEmail` e `Prioridade`)
3. **Repository** (`IEmailRepository` e `EmailRepository`)
4. **Command/Handler** (`SalvarEmailCommand` e `SalvarEmailCommandHandler`)
5. **Service Interface** (`IEmailService`)

### 📊 **Estrutura da Entidade Email**

```csharp
public class Email : Entity
{
    // Dados básicos
    public string Remetente { get; set; }
    public string? NomeRemetente { get; set; }
    public string Destinatario { get; set; }
    public string? NomeDestinatario { get; set; }
    public string? Copia { get; set; }          // CC
    public string? CopiaOculta { get; set; }    // BCC
    public string Assunto { get; set; }
    public string Corpo { get; set; }
    public bool EhHtml { get; set; }

    // Controle e status
    public Prioridade Prioridade { get; set; }
    public StatusEmail Status { get; set; }
    public int TentativasEnvio { get; set; }
    public DateTime? UltimaTentativa { get; set; }
    public DateTime? DataEnvio { get; set; }
    public string? MensagemErro { get; set; }

    // Relacionamentos
    public Guid? UsuarioId { get; set; }
    public Guid? ClienteId { get; set; }

    // Metadados
    public string? Tipo { get; set; }
    public string? DadosAdicionais { get; set; }
    public DateTime? DataProgramada { get; set; }
}
```

### 📋 **Status Possíveis**

```csharp
public enum StatusEmail
{
    Pendente = 1,       // Aguardando envio
    Processando = 2,    // Sendo enviado
    Enviado = 3,        // Enviado com sucesso
    Falha = 4,          // Falha no envio
    Cancelado = 5,      // Cancelado
    Agendado = 6,       // Agendado para envio futuro
    Rejeitado = 7       // Rejeitado pelo servidor
}
```

## Como Usar o Sistema

### 1. Salvar Email via Command/Handler

```csharp
public class ExemploController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost("enviar-email")]
    public async Task<IActionResult> EnviarEmail([FromBody] EnviarEmailRequest request)
    {
        var command = new SalvarEmailCommand
        {
            Remetente = "sistema@moope.com.br",
            NomeRemetente = "Sistema Moope",
            Destinatario = request.Destinatario,
            NomeDestinatario = request.NomeDestinatario,
            Assunto = request.Assunto,
            Corpo = request.Corpo,
            EhHtml = true,
            Prioridade = Prioridade.Normal,
            Tipo = "NOTIFICACAO",
            UsuarioId = request.UsuarioId,
            EnviarImediatamente = true
        };

        var resultado = await _mediator.Send(command);

        if (!resultado.Status)
            return BadRequest(resultado.Mensagem);

        return Ok(new { emailId = resultado.Dados, mensagem = "Email salvo e enviado com sucesso" });
    }
}
```

### 2. Envio de Email de Boas-vindas

```csharp
public class CriarClienteCommandHandler : ICommandHandler<CriarClienteCommand, Result<Guid>>
{
    private readonly IMediator _mediator;

    public async Task<Result<Guid>> Handle(CriarClienteCommand request, CancellationToken cancellationToken)
    {
        // ... lógica de criação do cliente

        // Enviar email de boas-vindas
        var emailCommand = new SalvarEmailCommand
        {
            Remetente = "noreply@moope.com.br",
            NomeRemetente = "Equipe Moope",
            Destinatario = request.Email,
            NomeDestinatario = request.Nome,
            Assunto = "Bem-vindo ao Sistema Moope!",
            Corpo = CriarCorpoEmailBoasVindas(request.Nome, senhaGerada),
            EhHtml = true,
            Prioridade = Prioridade.Alta,
            Tipo = "BOAS_VINDAS",
            ClienteId = clienteId,
            EnviarImediatamente = true
        };

        await _mediator.Send(emailCommand);

        return new Result<Guid> { Status = true, Dados = clienteId };
    }

    private string CriarCorpoEmailBoasVindas(string nome, string senha)
    {
        return $@"
            <h2>Bem-vindo ao Sistema Moope, {nome}!</h2>
            <p>Sua conta foi criada com sucesso.</p>
            <p><strong>Senha temporária:</strong> {senha}</p>
            <p>Por favor, altere sua senha no primeiro acesso.</p>
            <p>Atenciosamente,<br/>Equipe Moope</p>
        ";
    }
}
```

### 3. Email de Reset de Senha

```csharp
public class ResetSenhaCommandHandler : ICommandHandler<ResetSenhaCommand, Result<bool>>
{
    private readonly IMediator _mediator;
    private readonly IPasswordGenerator _passwordGenerator;

    public async Task<Result<bool>> Handle(ResetSenhaCommand request, CancellationToken cancellationToken)
    {
        // Gerar nova senha
        var novaSenha = _passwordGenerator.GerarSenhaTemporaria(10);

        // ... atualizar senha no sistema

        // Enviar email com nova senha
        var emailCommand = new SalvarEmailCommand
        {
            Remetente = "suporte@moope.com.br",
            NomeRemetente = "Suporte Moope",
            Destinatario = request.Email,
            Assunto = "Nova Senha - Sistema Moope",
            Corpo = $@"
                <h2>Nova Senha Gerada</h2>
                <p>Uma nova senha foi gerada para sua conta:</p>
                <div style='background: #f5f5f5; padding: 15px; font-family: monospace; font-size: 18px;'>
                    <strong>{novaSenha}</strong>
                </div>
                <p>Esta é uma senha temporária. Altere-a no primeiro acesso.</p>
            ",
            EhHtml = true,
            Prioridade = Prioridade.Alta,
            Tipo = "RESET_SENHA",
            UsuarioId = request.UsuarioId,
            EnviarImediatamente = true
        };

        await _mediator.Send(emailCommand);

        return new Result<bool> { Status = true, Dados = true };
    }
}
```

### 4. Email Agendado

```csharp
public async Task<IActionResult> AgendarEmailPromocional()
{
    var command = new SalvarEmailCommand
    {
        Remetente = "marketing@moope.com.br",
        NomeRemetente = "Marketing Moope",
        Destinatario = "cliente@email.com",
        Assunto = "Promoção Especial - Black Friday",
        Corpo = "HTML da promoção...",
        EhHtml = true,
        Prioridade = Prioridade.Normal,
        Tipo = "PROMOCIONAL",
        DataProgramada = DateTime.UtcNow.AddDays(7), // Enviar em 7 dias
        EnviarImediatamente = false // Não enviar agora
    };

    var resultado = await _mediator.Send(command);
    return Ok(resultado);
}
```

## Funcionalidades do Repository

### 1. Buscar Emails por Status

```csharp
public class EmailService : IEmailService
{
    private readonly IEmailRepository _emailRepository;

    public async Task<int> ProcessarEmailsPendentesAsync()
    {
        var emailsPendentes = await _emailRepository.BuscarPendentesAsync();
        int processados = 0;

        foreach (var email in emailsPendentes)
        {
            if (await TentarEnviarEmail(email))
            {
                await _emailRepository.MarcarComoEnviadoAsync(email.Id);
                processados++;
            }
            else
            {
                await _emailRepository.AtualizarStatusAsync(email.Id, StatusEmail.Falha, "Falha no envio");
            }
        }

        return processados;
    }

    public async Task<int> ProcessarEmailsAgendadosAsync()
    {
        var emailsAgendados = await _emailRepository.BuscarAgendadosParaEnvioAsync();
        int processados = 0;

        foreach (var email in emailsAgendados)
        {
            // Atualizar status para pendente para processamento
            await _emailRepository.AtualizarStatusAsync(email.Id, StatusEmail.Pendente);
            processados++;
        }

        return processados;
    }
}
```

### 2. Estatísticas de Emails

```csharp
[HttpGet("estatisticas")]
public async Task<IActionResult> BuscarEstatisticas([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
{
    var estatisticas = await _emailRepository.BuscarEstatisticasAsync(inicio, fim);

    return Ok(new
    {
        totalEmails = estatisticas.TotalEmails,
        enviados = estatisticas.EmailsEnviados,
        pendentes = estatisticas.EmailsPendentes,
        falhas = estatisticas.EmailsComFalha,
        taxaSucesso = $"{estatisticas.TaxaSucesso:F2}%",
        taxaFalha = $"{estatisticas.TaxaFalha:F2}%"
    });
}
```

### 3. Reprocessar Emails com Falha

```csharp
[HttpPost("reprocessar-falhas")]
public async Task<IActionResult> ReprocessarEmailsComFalha()
{
    var emailsComFalha = await _emailRepository.BuscarFalhasParaReprocessarAsync(maxTentativas: 3);
    int reprocessados = 0;

    foreach (var email in emailsComFalha)
    {
        // Resetar para pendente para nova tentativa
        await _emailRepository.AtualizarStatusAsync(email.Id, StatusEmail.Pendente);
        reprocessados++;
    }

    return Ok(new { reprocessados });
}
```

## Job/Background Service para Processamento

```csharp
public class EmailBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailBackgroundService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                // Processar emails pendentes
                var pendentesProcessados = await emailService.ProcessarEmailsPendentesAsync();

                // Processar emails agendados
                var agendadosProcessados = await emailService.ProcessarEmailsAgendadosAsync();

                if (pendentesProcessados > 0 || agendadosProcessados > 0)
                {
                    _logger.LogInformation($"Processados: {pendentesProcessados} pendentes, {agendadosProcessados} agendados");
                }

                // Aguardar antes da próxima execução
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar emails");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
```

## Tipos de Email Comuns

```csharp
public static class TiposEmail
{
    public const string BOAS_VINDAS = "BOAS_VINDAS";
    public const string RESET_SENHA = "RESET_SENHA";
    public const string CONFIRMACAO_CADASTRO = "CONFIRMACAO_CADASTRO";
    public const string NOTIFICACAO_VENDA = "NOTIFICACAO_VENDA";
    public const string PROMOCIONAL = "PROMOCIONAL";
    public const string SISTEMA = "SISTEMA";
    public const string COBRANCA = "COBRANCA";
    public const string NEWSLETTER = "NEWSLETTER";
}
```

## Exemplos de Templates

### Template de Boas-vindas

```csharp
public static string TemplateBoasVindas(string nome, string senha)
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
                        <code style='font-size: 18px;'>{senha}</code>
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
```

## Próximos Passos

1. **Implementar EmailService**: Criar service real para envio via SMTP
2. **Templates Avançados**: Sistema de templates dinâmicos
3. **Attachments**: Suporte a anexos
4. **Webhooks**: Notificações de status de entrega
5. **Analytics**: Tracking de abertura e cliques
6. **Queue System**: Integração com filas (RabbitMQ, Azure Service Bus)
7. **Rate Limiting**: Controle de taxa de envio
8. **Blacklist**: Sistema de lista negra de emails

A implementação fornece uma base sólida e extensível para o gerenciamento completo de emails no sistema!
