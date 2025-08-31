using System.Text.Json.Serialization;

namespace Projeto.Moope.Core.DTOs.Pagamentos
{
    /// <summary>
    /// DTO para resposta de busca de cliente no CelPay
    /// </summary>
    public class CelPayCustomerResponseDto
    {
        /// <summary>
        /// Quantidade total de clientes encontrados na página
        /// </summary>
        [JsonPropertyName("totalQtdFoundInPage")]
        public int TotalQtdFoundInPage { get; set; }

        /// <summary>
        /// Lista de clientes encontrados
        /// </summary>
        [JsonPropertyName("Customers")]
        public List<CelPayCustomerDto> Customers { get; set; } = new List<CelPayCustomerDto>();

        /// <summary>
        /// Indica se há clientes encontrados
        /// </summary>
        public bool HasCustomers => Customers?.Any() == true;

        /// <summary>
        /// Retorna o primeiro cliente encontrado
        /// </summary>
        public CelPayCustomerDto? FirstCustomer => Customers?.FirstOrDefault();

        /// <summary>
        /// Indica se a operação foi bem-sucedida (baseado na presença de clientes)
        /// </summary>
        public bool Type => HasCustomers;
        public string ErrorMessage  { get; set; }  
        public string ErrorCode   { get; set; }
    }

    /// <summary>
    /// DTO para dados do cliente no CelPay
    /// </summary>
    public class CelPayCustomerDto
    {
        /// <summary>
        /// ID externo do cliente
        /// </summary>
        [JsonPropertyName("myId")]
        public string? MyId { get; set; }

        /// <summary>
        /// ID do cliente no CelPay
        /// </summary>
        [JsonPropertyName("galaxPayId")]
        public int GalaxPayId { get; set; }

        /// <summary>
        /// Nome do cliente
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Documento do cliente (CPF/CNPJ)
        /// </summary>
        [JsonPropertyName("document")]
        public string? Document { get; set; }

        /// <summary>
        /// Data de criação
        /// </summary>
        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        /// <summary>
        /// Data de atualização
        /// </summary>
        [JsonPropertyName("updatedAt")]
        public string UpdatedAt { get; set; } = string.Empty;

        /// <summary>
        /// Status do cliente
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Emails do cliente
        /// </summary>
        [JsonPropertyName("emails")]
        public List<string> Emails { get; set; } = new List<string>();

        /// <summary>
        /// Telefones do cliente (números como inteiros)
        /// </summary>
        [JsonPropertyName("phones")]
        public List<long> Phones { get; set; } = new List<long>();

        /// <summary>
        /// Endereço do cliente
        /// </summary>
        [JsonPropertyName("Address")]
        public CelPayAddressDto? Address { get; set; }

        /// <summary>
        /// Campos extras do cliente
        /// </summary>
        [JsonPropertyName("ExtraFields")]
        public List<CelPayExtraFieldDto> ExtraFields { get; set; } = new List<CelPayExtraFieldDto>();

        /// <summary>
        /// Verifica se o cliente tem o email especificado
        /// </summary>
        public bool HasEmail(string email)
        {
            return Emails?.Any(e => string.Equals(e, email, StringComparison.OrdinalIgnoreCase)) == true;
        }
    }

    /// <summary>
    /// DTO para endereço do cliente no CelPay
    /// </summary>
    public class CelPayAddressDto
    {
        /// <summary>
        /// CEP
        /// </summary>
        [JsonPropertyName("zipCode")]
        public string? ZipCode { get; set; }

        /// <summary>
        /// Logradouro
        /// </summary>
        [JsonPropertyName("street")]
        public string? Street { get; set; }

        /// <summary>
        /// Número
        /// </summary>
        [JsonPropertyName("number")]
        public string? Number { get; set; }

        /// <summary>
        /// Complemento
        /// </summary>
        [JsonPropertyName("complement")]
        public string? Complement { get; set; }

        /// <summary>
        /// Bairro
        /// </summary>
        [JsonPropertyName("neighborhood")]
        public string? Neighborhood { get; set; }

        /// <summary>
        /// Cidade
        /// </summary>
        [JsonPropertyName("city")]
        public string? City { get; set; }

        /// <summary>
        /// Estado
        /// </summary>
        [JsonPropertyName("state")]
        public string? State { get; set; }
    }

    /// <summary>
    /// DTO para campos extras do cliente no CelPay
    /// </summary>
    public class CelPayExtraFieldDto
    {
        /// <summary>
        /// Nome da tag
        /// </summary>
        [JsonPropertyName("tagName")]
        public string TagName { get; set; } = string.Empty;

        /// <summary>
        /// Valor da tag
        /// </summary>
        [JsonPropertyName("tagValue")]
        public string TagValue { get; set; } = string.Empty;
    }
}
