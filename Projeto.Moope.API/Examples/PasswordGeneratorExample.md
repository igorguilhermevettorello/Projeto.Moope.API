# PasswordGenerator - Geração Automática de Senhas

## Visão Geral

O **PasswordGenerator** é uma ferramenta robusta e segura para geração automática de senhas no sistema. Foi desenvolvido seguindo as melhores práticas de segurança e oferece flexibilidade total na configuração dos tipos de senhas geradas.

## Características Principais

### 🔒 **Segurança**

- Utiliza `RandomNumberGenerator` criptograficamente seguro
- Garante diversidade de caracteres quando solicitado
- Embaralha senhas para evitar padrões previsíveis
- Suporte a exclusão de caracteres ambíguos

### 🎯 **Flexibilidade**

- Configuração completa de tipos de caracteres
- Presets pré-definidos para casos comuns
- Métodos de extensão para facilitar uso
- Validação de senhas existentes

### 🛠️ **Facilidade de Uso**

- Interface simples via injeção de dependência
- Métodos de conveniência para casos comuns
- Documentação completa e exemplos práticos

## Configuração

O PasswordGenerator já está configurado na injeção de dependência:

```csharp
// Em DependencyInjectionConfig.cs
private static void RegisterUtilities(IServiceCollection service)
{
    service.AddScoped<IPasswordGenerator, PasswordGenerator>();
}
```

## Uso Básico

### 1. Injeção de Dependência

```csharp
public class ExemploController : ControllerBase
{
    private readonly IPasswordGenerator _passwordGenerator;

    public ExemploController(IPasswordGenerator passwordGenerator)
    {
        _passwordGenerator = passwordGenerator;
    }
}
```

### 2. Exemplos Simples

```csharp
// Senha padrão (12 caracteres, todos os tipos)
var senhaBasica = _passwordGenerator.GerarSenha();
// Resultado: "Kp8$mX2@nQ9w"

// Senha com comprimento específico
var senhaPersonalizada = _passwordGenerator.GerarSenha(16);
// Resultado: "R7#vB9@mK2$xN8pL"
```

## Métodos de Extensão (Conveniência)

### 1. Senha Simples (Clientes)

```csharp
// Senha simples: apenas letras e números, sem símbolos
var senhaCliente = _passwordGenerator.GerarSenhaSimples(8);
// Resultado: "Km8vB2nX"
```

### 2. Senha Forte (Administradores)

```csharp
// Senha forte: todos os caracteres, máxima segurança
var senhaAdmin = _passwordGenerator.GerarSenhaForte(16);
// Resultado: "K8$mX@2pQ#9wB7&v"
```

### 3. PIN Numérico

```csharp
// PIN numérico para autenticação de dois fatores
var pin = _passwordGenerator.GerarPin(6);
// Resultado: "875423"
```

### 4. Senha Temporária

```csharp
// Senha temporária (será alterada pelo usuário)
var senhaTemp = _passwordGenerator.GerarSenhaTemporaria(8);
// Resultado: "Km8vB2nX"
```

### 5. Senha Segura para API

```csharp
// Senha segura para APIs (evita caracteres problemáticos)
var senhaApi = _passwordGenerator.GerarSenhaSeguraParaAPI(12);
// Resultado: "K8@mX2pQ#9wB"
```

## Configuração Avançada

### 1. Usando PasswordOptions

```csharp
var opcoes = new PasswordOptions
{
    Comprimento = 14,
    IncluirMinusculas = true,
    IncluirMaiusculas = true,
    IncluirNumeros = true,
    IncluirSimbolos = false,        // Sem símbolos
    ExcluirAmbiguos = true,         // Excluir 0, O, l, I
    GarantirDiversidade = true,     // Pelo menos um de cada tipo
    SimbolosCustomizados = null
};

var senhaCustomizada = _passwordGenerator.GerarSenha(opcoes);
// Resultado: "Km8vB2nXpQ9wR7"
```

### 2. Símbolos Customizados

```csharp
var opcoes = new PasswordOptions
{
    Comprimento = 10,
    IncluirSimbolos = true,
    SimbolosCustomizados = "!@#$"  // Apenas estes símbolos
};

var senha = _passwordGenerator.GerarSenha(opcoes);
// Resultado: "K8@mX2#pQ!"
```

## Presets Pré-definidos

### 1. Para Clientes

```csharp
var senhaCliente = _passwordGenerator.GerarSenha(PasswordPresets.ParaCliente);
// 8 caracteres, sem símbolos, fácil de lembrar
```

### 2. Para Administradores

```csharp
var senhaAdmin = _passwordGenerator.GerarSenha(PasswordPresets.ParaAdministrador);
// 16 caracteres, máxima segurança
```

### 3. Temporária

```csharp
var senhaTemp = _passwordGenerator.GerarSenha(PasswordPresets.Temporaria);
// 10 caracteres, será alterada pelo usuário
```

### 4. PIN

```csharp
var pin = _passwordGenerator.GerarSenha(PasswordPresets.Pin);
// 6 dígitos numéricos
```

### 5. Para API

```csharp
var senhaApi = _passwordGenerator.GerarSenha(PasswordPresets.ParaAPI);
// 32 caracteres, segura para sistemas
```

## Geração em Lote

```csharp
// Gerar múltiplas senhas
var senhas = _passwordGenerator.GerarSenhas(5, PasswordPresets.ParaCliente);
// Resultado: ["Km8vB2nX", "R7pQ9wBv", "X2mK8pLn", ...]

// Usando foreach
foreach (var senha in senhas)
{
    Console.WriteLine($"Senha gerada: {senha}");
}
```

