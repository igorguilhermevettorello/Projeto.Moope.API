# Cadastro de Cliente com Endereço Opcional

## Alterações Implementadas

A obrigatoriedade do endereço no cadastro do cliente foi removida, permitindo maior flexibilidade no processo de cadastro. Agora é possível criar clientes sem informar o endereço inicialmente.

## Estrutura de Dados Atualizada

### 1. CreateClienteDto (API Layer)

```csharp
public class CreateClienteDto
{
    [Required] public string Nome { get; set; }
    [Required] [EmailAddress] public string Email { get; set; }
    [Required] public string CpfCnpj { get; set; }
    [Required] public string Telefone { get; set; }
    [Required] public TipoPessoa TipoPessoa { get; set; }
    public bool Ativo { get; set; } = true;
    [Required] [MinLength(6)] public string Senha { get; set; }
    [Required] [Compare("Senha")] public string Confirmacao { get; set; }

    // ✅ OPCIONAL AGORA
    public CreateEnderecoDto? Endereco { get; set; }
}
```

### 2. CriarClienteCommand (Core Layer)

```csharp
public class CriarClienteCommand : ICommand<Result<Guid>>
{
    // Campos obrigatórios do cliente
    [Required] public string Nome { get; set; } = string.Empty;
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
    // ... outros campos obrigatórios

    // ✅ CAMPOS DE ENDEREÇO OPCIONAIS
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }
}
```

## Exemplos de Uso

### 1. Cadastro COM Endereço (Funcionalidade Mantida)

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
  "senha": "MinhaSenh@123",
  "confirmacao": "MinhaSenh@123",
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

### 2. Cadastro SEM Endereço (Nova Funcionalidade)

```http
POST /api/cliente
Content-Type: application/json
Authorization: Bearer {token}

{
  "nome": "Maria Santos",
  "email": "maria.santos@email.com",
  "cpfCnpj": "987.654.321-00",
  "telefone": "11888888888",
  "tipoPessoa": 1,
  "senha": "MinhaSenh@456",
  "confirmacao": "MinhaSenh@456"
}
```

### 3. Cadastro com Endereço Parcial

```http
POST /api/cliente
Content-Type: application/json
Authorization: Bearer {token}

{
  "nome": "Carlos Oliveira",
  "email": "carlos.oliveira@email.com",
  "cpfCnpj": "456.789.123-00",
  "telefone": "11777777777",
  "tipoPessoa": 1,
  "senha": "MinhaSenh@789",
  "confirmacao": "MinhaSenh@789",
  "endereco": {
    "cidade": "Rio de Janeiro",
    "estado": "RJ"
  }
}
```

## Lógica de Negócio Implementada

### Handler Logic (CriarClienteCommandHandler)

```csharp
private async Task<Guid> ProcessarNovoUsuario(...)
{
    // Configurar usuário
    usuario.Id = identityUserId;
    usuario.TipoUsuario = TipoUsuario.Cliente;

    // ✅ SALVAR ENDEREÇO APENAS SE INFORMADO
    if (TemEnderecoInformado(request))
    {
        var rsEndereco = await _enderecoService.SalvarAsync(endereco);
        if (!rsEndereco.Status)
            throw new Exception(rsEndereco.Mensagem ?? "Erro ao salvar endereço");

        usuario.EnderecoId = rsEndereco.Dados.Id;
    }
    // Se não tem endereço, EnderecoId fica null

    var rsUsuario = await _usuarioService.SalvarAsync(usuario);
    // ... continua o processo
}

private bool TemEnderecoInformado(CriarClienteCommand request)
{
    return !string.IsNullOrWhiteSpace(request.Logradouro) ||
           !string.IsNullOrWhiteSpace(request.Numero) ||
           !string.IsNullOrWhiteSpace(request.Bairro) ||
           !string.IsNullOrWhiteSpace(request.Cidade) ||
           !string.IsNullOrWhiteSpace(request.Estado) ||
           !string.IsNullOrWhiteSpace(request.Cep);
}
```

