using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Projeto.Moope.API.DTOs;
using Projeto.Moope.Core.DTOs.Pagamentos;
using Projeto.Moope.Core.Interfaces.Pagamentos;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Projeto.Moope.Infrastructure.Services.Pagamentos
{
    public class CelPayGatewayStrategy : IPaymentGatewayStrategy
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CelPayGatewayStrategy> _logger;
        private CelPayAuthResponseDto? _cachedToken;
        private readonly object _tokenLock = new object();

        public string NomeGateway => "CelPay";
        private readonly string _baseUrl;
        public CelPayGatewayStrategy(
            HttpClient httpClient, 
            IConfiguration configuration,
            ILogger<CelPayGatewayStrategy> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            // Configurar base URL
            var baseUrl = _configuration["CelPay:IsProduction"] == "true" 
                ? _configuration["CelPay:BaseUrl"] 
                : _configuration["CelPay:BaseUrlSandbox"];

            _baseUrl = baseUrl;
            _httpClient.BaseAddress = new Uri(baseUrl ?? "https://api.sandbox.cel.cash/v2/");
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            
            var timeoutSeconds = int.Parse(_configuration["CelPay:TimeoutSeconds"] ?? "30");
            _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        }

        /// <summary>
        /// Autentica na API do CelPay e obtém o token de acesso
        /// </summary>
        private async Task<CelPayAuthResponseDto> ObterTokenAsync()
        {
            lock (_tokenLock)
            {
                // Verifica se já temos um token válido em cache
                if (_cachedToken?.IsTokenValido == true)
                {
                    return _cachedToken;
                }
            }

            try
            {
                var authConfig = ObterConfiguracaoAuth();
                var credentials = $"{authConfig.GalaxId}:{authConfig.GalaxHash}";
                var base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
                
                var endpoint = "token";

                var jsonBody = $@"{{
                  ""grant_type"": ""authorization_code"",
                  ""scope"": ""{authConfig.Scope}""
                }}";

                using var http = new HttpClient { BaseAddress = new Uri(_baseUrl) };

                var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
                req.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                
                using var resp = await http.SendAsync(req);
                var body = await resp.Content.ReadAsStringAsync();
                if (resp.IsSuccessStatusCode)
                {
                    var authResponse = JsonSerializer.Deserialize<CelPayAuthResponseDto>(body, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                
                    if (authResponse != null)
                    {
                        authResponse.ObtidoEm = DateTime.UtcNow;
                        
                        lock (_tokenLock)
                        {
                            _cachedToken = authResponse;
                        }
                
                        _logger.LogInformation("Token obtido com sucesso. Expira em {ExpiresIn} segundos", authResponse.ExpiresIn);
                        return authResponse;
                    }
                }
                resp.EnsureSuccessStatusCode();
                _logger.LogError("Erro ao obter token. Status: {Status}, Response: {Response}", resp.StatusCode, body);
                throw new InvalidOperationException($"Falha na autenticação CelPay: {resp.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao obter token do CelPay");
                throw;
            }
        }

        /// <summary>
        /// Configura o HttpClient com o token de autorização Bearer
        /// </summary>
        private async Task ConfigurarAutorizacaoAsync()
        {
            var token = await ObterTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }
        
        private CelPayAuthConfigDto ObterConfiguracaoAuth()
        {
            return new CelPayAuthConfigDto
            {
                GalaxId = _configuration["CelPay:GalaxId"] ?? throw new InvalidOperationException("CelPay:GalaxId não configurado"),
                GalaxHash = _configuration["CelPay:GalaxHash"] ?? throw new InvalidOperationException("CelPay:GalaxHash não configurado"),
                GalaxIdPartner = _configuration["CelPay:GalaxIdPartner"],
                GalaxHashPartner = _configuration["CelPay:GalaxHashPartner"],
                BaseUrl = _httpClient.BaseAddress?.ToString() ?? "",
                IsProduction = bool.Parse(_configuration["CelPay:IsProduction"] ?? "false"),
                Scope = _configuration["CelPay:Scope"] ?? "",
            };
        }

        public async Task<CelPayResponseDto> ProcessarPagamentoAsync(VendaStoreDto vendaDto)
        {
            try
            {
                // Configurar autenticação antes de fazer a requisição
                await ConfigurarAutorizacaoAsync();

                var requestDto = MapearParaCelPayRequest(vendaDto);
                var jsonContent = JsonSerializer.Serialize(requestDto);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Processando pagamento via CelPay para venda: {VendaId}", requestDto.ExternalId);

                var response = await _httpClient.PostAsync("/charges", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var celPayResponse = JsonSerializer.Deserialize<CelPayResponseDto>(responseContent);
                    _logger.LogInformation("Pagamento processado com sucesso via CelPay. TransactionId: {TransactionId}", celPayResponse?.Id);
                    return celPayResponse ?? new CelPayResponseDto { Status = "ERROR", ErrorMessage = "Resposta inválida do gateway" };
                }
                else
                {
                    _logger.LogError("Erro ao processar pagamento via CelPay. Status: {Status}, Response: {Response}", response.StatusCode, responseContent);
                    return new CelPayResponseDto 
                    { 
                        Status = "ERROR", 
                        ErrorMessage = $"Erro HTTP: {response.StatusCode}",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao processar pagamento via CelPay");
                return new CelPayResponseDto 
                { 
                    Status = "ERROR", 
                    ErrorMessage = "Erro interno do sistema",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<CelPayResponseDto> ConsultarTransacaoAsync(string transactionId)
        {
            try
            {
                // Configurar autenticação antes de fazer a requisição
                await ConfigurarAutorizacaoAsync();

                var response = await _httpClient.GetAsync($"/charges/{transactionId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var celPayResponse = JsonSerializer.Deserialize<CelPayResponseDto>(responseContent);
                    return celPayResponse ?? new CelPayResponseDto { Status = "ERROR", ErrorMessage = "Resposta inválida do gateway" };
                }
                else
                {
                    _logger.LogError("Erro ao consultar transação via CelPay. Status: {Status}, Response: {Response}", response.StatusCode, responseContent);
                    return new CelPayResponseDto 
                    { 
                        Status = "ERROR", 
                        ErrorMessage = $"Erro HTTP: {response.StatusCode}",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao consultar transação via CelPay");
                return new CelPayResponseDto 
                { 
                    Status = "ERROR", 
                    ErrorMessage = "Erro interno do sistema",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        private CelPayRequestDto MapearParaCelPayRequest(VendaStoreDto vendaDto)
        {
            var (mes, ano) = ExtrairMesAnoValidade(vendaDto.DataValidade);
            
            return new CelPayRequestDto
            {
                ExternalId = Guid.NewGuid().ToString(),
                Amount = vendaDto.Valor,
                Currency = "BRL",
                
                Card = new CardInfo
                {
                    Number = vendaDto.NumeroCartao,
                    ExpMonth = mes,
                    ExpYear = ano,
                    Cvv = vendaDto.Cvv,
                    HolderName = vendaDto.NomeCliente
                },
                Customer = new CustomerInfo
                {
                    Name = vendaDto.NomeCliente,
                    Emails = new string[] { vendaDto.Email }
                },
                Description = vendaDto.Descricao ?? $"Venda para {vendaDto.NomeCliente}",
                Capture = "true",
                Installments = "1"
            };
        }

        private (string mes, string ano) ExtrairMesAnoValidade(string dataValidade)
        {
            var partes = dataValidade.Split('/');
            if (partes.Length == 2)
            {
                var mes = partes[0];
                var ano = "20" + partes[1]; // Assumindo formato MM/YY
                return (mes, ano);
            }
            
            throw new ArgumentException("Formato de data de validade inválido. Use MM/YY");
        }

        public async Task<CelPaySubscriptionResponseDto> CriarSubscriptionComPlanoAsync(CelPaySubscriptionRequestDto subscriptionDto)
        {
            try
            {
                // Configurar autenticação antes de fazer a requisição
                await ConfigurarAutorizacaoAsync();

                var jsonContent = JsonSerializer.Serialize(subscriptionDto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Criando subscription via CelPay para plano: {PlanId}", subscriptionDto.PlanId);

                var response = await _httpClient.PostAsync("subscriptions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var successResponse = JsonConvert.DeserializeObject<CelPaySubscriptionSuccessResponseDto>(responseContent);
                        
                        if (successResponse?.Type == true && successResponse.Subscription != null)
                        {
                            // Converter para o formato de resposta padrão
                            var subscriptionResponse = new CelPaySubscriptionResponseDto
                            {
                                GalaxPayId = successResponse.Subscription.GalaxPayId.ToString(),
                                Status = successResponse.Subscription.Status,
                                ExternalId = successResponse.Subscription.MyId ?? string.Empty,
                                CreatedAt = DateTime.Parse(successResponse.Subscription.CreatedAt),
                                UpdatedAt = DateTime.Parse(successResponse.Subscription.UpdatedAt),
                                Description = $"Subscription criada com sucesso. GalaxPayId: {successResponse.Subscription.GalaxPayId}",
                                Transactions = successResponse.Subscription.Transactions
                            };
                            
                            _logger.LogInformation("Subscription criada com sucesso via CelPay. SubscriptionId: {SubscriptionId}", subscriptionResponse.GalaxPayId);
                            return subscriptionResponse;
                        }
                    }
                    catch (JsonException)
                    {
                        _logger.LogWarning("Falha ao deserializar resposta de sucesso, tentando formato padrão");
                    }
                    
                    var fallbackResponse = JsonConvert.DeserializeObject<CelPaySubscriptionResponseDto>(responseContent);
                    
                    _logger.LogInformation("Subscription criada com sucesso via CelPay. SubscriptionId: {SubscriptionId}", fallbackResponse?.GalaxPayId);
                    return fallbackResponse ?? new CelPaySubscriptionResponseDto { Status = "SUCCESS", ErrorMessage = "Resposta inválida do gateway" };
                }
                else
                {
                    _logger.LogError("Erro ao criar subscription via CelPay. Status: {Status}, Response: {Response}", response.StatusCode, responseContent);
                    return new CelPaySubscriptionResponseDto 
                    { 
                        Status = "ERROR", 
                        ErrorMessage = $"Erro HTTP: {response.StatusCode}",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar subscription via CelPay");
                return new CelPaySubscriptionResponseDto 
                { 
                    Status = "ERROR", 
                    ErrorMessage = "Erro interno do sistema",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }
        
        public async Task<CelPaySubscriptionResponseDto> ConsultarSubscriptionAsync(string subscriptionId)
        {
            try
            {
                // Configurar autenticação antes de fazer a requisição
                await ConfigurarAutorizacaoAsync();

                var response = await _httpClient.GetAsync($"subscriptions/{subscriptionId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var subscriptionResponse = JsonSerializer.Deserialize<CelPaySubscriptionResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return subscriptionResponse ?? new CelPaySubscriptionResponseDto { Status = "ERROR", ErrorMessage = "Resposta inválida do gateway" };
                }
                else
                {
                    _logger.LogError("Erro ao consultar subscription via CelPay. Status: {Status}, Response: {Response}", response.StatusCode, responseContent);
                    return new CelPaySubscriptionResponseDto 
                    { 
                        Status = "ERROR", 
                        ErrorMessage = $"Erro HTTP: {response.StatusCode}",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao consultar subscription via CelPay");
                return new CelPaySubscriptionResponseDto 
                { 
                    Status = "ERROR", 
                    ErrorMessage = "Erro interno do sistema",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        /// <summary>
        /// Cancela uma subscription no CelPay
        /// </summary>
        public async Task<CelPaySubscriptionResponseDto> CancelarSubscriptionAsync(CelPayCancelSubscriptionDto cancelDto)
        {
            try
            {
                // Configurar autenticação antes de fazer a requisição
                await ConfigurarAutorizacaoAsync();

                var jsonContent = JsonSerializer.Serialize(cancelDto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Cancelando subscription via CelPay: {SubscriptionId}", cancelDto.SubscriptionId);

                var response = await _httpClient.PostAsync($"/subscriptions/{cancelDto.SubscriptionId}/cancel", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var subscriptionResponse = JsonSerializer.Deserialize<CelPaySubscriptionResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    
                    _logger.LogInformation("Subscription cancelada com sucesso via CelPay: {SubscriptionId}", cancelDto.SubscriptionId);
                    return subscriptionResponse ?? new CelPaySubscriptionResponseDto { Status = "CANCELLED", ErrorMessage = "Resposta inválida do gateway" };
                }
                else
                {
                    _logger.LogError("Erro ao cancelar subscription via CelPay. Status: {Status}, Response: {Response}", response.StatusCode, responseContent);
                    return new CelPaySubscriptionResponseDto 
                    { 
                        Status = "ERROR", 
                        ErrorMessage = $"Erro HTTP: {response.StatusCode}",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao cancelar subscription via CelPay");
                return new CelPaySubscriptionResponseDto 
                { 
                    Status = "ERROR", 
                    ErrorMessage = "Erro interno do sistema",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        /// <summary>
        /// Atualiza uma subscription no CelPay
        /// </summary>
        public async Task<CelPaySubscriptionResponseDto> AtualizarSubscriptionAsync(CelPayUpdateSubscriptionDto updateDto)
        {
            try
            {
                // Configurar autenticação antes de fazer a requisição
                await ConfigurarAutorizacaoAsync();

                var jsonContent = JsonSerializer.Serialize(updateDto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Atualizando subscription via CelPay: {SubscriptionId}", updateDto.SubscriptionId);

                var response = await _httpClient.PutAsync($"/subscriptions/{updateDto.SubscriptionId}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var subscriptionResponse = JsonSerializer.Deserialize<CelPaySubscriptionResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    
                    _logger.LogInformation("Subscription atualizada com sucesso via CelPay: {SubscriptionId}", updateDto.SubscriptionId);
                    return subscriptionResponse ?? new CelPaySubscriptionResponseDto { Status = "UPDATED", ErrorMessage = "Resposta inválida do gateway" };
                }
                else
                {
                    _logger.LogError("Erro ao atualizar subscription via CelPay. Status: {Status}, Response: {Response}", response.StatusCode, responseContent);
                    return new CelPaySubscriptionResponseDto 
                    { 
                        Status = "ERROR", 
                        ErrorMessage = $"Erro HTTP: {response.StatusCode}",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar subscription via CelPay");
                return new CelPaySubscriptionResponseDto 
                { 
                    Status = "ERROR", 
                    ErrorMessage = "Erro interno do sistema",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        /// <summary>
        /// Busca cliente por email no CelPay
        /// </summary>
        public async Task<CelPayCustomerResponseDto> BuscarClientePorEmailAsync(string email)
        {
            try
            {
                // Configurar autenticação antes de fazer a requisição
                await ConfigurarAutorizacaoAsync();

                _logger.LogInformation("Buscando cliente por email via CelPay: {Email}", email);

                // Construir query string para busca por email
                // Usar string interpolation para evitar codificação do @
                var queryString = $"customers?emails={email}&startAt=0&limit=100";
                
                _logger.LogInformation("URL da requisição: {Url}", $"{_baseUrl}{queryString}");
                
                var response = await _httpClient.GetAsync(queryString);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var customerResponse = JsonSerializer.Deserialize<CelPayCustomerResponseDto>(responseContent);
                    
                    _logger.LogInformation("Busca de cliente realizada com sucesso via CelPay. Clientes encontrados: {Count}", 
                        customerResponse?.Customers?.Count ?? 0);
                    
                    return customerResponse ?? new CelPayCustomerResponseDto { Type = false, ErrorMessage = "Resposta inválida do gateway" };
                }
                else
                {
                    _logger.LogError("Erro ao buscar cliente via CelPay. Status: {Status}, Response: {Response}", response.StatusCode, responseContent);
                    return new CelPayCustomerResponseDto 
                    { 
                        Type = false,
                        ErrorMessage = $"Erro HTTP: {response.StatusCode}",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar cliente via CelPay");
                return new CelPayCustomerResponseDto 
                { 
                    Type = false,
                    // ErrorMessage = "Erro interno do sistema",
                    // ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        /// <summary>
        /// Cria um novo cliente no CelPay
        /// </summary>
        public async Task<CelPayCustomerResponseDto> CriarClienteAsync(CelPayCustomerRequestDto customerDto)
        {
            try
            {
                // Configurar autenticação antes de fazer a requisição
                await ConfigurarAutorizacaoAsync();

                var jsonContent = JsonSerializer.Serialize(customerDto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Criando cliente via CelPay: {Name} - {Email}", customerDto.Name, string.Join(", ", customerDto.Emails));

                var response = await _httpClient.PostAsync("/customers", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var customerResponse = JsonSerializer.Deserialize<CelPayCustomerResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    
                    _logger.LogInformation("Cliente criado com sucesso via CelPay. ClienteId: {CustomerId}", 
                        customerResponse?.FirstCustomer?.GalaxPayId);
                    
                    return customerResponse ?? new CelPayCustomerResponseDto { Type = false, ErrorMessage = "Resposta inválida do gateway" };
                }
                else
                {
                    _logger.LogError("Erro ao criar cliente via CelPay. Status: {Status}, Response: {Response}", response.StatusCode, responseContent);
                    return new CelPayCustomerResponseDto 
                    { 
                        Type = false,
                        ErrorMessage = $"Erro HTTP: {response.StatusCode}",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar cliente via CelPay");
                return new CelPayCustomerResponseDto 
                { 
                    Type = false,
                    ErrorMessage = "Erro interno do sistema",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }
    }
}
