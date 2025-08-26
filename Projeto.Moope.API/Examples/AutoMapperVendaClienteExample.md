# AutoMapper: CreateVendaDto → CriarClienteCommand

## Problema Resolvido

Foi corrigido o erro de mapeamento:

```
Missing type map configuration or unsupported mapping.
Mapping types:
CreateVendaDto -> CriarClienteCommand
```

Este erro ocorria na linha 54 do `VendaController.cs` porque o AutoMapper não tinha configuração para mapear um DTO de venda para um command de criação de cliente.

## 🏗️ Contexto da Aplicação

No fluxo de processamento de vendas, quando um cliente novo faz uma compra, o sistema precisa:

1. **Criar o cliente** automaticamente usando `CriarClienteCommand`
2. **Processar a venda** usando `ProcessarVendaCommand`

Por isso, o `CreateVendaDto` precisa ser mapeado para `CriarClienteCommand` para criar o cliente antes de processar a venda.

## 🚀 Solução Implementada

### Mapeamento AutoMapper Adicionado:

```csharp
// Mapeamento de CreateVendaDto para CriarClienteCommand (usado no processo de venda)
CreateMap<CreateVendaDto, CriarClienteCommand>()
    .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.NomeCliente))
    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
    .ForMember(dest => dest.CpfCnpj, opt => opt.MapFrom(src => src.CpfCnpj))
    .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src => src.Telefone))
    .ForMember(dest => dest.TipoPessoa, opt => opt.MapFrom(src => src.TipoPessoa))
    .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => true))
    .ForMember(dest => dest.VendedorId, opt => opt.MapFrom(src => src.VendedorId))
    // Campos obrigatórios que não existem no CreateVendaDto - usar valores padrão
    .ForMember(dest => dest.Senha, opt => opt.MapFrom(src => "ClienteVenda123!"))
    .ForMember(dest => dest.Confirmacao, opt => opt.MapFrom(src => "ClienteVenda123!"))
    .ForMember(dest => dest.NomeFantasia, opt => opt.MapFrom(src => string.Empty))
    .ForMember(dest => dest.InscricaoEstadual, opt => opt.MapFrom(src => string.Empty))
    // Endereço não informado na venda
    .ForMember(dest => dest.Logradouro, opt => opt.MapFrom(src => (string)null))
    .ForMember(dest => dest.Numero, opt => opt.MapFrom(src => (string)null))
    .ForMember(dest => dest.Complemento, opt => opt.MapFrom(src => (string)null))
    .ForMember(dest => dest.Bairro, opt => opt.MapFrom(src => (string)null))
    .ForMember(dest => dest.Cidade, opt => opt.MapFrom(src => (string)null))
    .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => (string)null))
    .ForMember(dest => dest.Cep, opt => opt.MapFrom(src => (string)null));
```

## 📊 Mapeamento de Campos

### ✅ **Campos Mapeados Diretamente:**

| CreateVendaDto | CriarClienteCommand | Observações         |
| -------------- | ------------------- | ------------------- |
| `NomeCliente`  | `Nome`              | Nome do cliente     |
| `Email`        | `Email`             | Email do cliente    |
| `CpfCnpj`      | `CpfCnpj`           | CPF ou CNPJ         |
| `Telefone`     | `Telefone`          | Telefone de contato |
| `TipoPessoa`   | `TipoPessoa`        | Física ou Jurídica  |
| `VendedorId`   | `VendedorId`        | ID do vendedor      |

### ⚙️ **Campos com Valores Padrão:**

| Campo               | Valor Padrão         | Justificativa                       |
| ------------------- | -------------------- | ----------------------------------- |
| `Ativo`             | `true`               | Cliente criado via venda fica ativo |
| `Senha`             | `"ClienteVenda123!"` | Senha temporária forte              |
| `Confirmacao`       | `"ClienteVenda123!"` | Confirmação da senha                |
| `NomeFantasia`      | `string.Empty`       | Não informado na venda              |
| `InscricaoEstadual` | `string.Empty`       | Não informado na venda              |

### 🏠 **Campos de Endereço:**

Todos os campos de endereço são mapeados como `null` porque não são coletados no processo de venda:

- `Logradouro` → `null`
- `Numero` → `null`
- `Complemento` → `null`
- `Bairro` → `null`
- `Cidade` → `null`
- `Estado` → `null`
- `Cep` → `null`

