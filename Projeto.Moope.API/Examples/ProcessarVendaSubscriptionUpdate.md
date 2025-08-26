# ProcessarVendaCommandHandler - Atualização para Subscriptions

## Alterações Implementadas

O `ProcessarVendaCommandHandler` foi atualizado para usar o método `CriarSubscriptionComPlanoAsync` em vez do `ProcessarPagamentoAsync`, transformando o fluxo de venda única em venda de assinatura/subscription.

## 🔄 Principais Mudanças

### **1. Método de Pagamento Alterado**

**Antes:**

```csharp
var vendaStoreDto = new VendaStoreDto { /* dados da venda */ };
var resultadoPagamento = await _paymentGateway.ProcessarPagamentoAsync(vendaStoreDto);
```

**Depois:**

```csharp
var subscriptionDto = new CelPaySubscriptionRequestDto { /* dados da subscription */ };
var resultadoPagamento = await _paymentGateway.CriarSubscriptionComPlanoAsync(subscriptionDto);
```

### **2. Status de Sucesso Atualizados**

**Antes:**

```csharp
if (resultadoPagamento.Status == "APPROVED" || resultadoPagamento.Status == "SUCCESS")
```

**Depois:**

```csharp
if (resultadoPagamento.Status == "ACTIVE" || resultadoPagamento.Status == "PENDING")
```

### **3. Método de Pagamento na Transação**

**Antes:**

```csharp
MetodoPagamento = "CARTAO_CREDITO"
```

**Depois:**

```csharp
MetodoPagamento = "SUBSCRIPTION"
```

## 📊 Estrutura da Subscription

### **CelPaySubscriptionRequestDto Construído:**

```csharp
var subscriptionDto = new CelPaySubscriptionRequestDto
{
    ExternalId = pedido.Id.ToString(),
    PlanId = plano.Codigo, // Usa o código do plano
    Card = new CardInfo
    {
        Number = request.NumeroCartao,
        ExpMonth = ExtrairMesValidade(request.DataValidade),
        ExpYear = ExtrairAnoValidade(request.DataValidade),
        Cvv = request.Cvv,
        HolderName = request.NomeCliente
    },
    Customer = new CustomerInfo
    {
        Name = request.NomeCliente,
        Email = request.Email,
        Phone = request.Telefone
    },
    Description = request.Descricao ?? $"Assinatura {plano.Descricao} - {request.NomeCliente}",
    StartDate = DateTime.UtcNow,
    Metadata = new SubscriptionMetadata
    {
        ClienteId = request.ClienteId?.ToString(),
        VendedorId = request.VendedorId != Guid.Empty ? request.VendedorId.ToString() : null,
        Observacoes = $"Pedido: {pedido.Id}"
    }
};
```

## 🔧 Novos Métodos Auxiliares

### **ExtrairMesValidade()**

```csharp
private string ExtrairMesValidade(string dataValidade)
{
    var partes = dataValidade.Split('/');
    if (partes.Length == 2)
    {
        return partes[0]; // Retorna MM
    }

    throw new ArgumentException("Formato de data de validade inválido. Use MM/YY");
}
```

### **ExtrairAnoValidade()**

```csharp
private string ExtrairAnoValidade(string dataValidade)
{
    var partes = dataValidade.Split('/');
    if (partes.Length == 2)
    {
        return "20" + partes[1]; // Retorna 20YY
    }

    throw new ArgumentException("Formato de data de validade inválido. Use MM/YY");
}
```

## 🎯 Mapeamento de Dados

### **Do Command para Subscription:**

| Campo Command  | Campo Subscription    | Observações               |
| -------------- | --------------------- | ------------------------- |
| `pedido.Id`    | `ExternalId`          | ID único do pedido        |
| `plano.Codigo` | `PlanId`              | Código do plano na CelPay |
| `NumeroCartao` | `Card.Number`         | Dados do cartão           |
| `DataValidade` | `ExpMonth/ExpYear`    | Separado em mês/ano       |
| `Cvv`          | `Card.Cvv`            | CVV do cartão             |
| `NomeCliente`  | `Card.HolderName`     | Nome no cartão            |
| `Email`        | `Customer.Email`      | Email do cliente          |
| `Telefone`     | `Customer.Phone`      | Telefone do cliente       |
| `Descricao`    | `Description`         | Descrição da assinatura   |
| `ClienteId`    | `Metadata.ClienteId`  | ID do cliente (metadata)  |
| `VendedorId`   | `Metadata.VendedorId` | ID do vendedor (metadata) |

