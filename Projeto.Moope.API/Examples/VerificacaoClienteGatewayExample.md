# Verificação de Cliente no Gateway de Pagamento

## Visão Geral

Esta implementação adiciona a funcionalidade de verificar se um cliente já está cadastrado no gateway de pagamento (CelPay) antes de criar uma subscription. Se o cliente não existir, ele será criado automaticamente no gateway.

## Funcionalidades Implementadas

### 1. **DTOs para Cliente**
- `CelPayCustomerResponseDto` - Resposta de busca/criação de cliente
- `CelPayCustomerRequestDto` - Dados para criação de cliente
- `CelPayCustomerDto` - Dados do cliente no gateway
- `CelPayAddressDto` - Endereço do cliente

### 2. **Métodos no Gateway**
- `BuscarClientePorEmailAsync(string email)` - Busca cliente por email
- `CriarClienteAsync(CelPayCustomerRequestDto customerDto)` - Cria novo cliente

### 3. **Integração no ProcessarVendaCommandHandler**
- Verificação automática de cliente antes de criar subscription
- Criação automática de cliente se não existir
- Logs detalhados do processo

## Fluxo de Funcionamento

### 1. **Processamento da Venda**
```
VendaController.ProcessarVenda()
├── ProcessarVendaCommandHandler.Handle()
│   ├── 1. Validar vendedor e plano
│   ├── 2. Criar pedido
│   ├── 3. 🔍 VERIFICAR CLIENTE NO GATEWAY
│   │   ├── BuscarClientePorEmailAsync()
│   │   └── Se não existir: CriarClienteAsync()
│   ├── 4. Criar subscription com plano
│   └── 5. Processar transações
└── SalvarEmailConfirmacaoCompra()
```

### 2. **Verificação de Cliente**
```csharp
// Verificar se o cliente já existe no gateway
var clienteExisteNoGateway = await _paymentGateway.BuscarClientePorEmailAsync(request.Email);

if (clienteExisteNoGateway.Type && clienteExisteNoGateway.HasCustomers)
{
    // Cliente já existe - usar ID existente
    customerId = clienteExisteNoGateway.FirstCustomer?.GalaxPayId.ToString();
}
else
{
    // Cliente não existe - criar no gateway
    var customerDto = new CelPayCustomerRequestDto
    {
        MyId = request.ClienteId?.ToString(),
        Name = request.NomeCliente,
        Document = request.CpfCnpj,
        Emails = new List<string> { request.Email },
        Phones = new List<string> { request.Telefone }
    };
    
    var resultadoCriacaoCliente = await _paymentGateway.CriarClienteAsync(customerDto);
    customerId = resultadoCriacaoCliente.FirstCustomer?.GalaxPayId.ToString();
}
```

## Documentação da API CelPay

Baseado na [documentação oficial do CelPay](https://docs-celpayments.celcoin.com.br/customers/list):

### **GET /customers** - Listar Clientes
- **Scope necessário**: `customers.read`
- **Parâmetros de busca**: `emails`, `documents`, `phones`
- **Resposta**: Lista de clientes encontrados

### **POST /customers** - Criar Cliente
- **Scope necessário**: `customers.write`
- **Body**: Dados do cliente (nome, documento, emails, telefones, endereço)
- **Resposta**: Cliente criado com ID do gateway

## Exemplo de Uso

### Request de Venda
```http
POST /api/venda/processar
Content-Type: application/json

{
  "nomeCliente": "João Silva",
  "email": "joao.silva@email.com",
  "telefone": "(11) 99999-9999",
  "tipoPessoa": 1,
  "cpfCnpj": "123.456.789-00",
  "planoId": "550e8400-e29b-41d4-a716-446655440001",
  "quantidade": 1,
  "nomeCartao": "João Silva",
  "numeroCartao": "4111111111111111",
  "cvv": "123",
  "dataValidade": "12/25"
}
```

### Logs do Processo
```
[INFO] Buscando cliente por email via CelPay: joao.silva@email.com
[INFO] Cliente encontrado no gateway CelPay. CustomerId: 12345
[INFO] Criando subscription via CelPay para plano: PLANO_PREMIUM
[INFO] Subscription criada com sucesso via CelPay. SubscriptionId: 67890
```

### Resposta
```json
{
  "status": true,
  "mensagem": "Pedido criado com sucesso!",
  "dados": {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "clienteId": "550e8400-e29b-41d4-a716-446655440003",
    "planoId": "550e8400-e29b-41d4-a716-446655440001",
    "total": 99.90,
    "statusAssinatura": "Active",
    "created": "2024-01-15T10:30:00Z"
  }
}
```

## Cenários de Teste

### 1. **Cliente Novo (Primeira Compra)**
- Cliente não existe no gateway
- Sistema cria cliente automaticamente
- Subscription é criada com sucesso

### 2. **Cliente Existente (Compra Recorrente)**
- Cliente já existe no gateway
- Sistema usa ID existente
- Subscription é criada com sucesso

### 3. **Erro na Criação de Cliente**
- Falha na criação do cliente no gateway
- Processamento da venda é interrompido
- Erro é retornado ao usuário

## Vantagens da Implementação

### 1. **Consistência de Dados**
- Cliente sempre existe no gateway antes da subscription
- Evita erros de referência inválida
- Sincronização automática entre sistemas

### 2. **Experiência do Usuário**
- Processo transparente para o cliente
- Não há necessidade de cadastro manual
- Criação automática quando necessário

### 3. **Robustez**
- Tratamento de erros específicos
- Logs detalhados para debugging
- Fallback em caso de falha

### 4. **Performance**
- Busca otimizada por email
- Cache de cliente no gateway
- Evita duplicação de dados

## Configurações Necessárias

### 1. **Scopes do CelPay**
```json
{
  "CelPay": {
    "Scope": "customers.read customers.write subscriptions.read subscriptions.write"
  }
}
```

### 2. **Logs**
```json
{
  "Logging": {
    "LogLevel": {
      "Projeto.Moope.Infrastructure.Services.Pagamentos.CelPayGatewayStrategy": "Information"
    }
  }
}
```

## Monitoramento

### 1. **Métricas Importantes**
- Taxa de sucesso na busca de clientes
- Taxa de sucesso na criação de clientes
- Tempo de resposta das operações
- Erros de integração com gateway

### 2. **Alertas Recomendados**
- Falha na busca de cliente
- Falha na criação de cliente
- Timeout nas operações
- Taxa de erro > 5%

## Conclusão

A implementação garante que todos os clientes estejam devidamente cadastrados no gateway de pagamento antes de processar subscriptions, melhorando a confiabilidade e consistência do sistema de pagamentos.

A integração segue as melhores práticas de:
- **Tratamento de erros robusto**
- **Logs detalhados**
- **Separação de responsabilidades**
- **Reutilização de código**
- **Documentação clara**

🎉 **Sistema mais robusto e confiável para processamento de pagamentos!**
