# Correção: Query String de Email no CelPay

## Problema Identificado

A requisição para buscar cliente por email estava sendo enviada com o `@` codificado como `%40`:

```
❌ ANTES: https://api.sandbox.cel.cash/v2/customers?emails=teste%40teste.com.br&startAt=0&limit=100
```

## Solução Implementada

Modificado o método `BuscarClientePorEmailAsync` para usar string interpolation direta, evitando a codificação automática do `@`:

```csharp
// ❌ ANTES - Usando HttpUtility.ParseQueryString (codifica o @)
var queryParams = System.Web.HttpUtility.ParseQueryString(string.Empty);
queryParams["emails"] = email;
queryParams["startAt"] = "0";
queryParams["limit"] = "100";
var response = await _httpClient.GetAsync($"customers?{queryParams}");

// ✅ DEPOIS - Usando string interpolation direta
var queryString = $"customers?emails={email}&startAt=0&limit=100";
var response = await _httpClient.GetAsync(queryString);
```

## Resultado

Agora a requisição é enviada corretamente com o `@` literal:

```
✅ DEPOIS: https://api.sandbox.cel.cash/v2/customers?emails=teste@teste.com.br&startAt=0&limit=100
```

## Logs Adicionados

Para facilitar o debugging, foi adicionado um log da URL completa:

```csharp
_logger.LogInformation("URL da requisição: {Url}", $"{_baseUrl}{queryString}");
```

## Exemplo de Log

```
[INFO] Buscando cliente por email via CelPay: teste@teste.com.br
[INFO] URL da requisição: https://api.sandbox.cel.cash/v2/customers?emails=teste@teste.com.br&startAt=0&limit=100
[INFO] Busca de cliente realizada com sucesso via CelPay. Clientes encontrados: 1
```

## Teste da Correção

### Request de Teste
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

### URL Gerada
```
https://api.sandbox.cel.cash/v2/customers?emails=joao.silva@email.com&startAt=0&limit=100
```

## Vantagens da Correção

1. **✅ URL Correta**: O `@` é enviado literalmente, não codificado
2. **✅ Compatibilidade**: Funciona corretamente com a API do CelPay
3. **✅ Debugging**: Logs mostram a URL exata sendo enviada
4. **✅ Simplicidade**: Código mais limpo e direto

## Considerações Técnicas

### Por que o HttpUtility.ParseQueryString codifica?
- O `HttpUtility.ParseQueryString` automaticamente codifica caracteres especiais
- O `@` é considerado um caractere especial em URLs
- A codificação `%40` é tecnicamente correta para URLs, mas a API do CelPay espera o `@` literal

### Por que string interpolation funciona?
- String interpolation não aplica codificação automática
- O `@` é mantido literal na string
- A API do CelPay recebe o email no formato esperado

## Conclusão

A correção garante que a busca de cliente por email funcione corretamente com a API do CelPay, enviando o email no formato literal esperado pela API.

🎉 **Problema resolvido! A busca de cliente agora funciona corretamente.**
