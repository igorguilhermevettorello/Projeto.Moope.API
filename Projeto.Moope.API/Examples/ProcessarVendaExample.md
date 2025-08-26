# Exemplo de Uso do Command/Handler Pattern para Processar Vendas

## Arquitetura Implementada

A implementação segue o padrão **Command/Handler** utilizando **MediatR** para desacoplar a lógica de negócio dos controllers, seguindo os princípios de **Clean Architecture**.

### Componentes Principais

1. **ProcessarVendaCommand**: Encapsula todos os dados necessários para processar uma venda
2. **ProcessarVendaCommandHandler**: Contém toda a lógica de negócio para processar a venda
3. **VendaController**: Recebe a requisição e delega o processamento via MediatR

## Fluxo de Execução

### 1. Requisição HTTP

```http
POST /api/venda/processar
Content-Type: application/json

{
  "nomeCliente": "João Silva",
  "email": "joao.silva@email.com",
  "telefone": "11999999999",
  "tipoPessoa": 1,
  "cpfCnpj": "123.456.789-00",
  "vendedorId": "12345678-1234-1234-1234-123456789012",
  "planoId": "87654321-4321-4321-4321-210987654321",
  "quantidade": 1,
  "nomeCartao": "João Silva",
  "numeroCartao": "4111111111111111",
  "cvv": "123",
  "dataValidade": "12/25"
}
```

### 2. Controller (API Layer)

```csharp
[HttpPost("processar")]
public async Task<IActionResult> ProcessarVenda([FromBody] CreateVendaDto vendaDto)
{
    if (!ModelState.IsValid)
        return CustomResponse(ModelState);

    try
    {
        var command = _mapper.Map<ProcessarVendaCommand>(vendaDto);
        var resultado = await _mediator.Send(command);

        if (!resultado.Status)
            return CustomResponse();

        return Ok(resultado);
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
public class ProcessarVendaCommand : ICommand<Result<Pedido>>
{
    public string NomeCliente { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string NumeroCartao { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public string DataValidade { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public Guid VendedorId { get; set; }
    public Guid PlanoId { get; set; }
    public int Quantidade { get; set; }
    public string? Descricao { get; set; }
    public Guid? ClienteId { get; set; }
}
```

### 4. Handler (Core Layer)

```csharp
public class ProcessarVendaCommandHandler : ICommandHandler<ProcessarVendaCommand, Result<Pedido>>
{
    public async Task<Result<Pedido>> Handle(ProcessarVendaCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar vendedor
        // 2. Validar plano
        // 3. Calcular valor total
        // 4. Criar/buscar cliente
        // 5. Criar pedido
        // 6. Processar pagamento
        // 7. Atualizar status e criar transação
        // 8. Retornar resultado
    }
}
```

## Vantagens da Implementação

### 1. **Separation of Concerns**

- Controller apenas orquestra a requisição
- Handler contém toda a lógica de negócio
- Command encapsula os dados de entrada

### 2. **Testabilidade**

- Handlers podem ser testados independentemente
- Fácil mock de dependências
- Testes unitários mais focados

### 3. **Manutenibilidade**

- Lógica de negócio centralizada no Handler
- Fácil adição de novos comportamentos
- Baixo acoplamento entre camadas

### 4. **Escalabilidade**

- Possibilidade de adicionar middlewares (validação, logging, cache)
- Pipeline de processamento configurável
- Fácil adição de novos Commands

### 5. **Princípios SOLID**

- **Single Responsibility**: Cada handler tem uma responsabilidade
- **Open/Closed**: Extensível via novos handlers
- **Dependency Inversion**: Depende de abstrações (interfaces)

## Benefícios do MediatR

1. **Desacoplamento**: Controllers não conhecem implementações específicas
2. **Pipeline**: Comportamentos transversais (validação, logging, etc.)
3. **Request/Response**: Padrão consistente de comunicação
4. **InProc Messaging**: Comunicação interna via messages

## Configuração da Injeção de Dependência

```csharp
private static void RegisterMediatR(IServiceCollection service)
{
    service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProcessarVendaCommand).Assembly));
}
```

## Mapeamento AutoMapper

```csharp
CreateMap<CreateVendaDto, ProcessarVendaCommand>()
    .ForMember(dest => dest.VendedorId, opt => opt.MapFrom(src => src.VendedorId ?? Guid.Empty))
    .ForMember(dest => dest.PlanoId, opt => opt.MapFrom(src => src.PlanoId ?? Guid.Empty))
    .ForMember(dest => dest.Valor, opt => opt.Ignore()) // Calculado no handler
    .ForMember(dest => dest.Descricao, opt => opt.Ignore())
    .ForMember(dest => dest.ClienteId, opt => opt.Ignore());
```

## Próximos Passos

1. **Implementar Validações**: Usar FluentValidation para validar Commands
2. **Adicionar Logging**: Middleware para log de requests/responses
3. **Implementar Cache**: Cache de consultas frequentes
4. **Adicionar Retry**: Política de retry para falhas temporárias
5. **Implementar Queries**: Separar Commands (escrita) de Queries (leitura) - CQRS