## 🚦 Status de Subscription

### **Status de Sucesso:**

- `ACTIVE` - Subscription ativa e funcionando
- `PENDING` - Aguardando processamento/ativação

### **Status de Falha:**

- `ERROR` - Erro no processamento
- `CANCELLED` - Cancelada
- `SUSPENDED` - Suspensa
- Qualquer outro status não listado acima

## 📋 Fluxo Atualizado

1. **Validar vendedor** (se informado)
2. **Validar plano** (existência e status ativo)
3. **Calcular valor total** (plano.Valor × quantidade)
4. **Criar pedido** com snapshot do plano
5. **Criar subscription** via CelPay com plano
6. **Processar resultado:**
   - **Sucesso** (`ACTIVE`/`PENDING`): Pedido = `APROVADO`, Transação = `SUBSCRIPTION`
   - **Falha**: Pedido = `REJEITADO`, Notificação de erro

## ✅ Benefícios da Alteração

### **1. Modelo de Negócio Atualizado**

- ✅ Suporte a assinaturas recorrentes
- ✅ Integração direta com planos da CelPay
- ✅ Cobrança automática conforme plano

### **2. Rastreabilidade Melhorada**

- ✅ ExternalId vincula pedido à subscription
- ✅ Metadata preserva relações cliente/vendedor
- ✅ Observações incluem ID do pedido

### **3. Flexibilidade de Planos**

- ✅ Usa código do plano como PlanId
- ✅ Planos são gerenciados no CelPay
- ✅ Mudanças de plano via API

### **4. Dados Estruturados**

- ✅ Separação correta de mês/ano validade
- ✅ Informações completas do cliente
- ✅ Metadata extensível

## 🧪 Exemplos de Uso

### **Requisição de Venda (Igual):**

```json
{
  "nomeCliente": "João Silva",
  "email": "joao@email.com",
  "telefone": "(11) 99999-9999",
  "cpfCnpj": "12345678901",
  "planoId": "550e8400-e29b-41d4-a716-446655440000",
  "quantidade": 1,
  "numeroCartao": "4111111111111111",
  "cvv": "123",
  "dataValidade": "12/25",
  "vendedorId": "660e8400-e29b-41d4-a716-446655440000"
}
```

### **Resposta de Sucesso:**

```json
{
  "status": true,
  "mensagem": "Subscription criada com sucesso",
  "dados": {
    "id": "770e8400-e29b-41d4-a716-446655440000",
    "clienteId": "880e8400-e29b-41d4-a716-446655440000",
    "vendedorId": "660e8400-e29b-41d4-a716-446655440000",
    "planoId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "APROVADO",
    "total": 99.9
  }
}
```

## 🔄 Compatibilidade

### **Mantido:**

- ✅ Interface do ProcessarVendaCommand inalterada
- ✅ Validações de vendedor/plano mantidas
- ✅ Estrutura de pedido preservada
- ✅ Sistema de notificações igual

### **Alterado:**

- ✅ Gateway usa subscription em vez de pagamento único
- ✅ Status de sucesso específicos para subscription
- ✅ Método de pagamento atualizado para "SUBSCRIPTION"
- ✅ Mensagens de erro específicas para subscription

## 📈 Próximos Passos

1. **Webhook Integration**: Receber notificações de status da subscription
2. **Gestão de Planos**: CRUD de planos no CelPay
3. **Dashboard**: Visualização de subscriptions ativas
4. **Relatórios**: Métricas de assinaturas e churn
5. **Notificações**: Avisos de renovação/falha de cobrança

A alteração transforma o sistema de vendas únicas em um modelo de subscription robusto, mantendo compatibilidade com a interface existente e adicionando todas as funcionalidades necessárias para gestão de assinaturas.
