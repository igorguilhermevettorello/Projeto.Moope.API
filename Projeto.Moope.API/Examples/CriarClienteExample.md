# Exemplo de Uso do Command/Handler Pattern para Criar Cliente

## Arquitetura Implementada

A implementação segue o padrão **Command/Handler** utilizando **MediatR** para criar clientes, seguindo os princípios de **Clean Architecture** e **DDD**.

### Componentes Principais

1. **CriarClienteCommand**: Encapsula todos os dados necessários para criar um cliente
2. **CriarClienteCommandHandler**: Contém toda a lógica de negócio para criação do cliente
3. **ClienteController**: Recebe a requisição e delega o processamento via MediatR

## Fluxo de Execução

### 1. Requisição HTTP

```http
POST /api/cliente
Content-Type: application/json
Authorization: Bearer {token}

{
  "nome": "João Silva",
  "email": "joao.silva@email.com",
  "cpfCnpj": "123.456.789-00",
  "telefone": "11999999999",
  "tipoPessoa": 1,
  "ativo": true,
  "senha": "MinhaSenh@123",
  "confirmacao": "MinhaSenh@123",
  "nomeFantasia": "",
  "inscricaoEstadual": "",
  "endereco": {
    "logradouro": "Rua das Flores",
    "numero": "123",
    "complemento": "Apt 45",
    "bairro": "Centro",
    "cidade": "São Paulo",
    "estado": "SP",
    "cep": "01234-567"
  }
}
```

### 2. Controller (API Layer)

```csharp
[HttpPost]
public async Task<IActionResult> CriarAsync([FromBody] CreateClienteDto createClienteDto)
{
    if (!ModelState.IsValid)
        return CustomResponse(ModelState);

    try
    {
        var command = _mapper.Map<CriarClienteCommand>(createClienteDto);

        // Se não for admin, definir vendedor como o usuário logado
        if (!await IsAdmin())
        {
            command.VendedorId = UsuarioId;
        }

        var resultado = await _mediator.Send(command);

        if (!resultado.Status)
            return CustomResponse();

        return Created(string.Empty, new { id = resultado.Dados });
    }
    catch (Exception ex)
    {
        NotificarErro("Mensagem", ex.Message);
        return CustomResponse();
    }
}
```

### 3. Command (Core Layer)

```csharp
public class CriarClienteCommand : ICommand<Result<Guid>>
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string CpfCnpj { get; set; } = string.Empty;
    [Required] public string Telefone { get; set; } = string.Empty;
    [Required] public TipoPessoa TipoPessoa { get; set; }
    public bool Ativo { get; set; } = true;
    [Required] [MinLength(6)] public string Senha { get; set; } = string.Empty;
    [Required] [Compare("Senha")] public string Confirmacao { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string InscricaoEstadual { get; set; } = string.Empty;
    public Guid? VendedorId { get; set; }

    // Dados do Endereço (flatten do DTO)
    [Required] public string Logradouro { get; set; } = string.Empty;
    [Required] public string Numero { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
    [Required] public string Bairro { get; set; } = string.Empty;
    [Required] public string Cidade { get; set; } = string.Empty;
    [Required] public string Estado { get; set; } = string.Empty;
    [Required] public string Cep { get; set; } = string.Empty;
}
```

### 4. Handler (Core Layer)

```csharp
public class CriarClienteCommandHandler : ICommandHandler<CriarClienteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CriarClienteCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 1. Criar usuário no Identity
            var rsIdentity = await _identityUserService.CriarUsuarioAsync(
                request.Email, request.Senha, request.Telefone, TipoUsuario.Cliente);

            if (!rsIdentity.Status)
                return Error(rsIdentity.Mensagem);

            // 2. Verificar se usuário já existe
            if (rsIdentity.UsuarioExiste)
            {
                // Usuário existente: apenas adicionar papel de cliente
                await ProcessarUsuarioExistente(cliente, identityUser);
            }
            else
            {
                // Novo usuário: processo completo
                await ProcessarNovoUsuario(request, entities...);
            }

            await _unitOfWork.CommitAsync();
            return Success(clienteId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return Error(ex.Message);
        }
    }
}
```

## Lógica de Negócio Implementada

### Cenário 1: Novo Usuário

1. **Criar usuário no Identity** com email, senha e telefone
2. **Salvar endereço** no banco de dados
3. **Criar usuário core** linkado ao Identity
4. **Criar cliente** com referências corretas
5. **Salvar Pessoa Física ou Jurídica** baseado no tipo
6. **Definir vendedor** se não for admin

### Cenário 2: Usuário Existente

1. **Usuário já existe no Identity**
2. **Adicionar papel de Cliente** ao usuário
3. **Criar registro de Cliente** apenas

## Vantagens da Implementação

### 1. **Transação Atômica**

- Todo o processo ocorre em uma única transação
- Rollback automático em caso de erro
- Consistência de dados garantida

### 2. **Reutilização de Usuários**

- Usuários existentes podem ter múltiplos papéis
- Evita duplicação de dados no Identity
- Flexibilidade para usuários serem Cliente e Vendedor

### 3. **Validação Centralizada**

- Validações de negócio no Handler
- Data Annotations no Command
- Validações customizadas quando necessário

### 4. **Separação de Responsabilidades**

- Controller: Orquestração e autorização
- Command: Estrutura de dados
- Handler: Lógica de negócio completa

### 5. **Testabilidade**

- Handler testável independentemente
- Mock fácil de dependências
- Cenários de teste bem definidos

## Configuração da Injeção de Dependência

O MediatR já está configurado no `DependencyInjectionConfig.cs`:

```csharp
private static void RegisterMediatR(IServiceCollection service)
{
    service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProcessarVendaCommand).Assembly));
}
```

## Mapeamento AutoMapper

```csharp
CreateMap<CreateClienteDto, CriarClienteCommand>()
    .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
    // ... outros campos do cliente
    // Mapeamento do endereço (flatten)
    .ForMember(dest => dest.Logradouro, opt => opt.MapFrom(src => src.Endereco.Logradouro))
    .ForMember(dest => dest.Numero, opt => opt.MapFrom(src => src.Endereco.Numero))
    // ... outros campos do endereço
```

## Comparação: Antes vs Depois

### ❌ Antes (Controller com 100+ linhas)

- Lógica de negócio no Controller
- Transações manuais
- Difícil de testar
- Violação do SRP
- Alto acoplamento

### ✅ Depois (Command/Handler)

- Controller com ~20 linhas
- Lógica no Handler (responsabilidade única)
- Transações automáticas (UnitOfWork)
- Fácil de testar e mockar
- Baixo acoplamento
- Reutilização via MediatR

## Endpoint Resultante

O endpoint `/api/cliente` (POST) agora:

1. ✅ Mantém a mesma interface externa
2. ✅ Reduz drasticamente o código do Controller
3. ✅ Centraliza lógica de negócio no Handler
4. ✅ Melhora testabilidade e manutenibilidade
5. ✅ Segue princípios SOLID e Clean Architecture

## Próximos Passos

1. **Implementar Commands para Update/Delete**: Criar comandos para outras operações
2. **Adicionar Validators**: FluentValidation para validações complexas
3. **Implementar Queries**: Separar leitura de escrita (CQRS)
4. **Adicionar Logging**: Middleware para audit trail
5. **Cache de Consultas**: Otimizar performance de leitura
