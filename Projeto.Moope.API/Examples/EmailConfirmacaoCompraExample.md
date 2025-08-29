# Exemplo de Email de Confirmação de Compra

## Visão Geral

Este exemplo demonstra como a funcionalidade de email de confirmação de compra funciona no sistema Moope. Quando um usuário processa uma compra (novo ou existente), um email é automaticamente salvo no banco de dados para envio posterior.

## Fluxo de Funcionamento

### 1. Processamento da Venda

Quando uma venda é processada através do endpoint `POST /api/venda/processar`, o sistema:

1. Valida os dados da venda
2. Cria ou identifica o cliente
3. Processa o pagamento via gateway
4. Cria o pedido e transações
5. **Salva automaticamente um email de confirmação**

### 2. Dados do Email

O email contém informações diferentes dependendo se é um cliente novo ou existente:

#### Para Clientes Novos:
- Dados da conta criada (nome, email, telefone, CPF/CNPJ)
- Dados da compra (plano, valor, quantidade)
- Status da assinatura
- Informações do vendedor (se aplicável)
- Endereço (se fornecido)

#### Para Clientes Existentes:
- Dados da compra (plano, valor, quantidade)
- Status da assinatura
- Informações do vendedor (se aplicável)

### 3. Template do Email

O email é gerado em HTML com:
- Cabeçalho personalizado
- Informações da compra em formato organizado
- Status da assinatura com cores indicativas
- Dados da conta (apenas para clientes novos)
- Informações do vendedor
- Próximos passos baseados no status

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
  "vendedorId": "550e8400-e29b-41d4-a716-446655440000",
  "planoId": "550e8400-e29b-41d4-a716-446655440001",
  "quantidade": 1,
  "nomeCartao": "João Silva",
  "numeroCartao": "4111111111111111",
  "cvv": "123",
  "dataValidade": "12/25"
}
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

### Email Salvo no Banco

Após o processamento bem-sucedido, um registro é criado na tabela `Emails` com:

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440004",
  "remetente": "noreply@moope.com.br",
  "nomeRemetente": "Moope",
  "destinatario": "joao.silva@email.com",
  "nomeDestinatario": "João Silva",
  "assunto": "Bem-vindo! Confirmação de compra - Plano Premium",
  "corpo": "<html>...template HTML completo...</html>",
  "ehHtml": true,
  "prioridade": "Normal",
  "status": "Pendente",
  "clienteId": "550e8400-e29b-41d4-a716-446655440003",
  "tipo": "CONFIRMACAO_COMPRA",
  "dadosAdicionais": "{\"pedidoId\":\"550e8400-e29b-41d4-a716-446655440002\",\"planoId\":\"550e8400-e29b-41d4-a716-446655440001\",\"valorTotal\":99.90,\"quantidade\":1,\"statusAssinatura\":\"Active\",\"clienteNovo\":true,\"dataCompra\":\"2024-01-15T10:30:00Z\",\"vendedorId\":\"550e8400-e29b-41d4-a716-446655440000\",\"nomeVendedor\":\"Maria Vendedora\"}",
  "created": "2024-01-15T10:30:00Z"
}
```

## Processamento de Emails

Os emails salvos podem ser processados posteriormente por:

1. **Serviço de Email**: Para envio imediato
2. **Job/Background Service**: Para processamento em lote
3. **API de Processamento**: Para processamento manual

### Exemplo de Processamento

```csharp
// Buscar emails pendentes
var emailsPendentes = await _emailRepository.BuscarPendentesAsync();

foreach (var email in emailsPendentes)
{
    try
    {
        // Tentar enviar
        var sucesso = await _emailService.EnviarEmailAsync(email);
        
        if (sucesso)
        {
            await _emailRepository.MarcarComoEnviadoAsync(email.Id);
        }
        else
        {
            await _emailRepository.AtualizarStatusAsync(email.Id, StatusEmail.Falha, "Falha no envio");
        }
    }
    catch (Exception ex)
    {
        await _emailRepository.AtualizarStatusAsync(email.Id, StatusEmail.Falha, ex.Message);
    }
}
```

## Vantagens da Implementação

1. **Não Bloqueia o Processamento**: O email é salvo de forma assíncrona
2. **Templates Dinâmicos**: Conteúdo personalizado para clientes novos/existentes
3. **Rastreabilidade**: Todos os emails ficam registrados no banco
4. **Retry Automático**: Sistema pode reprocessar emails com falha
5. **Auditoria**: Histórico completo de comunicações
6. **Flexibilidade**: Pode ser processado por diferentes serviços

## Configurações

### Email do Remetente
O email do remetente pode ser configurado no `SalvarEmailConfirmacaoCompraCommandHandler`:

```csharp
Remetente = "noreply@moope.com.br", // Configurar email padrão
NomeRemetente = "Moope",
```

### Templates
Os templates são gerados dinamicamente no método `GerarCorpoEmail()` e podem ser customizados conforme necessário.

### Status de Envio
Os emails são salvos com status `Pendente` e podem ser processados posteriormente pelos serviços de email configurados.
