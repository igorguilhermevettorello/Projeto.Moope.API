# Integração do PasswordGenerator no Sistema

## Como Usar o PasswordGenerator nos Handlers

### 1. Exemplo no CriarClienteCommandHandler

```csharp
public class CriarClienteCommandHandler : ICommandHandler<CriarClienteCommand, Result<Guid>>
{
    private readonly IPasswordGenerator _passwordGenerator;
    // ... outras dependências

    public CriarClienteCommandHandler(
        IPasswordGenerator passwordGenerator,
        // ... outras dependências
        )
    {
        _passwordGenerator = passwordGenerator;
        // ... inicializar outras dependências
    }

    public async Task<Result<Guid>> Handle(CriarClienteCommand request, CancellationToken cancellationToken)
    {
        // Se senha não foi fornecida, gerar automaticamente
        if (string.IsNullOrWhiteSpace(request.Senha))
        {
            request.Senha = _passwordGenerator.GerarSenhaSimples(8);
            request.Confirmacao = request.Senha;

            // TODO: Notificar cliente sobre senha gerada
            // await _notificationService.EnviarSenhaPorEmail(request.Email, request.Senha);
        }

        // Continuar com lógica existente...
        // ... resto do código
    }
}
```

### 2. Exemplo em um ResetSenhaCommandHandler

```csharp
public class ResetSenhaCommandHandler : ICommandHandler<ResetSenhaCommand, Result<bool>>
{
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IIdentityUserService _identityUserService;
    private readonly IEmailService _emailService;

    public async Task<Result<bool>> Handle(ResetSenhaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Gerar nova senha temporária
            var novaSenha = _passwordGenerator.GerarSenhaTemporaria(10);

            // Atualizar senha no sistema
            var resultado = await _identityUserService.AlterarSenhaAsync(request.UserId, novaSenha);
            if (!resultado.Status)
            {
                return new Result<bool> { Status = false, Mensagem = "Erro ao alterar senha" };
            }

            // Enviar nova senha por email
            await _emailService.EnviarNovaSenha(request.Email, novaSenha);

            return new Result<bool>
            {
                Status = true,
                Mensagem = "Nova senha enviada por email",
                Dados = true
            };
        }
        catch (Exception ex)
        {
            return new Result<bool> { Status = false, Mensagem = ex.Message };
        }
    }
}
```

### 3. Exemplo em um GerarApiKeyCommandHandler

```csharp
public class GerarApiKeyCommandHandler : ICommandHandler<GerarApiKeyCommand, Result<string>>
{
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IClienteService _clienteService;

    public async Task<Result<string>> Handle(GerarApiKeyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Gerar API Key segura
            var apiKey = _passwordGenerator.GerarSenha(PasswordPresets.ParaAPI);

            // Salvar no cliente
            var cliente = await _clienteService.BuscarPorIdAsync(request.ClienteId);
            if (cliente == null)
            {
                return new Result<string> { Status = false, Mensagem = "Cliente não encontrado" };
            }

            cliente.ApiKey = apiKey;
            cliente.ApiKeyGeradaEm = DateTime.UtcNow;

            var resultado = await _clienteService.AtualizarAsync(cliente);
            if (!resultado.Status)
            {
                return new Result<string> { Status = false, Mensagem = "Erro ao salvar API Key" };
            }

            return new Result<string>
            {
                Status = true,
                Mensagem = "API Key gerada com sucesso",
                Dados = apiKey
            };
        }
        catch (Exception ex)
        {
            return new Result<string> { Status = false, Mensagem = ex.Message };
        }
    }
}
```

## Middleware de Validação de Senhas

```csharp
public class PasswordValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPasswordGenerator _passwordGenerator;

    public PasswordValidationMiddleware(RequestDelegate next, IPasswordGenerator passwordGenerator)
    {
        _next = next;
        _passwordGenerator = passwordGenerator;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Interceptar requests de mudança de senha
        if (context.Request.Path.StartsWithSegments("/api/auth/change-password") &&
            context.Request.Method == "POST")
        {
            context.Request.EnableBuffering();
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            try
            {
                var json = JsonDocument.Parse(body);
                if (json.RootElement.TryGetProperty("novaSenha", out var senhaElement))
                {
                    var novaSenha = senhaElement.GetString();
                    if (!string.IsNullOrEmpty(novaSenha) && !_passwordGenerator.ValidarSenha(novaSenha))
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            erro = "Senha não atende aos critérios mínimos de segurança",
                            criterios = new
                            {
                                comprimentoMinimo = "6 caracteres",
                                diversidade = "Pelo menos 3 tipos de caracteres (minúsculas, maiúsculas, números, símbolos)"
                            }
                        });
                        return;
                    }
                }
            }
            catch
            {
                // Se não conseguir parsear, deixa continuar
            }
        }

        await _next(context);
    }
}
```

## Serviço de Notificação com Senhas

