# Sistema de Vendas - Moope API

## Visão Geral

Este módulo implementa uma rotina completa de vendas utilizando o padrão Strategy para integração com gateways de pagamento. Atualmente está configurado para trabalhar com o **CelPay** como gateway de pagamento principal.

## Arquitetura

### Padrão Strategy
- **Interface**: `IPaymentGatewayStrategy`
- **Implementação**: `CelPayGatewayStrategy`
- **Benefícios**: Facilita a troca de gateways de pagamento sem alterar o código principal

### Estrutura de Camadas
```
API Layer (Controllers)
    ↓
Service Layer (VendaService)
    ↓
Strategy Layer (PaymentGateway)
    ↓
Infrastructure Layer (HTTP Client, Database)
```

## Funcionalidades

### 1. Processamento de Vendas
- Recebe dados de pagamento (cartão de crédito, cliente, valor)
- Recebe informações do plano e quantidade
- Valida informações usando FluentValidation
- Valida existência e status do plano
- Calcula valor total baseado no plano e quantidade
- Processa pagamento via gateway
- Cria pedido e transação no banco
- Retorna resultado do processamento

### 2. Consultas
- Consulta venda por ID
- Lista vendas por vendedor
- Lista vendas por cliente
- Health check da API

### 3. Validações
- Validação de cartão de crédito (algoritmo Luhn)
- Validação de data de validade
- Validação de formato de dados
- Validação de valores e limites

## Configuração

### 1. Arquivo appsettings.json
```json
{
  "CelPay": {
    "BaseUrl": "https://api.celpayments.com.br",
    "ApiKey": "sua_api_key_do_celpay_aqui",
    "TimeoutSeconds": 30
  }
}
```

### 2. Dependências
- HttpClient configurado para CelPay
- Timeout configurável
- Headers de autenticação automáticos

## Endpoints da API

### POST /api/venda/processar
Processa uma nova venda com pagamento.

**Request Body:**
```json
{
  "nomeCliente": "João Silva",
  "numeroCartao": "4111111111111111",
  "cvv": "123",
  "dataValidade": "12/25",
  "email": "joao.silva@email.com",
  "telefone": "(11) 99999-9999",
  "valor": 150.00,
  "vendedorId": "guid-do-vendedor",
  "planoId": "guid-do-plano",
  "quantidade": 1,
  "descricao": "Compra de plano"
}
```

**Response:**
```json
{
  "id": "guid-da-venda",
  "status": "APROVADA",
  "mensagem": "Venda processada com sucesso",
  "codigoTransacao": "celpay-transaction-id",
  "dataProcessamento": "2024-01-15T10:30:00Z",
  "valor": 150.00,
  "nomeCliente": "João Silva",
  "email": "joao.silva@email.com",
  "vendedorId": "guid-do-vendedor",
  "planoId": "guid-do-plano",
  "nomePlano": "Plano Premium",
  "codigoPlano": "PLANO001",
  "valorUnitarioPlano": 150.00,
  "quantidade": 1,
  "sucesso": true
}
```

### GET /api/venda/{vendaId}
Consulta uma venda específica.

### GET /api/venda/vendedor/{vendedorId}
Lista todas as vendas de um vendedor.

### GET /api/venda/cliente/{email}
Lista todas as vendas de um cliente.

### GET /api/venda/health
Verifica o status da API (endpoint público).

## Modelos de Dados

### CreateVendaDto
- Dados de entrada para processamento de venda
- Inclui validações de cartão e dados pessoais
- Campos obrigatórios: `planoId` e `quantidade`

### VendaResponseDto
- Resposta padronizada para todas as operações
- Inclui status, mensagens e dados da transação
- **Snapshot do plano**: Preserva informações do momento da venda

### CelPayRequestDto / CelPayResponseDto
- DTOs específicos para integração com CelPay
- Mapeamento automático de dados

## Estratégia de Snapshot de Plano

### Problema Resolvido
Quando um plano tem seu valor alterado após uma venda, o pedido mantém o histórico correto do momento da compra.

### Implementação
- **ValorUnitarioPlano**: Valor unitário no momento da venda
- **DescricaoPlano**: Descrição do plano no momento da venda  
- **CodigoPlano**: Código do plano no momento da venda
- **Total**: Calculado automaticamente (ValorUnitarioPlano × Quantidade)

### Exemplo
```
Plano A: $10 (momento da venda)
Quantidade: 2
Total: $20

Se o Plano A for alterado para $15 posteriormente:
- Pedido continua mostrando: $10 × 2 = $20 ✅
- Plano atual mostra: $15 (para novas vendas)
```

## Fluxo de Processamento

1. **Validação**: Dados de entrada são validados
2. **Criação de Pedido**: Pedido é criado com status "PENDENTE"
3. **Processamento de Pagamento**: Gateway processa o pagamento
4. **Atualização de Status**: Pedido é atualizado baseado no resultado
5. **Criação de Transação**: Transação é criada se aprovada
6. **Resposta**: Resultado é retornado ao cliente

## Tratamento de Erros

- Validações de entrada com mensagens claras
- Tratamento de erros de gateway
- Logs detalhados para debugging
- Respostas padronizadas de erro

## Segurança

- Autenticação JWT obrigatória para endpoints sensíveis
- Validação de dados de entrada
- Não armazenamento de dados sensíveis do cartão
- Logs de auditoria para transações

## Testes

### Arquivo HTTP
Use o arquivo `Projeto.Moope.API.http` para testar os endpoints:
- Configure as variáveis de ambiente
- Teste cada endpoint individualmente
- Verifique as respostas e códigos de status

### Validações
- Teste com dados válidos e inválidos
- Verifique mensagens de erro
- Teste limites de valores e formatos

## Extensibilidade

### Adicionar Novo Gateway
1. Implementar `IPaymentGatewayStrategy`
2. Criar DTOs específicos do gateway
3. Registrar no container de DI
4. Configurar no appsettings.json

### Exemplo de Novo Gateway
```csharp
public class StripeGatewayStrategy : IPaymentGatewayStrategy
{
    // Implementação específica do Stripe
}
```

## Monitoramento

- Logs de todas as transações
- Métricas de sucesso/falha
- Timeout configurável
- Health checks automáticos

## Próximos Passos

1. **Implementar Webhooks**: Para notificações assíncronas do gateway
2. **Adicionar Retry Logic**: Para falhas temporárias de rede
3. **Implementar Cache**: Para consultas frequentes
4. **Adicionar Métricas**: Para monitoramento de performance
5. **Implementar Testes Unitários**: Para garantir qualidade do código

## Suporte

Para dúvidas ou problemas:
1. Verifique os logs da aplicação
2. Teste o endpoint de health check
3. Valide as configurações do CelPay
4. Verifique a conectividade com o gateway
