using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Projeto.Moope.Core.DTOs.Pagamentos
{
    /// <summary>
    /// DTO para criação de cliente no CelPay
    /// </summary>
    public class CelPayCustomerRequestDto
    {
        /// <summary>
        /// ID externo do cliente (opcional)
        /// </summary>
        [JsonPropertyName("myId")]
        public string? MyId { get; set; }

        /// <summary>
        /// Nome do cliente
        /// </summary>
        [Required(ErrorMessage = "Nome do cliente é obrigatório")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Documento do cliente (CPF/CNPJ)
        /// </summary>
        [JsonPropertyName("document")]
        public string? Document { get; set; }

        /// <summary>
        /// Emails do cliente
        /// </summary>
        [JsonPropertyName("emails")]
        public List<string> Emails { get; set; } = new List<string>();

        /// <summary>
        /// Telefones do cliente
        /// </summary>
        [JsonPropertyName("phones")]
        public List<string> Phones { get; set; } = new List<string>();

        /// <summary>
        /// Endereço do cliente
        /// </summary>
        [JsonPropertyName("address")]
        public CelPayAddressDto? Address { get; set; }
    }
}