```csharp
public interface IPasswordNotificationService
{
    Task EnviarSenhaTemporaria(string email, string senha);
    Task EnviarApiKey(string email, string apiKey);
    Task NotificarAlteracaoSenha(string email);
}

public class PasswordNotificationService : IPasswordNotificationService
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;

    public async Task EnviarSenhaTemporaria(string email, string senha)
    {
        var assunto = "Sua senha temporária - Sistema Moope";
        var corpo = $@"
            <h2>Senha Temporária Gerada</h2>
            <p>Olá!</p>
            <p>Uma senha temporária foi gerada para sua conta:</p>
            <div style='background: #f5f5f5; padding: 15px; font-family: monospace; font-size: 18px; text-align: center;'>
                <strong>{senha}</strong>
            </div>
            <p><strong>Importante:</strong> Esta é uma senha temporária. Por favor, altere-a no primeiro acesso.</p>
            <p>Se você não solicitou esta senha, entre em contato conosco imediatamente.</p>
        ";

        await _emailService.EnviarAsync(email, assunto, corpo);
    }

    public async Task EnviarApiKey(string email, string apiKey)
    {
        var assunto = "Sua API Key - Sistema Moope";
        var corpo = $@"
            <h2>API Key Gerada</h2>
            <p>Sua nova API Key foi gerada com sucesso:</p>
            <div style='background: #f5f5f5; padding: 15px; font-family: monospace; font-size: 14px; text-align: center; word-break: break-all;'>
                <strong>{apiKey}</strong>
            </div>
            <p><strong>Mantenha esta chave segura!</strong> Ela fornece acesso total à sua conta via API.</p>
        ";

        await _emailService.EnviarAsync(email, assunto, corpo);
    }

    public async Task NotificarAlteracaoSenha(string email)
    {
        var assunto = "Senha alterada - Sistema Moope";
        var corpo = @"
            <h2>Senha Alterada</h2>
            <p>Sua senha foi alterada com sucesso.</p>
            <p>Se você não fez esta alteração, entre em contato conosco imediatamente.</p>
        ";

        await _emailService.EnviarAsync(email, assunto, corpo);
    }
}
```

## Exemplos de Controllers

### 1. Endpoint para Reset de Senha

```csharp
[HttpPost("reset-senha")]
public async Task<IActionResult> ResetSenha([FromBody] ResetSenhaRequest request)
{
    if (!ModelState.IsValid)
        return CustomResponse(ModelState);

    try
    {
        var command = new ResetSenhaCommand
        {
            Email = request.Email,
            UserId = await GetUserIdByEmailAsync(request.Email)
        };

        var resultado = await _mediator.Send(command);

        if (!resultado.Status)
            return CustomResponse();

        return Ok(new { mensagem = "Nova senha enviada por email" });
    }
    catch (Exception ex)
    {
        NotificarErro("Erro", ex.Message);
        return CustomResponse();
    }
}
```

### 2. Endpoint para Gerar API Key

```csharp
[HttpPost("gerar-api-key")]
[Authorize]
public async Task<IActionResult> GerarApiKey()
{
    try
    {
        var command = new GerarApiKeyCommand { ClienteId = UsuarioId };
        var resultado = await _mediator.Send(command);

        if (!resultado.Status)
            return CustomResponse();

        return Ok(new {
            apiKey = resultado.Dados,
            mensagem = "API Key gerada com sucesso. Verifique seu email."
        });
    }
    catch (Exception ex)
    {
        NotificarErro("Erro", ex.Message);
        return CustomResponse();
    }
}
```

### 3. Endpoint para Validar Força da Senha

```csharp
[HttpPost("validar-senha")]
public IActionResult ValidarSenha([FromBody] ValidarSenhaRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Senha))
        return BadRequest("Senha é obrigatória");

    var valida = _passwordGenerator.ValidarSenha(request.Senha);

    return Ok(new {
        valida,
        forca = ClassificarForcaSenha(request.Senha),
        sugestoes = valida ? null : new[]
        {
            "Use pelo menos 6 caracteres",
            "Inclua pelo menos 3 tipos diferentes: minúsculas, maiúsculas, números e símbolos",
            "Evite sequências óbvias como '123456' ou 'abcdef'",
            "Use uma combinação única para cada conta"
        }
    });
}
```

## Configuração no Startup/Program

```csharp
// No Program.cs ou Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // ... outras configurações

    // Registrar PasswordGenerator (já configurado no DependencyInjectionConfig)
    services.AddDependencyInjectionConfig(Configuration);

    // Registrar serviço de notificação
    services.AddScoped<IPasswordNotificationService, PasswordNotificationService>();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ... outras configurações

    // Adicionar middleware de validação de senhas
    app.UseMiddleware<PasswordValidationMiddleware>();

    // ... outras configurações
}
```

## Casos de Uso Práticos

### 1. Cadastro Automático de Cliente (Sem Senha)

- Cliente se cadastra apenas com dados básicos
- Sistema gera senha automaticamente
- Envia senha por email/SMS
- Cliente faz primeiro login e altera senha

### 2. Recuperação de Senha

- Cliente esquece senha
- Sistema gera nova senha temporária
- Envia por email com instruções
- Cliente altera na primeira tentativa de login

### 3. Geração de Credenciais de API

- Cliente solicita acesso à API
- Sistema gera API Key segura
- Armazena hash da chave no banco
- Envia chave original por email

### 4. Senhas para Contas de Sistema

- Criação de contas de serviço
- Geração automática de senhas ultra-seguras
- Armazenamento em local seguro (vault)
- Rotação automática periódica

Esta integração fornece uma base sólida para geração e gerenciamento seguro de senhas em todo o sistema!