## Mapeamento AutoMapper Atualizado

```csharp
CreateMap<CreateClienteDto, CriarClienteCommand>()
    // ... mapeamentos dos campos do cliente
    // ✅ MAPEAMENTO SEGURO PARA ENDEREÇO OPCIONAL
    .ForMember(dest => dest.Logradouro, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Logradouro : null))
    .ForMember(dest => dest.Numero, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Numero : null))
    .ForMember(dest => dest.Bairro, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Bairro : null))
    .ForMember(dest => dest.Cidade, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Cidade : null))
    .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Estado : null))
    .ForMember(dest => dest.Cep, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Cep : null));
```

## Cenários de Teste

### ✅ Cenário 1: Cliente com Endereço Completo

- **Input**: Todos os campos do cliente + endereço completo
- **Resultado**: Cliente criado com endereço linkado
- **EnderecoId**: Preenchido com ID do endereço

### ✅ Cenário 2: Cliente sem Endereço

- **Input**: Apenas campos obrigatórios do cliente
- **Resultado**: Cliente criado sem endereço
- **EnderecoId**: null

### ✅ Cenário 3: Cliente com Endereço Parcial

- **Input**: Campos do cliente + alguns campos do endereço
- **Resultado**: Cliente criado com endereço parcialmente preenchido
- **EnderecoId**: Preenchido com ID do endereço

### ✅ Cenário 4: Objeto Endereco Vazio

- **Input**: Cliente + objeto endereco com campos vazios/null
- **Resultado**: Cliente criado sem endereço (endereço não é salvo)
- **EnderecoId**: null

## Vantagens da Implementação

### 1. **Flexibilidade de Cadastro**

- Permite cadastro rápido sem endereço
- Endereço pode ser adicionado posteriormente
- Melhora UX para cadastros urgentes

### 2. **Backward Compatibility**

- Mantém compatibilidade total com cadastros que incluem endereço
- Não quebra integrações existentes
- Mesma API, comportamento aprimorado

### 3. **Validação Inteligente**

- Valida endereço apenas se informado
- Não salva registros de endereço vazios
- Economiza espaço no banco de dados

### 4. **Manutenibilidade**

- Lógica centralizada no Handler
- Fácil de testar cenários com/sem endereço
- Código limpo e bem estruturado

## Impactos no Banco de Dados

### Tabela Usuario

- **EnderecoId**: Agora pode ser `null`
- **Relacionamento**: Optional com Endereco

### Tabela Endereco

- **Comportamento**: Só cria registro se informado
- **Integridade**: Mantida via foreign key opcional

## Response Examples

### Sucesso (Cliente com Endereço)

```json
{
  "id": "12345678-1234-1234-1234-123456789012"
}
```

### Sucesso (Cliente sem Endereço)

```json
{
  "id": "87654321-4321-4321-4321-210987654321"
}
```

### Erro de Validação

```json
{
  "errors": [
    {
      "campo": "Nome",
      "mensagem": "O campo Nome é obrigatório"
    }
  ]
}
```

## Considerações Importantes

1. **Endereço Opcional**: O endereço é completamente opcional no cadastro
2. **Validação Mantida**: Campos obrigatórios do cliente continuam sendo validados
3. **Performance**: Melhora performance ao não criar registros desnecessários
4. **Integração**: APIs existentes continuam funcionando normalmente
5. **UX**: Melhora experiência do usuário com cadastro mais flexível

## Próximos Passos Sugeridos

1. **Endpoint para Adicionar Endereço**: Criar endpoint específico para adicionar endereço depois
2. **Validação Condicional**: Implementar validações que dependem do contexto
3. **Dashboard de Clientes**: Identificar clientes sem endereço para follow-up
4. **Notificações**: Alertar sobre clientes que precisam completar cadastro