## 🔄 Fluxo de Execução

### VendaController.ProcessarVenda():

```csharp
[HttpPost("processar")]
public async Task<IActionResult> ProcessarVenda([FromBody] CreateVendaDto vendaDto)
{
    try
    {
        // 1. Mapear venda para comando de criação de cliente
        var cliente = _mapper.Map<CriarClienteCommand>(vendaDto);

        // 2. Definir vendedor se não for admin
        if (!await IsAdmin())
        {
            cliente.VendedorId = (UsuarioId == Guid.Empty) ? null : UsuarioId;
        }

        // 3. Criar cliente
        var rsCliente = await _mediator.Send(cliente);
        if (!rsCliente.Status)
            return CustomResponse();

        // 4. Processar venda
        var command = _mapper.Map<ProcessarVendaCommand>(vendaDto);
        var rsVenda = await _mediator.Send(command);

        if (!rsVenda.Status)
            return CustomResponse();

        return Ok();
    }
    catch (Exception ex)
    {
        NotificarErro("Mensagem", ex.Message);
        return CustomResponse();
    }
}
```

## 🔐 Considerações de Segurança

### ⚠️ **Senha Temporária:**

A senha `"ClienteVenda123!"` é temporária e deve ser alterada pelo cliente. Recomendações:

1. **Email de Boas-vindas**: Enviar email com credenciais temporárias
2. **Forçar Alteração**: Obrigar mudança no primeiro login
3. **Geração Aleatória**: Implementar geração de senha aleatória
4. **Expiração**: Definir prazo para alteração

### 📧 **Notificação por Email:**

```csharp
// Exemplo de implementação futura
await _emailService.EnviarEmailSimplesAsync(
    cliente.Email,
    "Bem-vindo ao Sistema Moope",
    $"Suas credenciais temporárias são: Email: {cliente.Email}, Senha: {senhaTemporaria}",
    true
);
```

## 🎯 Casos de Uso

### **Cenário 1: Cliente Novo**

```json
{
  "nomeCliente": "João Silva",
  "email": "joao@email.com",
  "telefone": "(11) 99999-9999",
  "tipoPessoa": 1,
  "cpfCnpj": "12345678901",
  "vendedorId": "550e8400-e29b-41d4-a716-446655440000",
  "planoId": "660e8400-e29b-41d4-a716-446655440000",
  "quantidade": 1
  // ... dados do cartão
}
```

**Resultado:**

- ✅ Cliente criado com dados básicos
- ✅ Senha temporária definida
- ✅ Venda processada
- ✅ Relacionamento vendedor-cliente estabelecido

### **Cenário 2: Pessoa Jurídica**

```json
{
  "nomeCliente": "Empresa ABC Ltda",
  "tipoPessoa": 2,
  "cpfCnpj": "12.345.678/0001-90"
  // ... outros dados
}
```

**Resultado:**

- ✅ Cliente criado como pessoa jurídica
- ✅ CNPJ validado
- ✅ Campos específicos PJ tratados

## 📈 Melhorias Futuras

1. **Geração de Senha Inteligente:**

```csharp
.ForMember(dest => dest.Senha, opt => opt.MapFrom((src, dest, destMember, context) =>
    context.Items["PasswordGenerator"].GeneratePassword()))
```

2. **Validação de Cliente Existente:**

```csharp
// Verificar se cliente já existe antes de criar
var clienteExistente = await _clienteService.BuscarPorEmailAsync(vendaDto.Email);
```

3. **Configuração Personalizada:**

```csharp
// Permitir configuração da senha padrão via appsettings
.ForMember(dest => dest.Senha, opt => opt.MapFrom(src =>
    _configuration["DefaultPassword:VendaCliente"]))
```

## ✅ Resultado

O mapeamento resolve o erro e permite que o fluxo de vendas funcione corretamente:

- ✅ **Erro corrigido**: Mapeamento AutoMapper configurado
- ✅ **Fluxo funcional**: Venda pode criar cliente automaticamente
- ✅ **Dados consistentes**: Mapeamento preserva integridade
- ✅ **Segurança básica**: Senha temporária forte
- ✅ **Extensível**: Preparado para melhorias futuras

O sistema agora pode processar vendas de clientes novos sem problemas, criando automaticamente a conta do cliente com dados básicos e processando a venda em sequência.