## Validação de Senhas

```csharp
// Validar se uma senha atende aos critérios mínimos
var senhaValida = _passwordGenerator.ValidarSenha("MinhaSenh@123");
// Resultado: true (tem pelo menos 3 tipos de caracteres)

var senhaInvalida = _passwordGenerator.ValidarSenha("123456");
// Resultado: false (apenas números, muito simples)
```

## Exemplos Práticos no Sistema

### 1. Cadastro de Cliente

```csharp
[HttpPost]
public async Task<IActionResult> CriarCliente([FromBody] CreateClienteDto dto)
{
    // Se senha não foi fornecida, gerar automaticamente
    if (string.IsNullOrEmpty(dto.Senha))
    {
        dto.Senha = _passwordGenerator.GerarSenhaSimples(8);
        dto.Confirmacao = dto.Senha;

        // TODO: Enviar senha por email ou SMS
        // await _emailService.EnviarSenhaTemporaria(dto.Email, dto.Senha);
    }

    var command = _mapper.Map<CriarClienteCommand>(dto);
    var resultado = await _mediator.Send(command);

    return Ok(resultado);
}
```

### 2. Reset de Senha

```csharp
[HttpPost("reset-senha")]
public async Task<IActionResult> ResetSenha([FromBody] ResetSenhaDto dto)
{
    // Gerar nova senha temporária
    var novaSenha = _passwordGenerator.GerarSenhaTemporaria(10);

    // Atualizar no sistema
    await _userManager.ChangePasswordAsync(user, novaSenha);

    // Enviar por email
    await _emailService.EnviarNovaSenha(dto.Email, novaSenha);

    return Ok(new { mensagem = "Nova senha enviada por email" });
}
```

### 3. Criação de API Keys

```csharp
[HttpPost("gerar-api-key")]
public async Task<IActionResult> GerarApiKey()
{
    var apiKey = _passwordGenerator.GerarSenha(PasswordPresets.ParaAPI);

    // Salvar no banco de dados
    var cliente = await GetClienteAtual();
    cliente.ApiKey = apiKey;
    await _clienteService.AtualizarAsync(cliente);

    return Ok(new { apiKey });
}
```

### 4. Validação em Middleware

```csharp
public class PasswordValidationMiddleware
{
    private readonly IPasswordGenerator _passwordGenerator;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/auth/change-password"))
        {
            var novaSenha = context.Request.Form["novaSenha"];

            if (!_passwordGenerator.ValidarSenha(novaSenha))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Senha não atende aos critérios mínimos");
                return;
            }
        }

        await _next(context);
    }
}
```

## Cenários de Uso

| Cenário                 | Método Recomendado            | Características         |
| ----------------------- | ----------------------------- | ----------------------- |
| **Cadastro de Cliente** | `GerarSenhaSimples(8)`        | Fácil de lembrar        |
| **Cadastro de Admin**   | `GerarSenhaForte(16)`         | Máxima segurança        |
| **Reset de Senha**      | `GerarSenhaTemporaria(10)`    | Será alterada           |
| **2FA/PIN**             | `GerarPin(6)`                 | Apenas números          |
| **API Key**             | `GerarSenha(ParaAPI)`         | 32 chars seguros        |
| **Senha de Sistema**    | `GerarSenhaSeguraParaAPI(24)` | Sem chars problemáticos |

## Características Técnicas

### Segurança

- ✅ **RandomNumberGenerator**: Criptograficamente seguro
- ✅ **Diversidade Garantida**: Pelo menos um char de cada tipo
- ✅ **Embaralhamento**: Evita padrões previsíveis
- ✅ **Exclusão de Ambíguos**: Opcional (0, O, l, I)

### Performance

- ✅ **Eficiente**: Geração rápida mesmo para lotes grandes
- ✅ **Memory Safe**: Não mantém senhas em memória
- ✅ **Thread Safe**: Pode ser usado em cenários concorrentes

### Flexibilidade

- ✅ **Configurável**: Todos os aspectos customizáveis
- ✅ **Extensível**: Fácil de adicionar novos presets
- ✅ **Presets**: Configurações prontas para casos comuns

## Limitações e Validações

### Limitações

- **Comprimento**: Mínimo 6, máximo 128 caracteres
- **Lote**: Máximo 1000 senhas por vez
- **Tipos**: Pelo menos um tipo de caractere deve estar habilitado

### Validações Automáticas

- Verifica se comprimento está dentro dos limites
- Valida se pelo menos um tipo de caractere está habilitado
- Garante diversidade se solicitado
- Previne configurações inválidas

## Boas Práticas

### ✅ Faça

- Use presets quando possível
- Sempre valide senhas recebidas
- Gere senhas temporárias para reset
- Use senhas diferentes para APIs
- Documente qual tipo de senha usar em cada caso

### ❌ Evite

- Senhas muito curtas (<8 caracteres) para usuários
- Apenas números para senhas permanentes
- Caracteres ambíguos em senhas que serão digitadas
- Reutilizar a mesma senha para diferentes propósitos
- Manter senhas em logs ou variáveis

## Próximos Passos

1. **Integração com Email**: Enviar senhas temporárias automaticamente
2. **Histórico de Senhas**: Evitar reutilização de senhas anteriores
3. **Políticas Customizadas**: Configurações por tipo de usuário
4. **Métricas**: Tracking de uso e efetividade
5. **Backup/Recovery**: Senhas de emergência para administradores
