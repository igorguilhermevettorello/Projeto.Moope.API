using Projeto.Moope.Core.Enums;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Projeto.Moope.Core.DTOs.Pagamentos
{
    /// <summary>
    /// Conversor personalizado para serializar enums como strings em minúsculo
    /// </summary>
    public class LowercaseEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string stringValue = reader.GetString() ?? string.Empty;
                if (Enum.TryParse<T>(stringValue, true, out T result))
                {
                    return result;
                }
            }
            
            throw new JsonException($"Não foi possível converter '{reader.GetString()}' para {typeof(T).Name}");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToLowerInvariant());
        }
    }

    /// <summary>
    /// DTO para criação de subscription com plano no CelPay
    /// </summary>
    public class CelPaySubscriptionRequestDto
    {
        public string ExternalId { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        [JsonConverter(typeof(LowercaseEnumConverter<Periodicidade>))]
        public Periodicidade Periodicity { get; set; }
        [JsonConverter(typeof(LowercaseEnumConverter<MetodoPagamento>))]
        public MetodoPagamento MainPaymentMethodId { get; set; }
        public int Quantity { get; set; }
        public int Value { get; set; }
        public string FirstPayDayDate { get; set; }
        public CardInfo Card { get; set; } = new();
        [JsonPropertyName("Customer")]
        public CustomerInfo Customer { get; set; } = new();
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public string? PromoCode { get; set; }
        public SubscriptionMetadata? Metadata { get; set; }
    }

    /// <summary>
    /// DTO para resposta de subscription do CelPay
    /// </summary>
    public class CelPaySubscriptionResponseDto
    {
        public string GalaxPayId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public CustomerResponse Customer { get; set; } = new();
        public CardResponse Card { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? NextChargeDate { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? Description { get; set; }
        public SubscriptionPlanInfo? Plan { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public List<TransactionDto> Transactions { get; set; } = new();
    }

    /// <summary>
    /// DTO para resposta de criação de subscription com sucesso no CelPay
    /// </summary>
    public class CelPaySubscriptionSuccessResponseDto
    {
        public bool Type { get; set; }
        public SubscriptionDetailDto Subscription { get; set; } = new();
    }

    /// <summary>
    /// DTO detalhado da subscription criada
    /// </summary>
    public class SubscriptionDetailDto
    {
        public string? MyId { get; set; }
        public int GalaxPayId { get; set; }
        public int Value { get; set; }
        public string PaymentLink { get; set; } = string.Empty;
        public string MainPaymentMethodId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdditionalInfo { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Periodicity { get; set; } = string.Empty;
        public string FirstPayDayDate { get; set; } = string.Empty;
        public CustomerDetailDto Customer { get; set; } = new();
        public List<TransactionDto> Transactions { get; set; } = new();
        public List<object> ExtraFields { get; set; } = new();
        public PaymentMethodCreditCardDto PaymentMethodCreditCard { get; set; } = new();
    }

    /// <summary>
    /// DTO detalhado do cliente na subscription
    /// </summary>
    public class CustomerDetailDto
    {
        public string? MyId { get; set; }
        public int GalaxPayId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Document { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public List<string> Emails { get; set; } = new();
        public List<string> Phones { get; set; } = new();
        public AddressDto Address { get; set; } = new();
        public List<object> ExtraFields { get; set; } = new();
    }

    /// <summary>
    /// DTO do endereço do cliente
    /// </summary>
    public class AddressDto
    {
        public string? ZipCode { get; set; }
        public string? Street { get; set; }
        public string? Number { get; set; }
        public string? Complement { get; set; }
        public string? Neighborhood { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }

    /// <summary>
    /// DTO da transação da subscription
    /// </summary>
    public class TransactionDto
    {
        public int GalaxPayId { get; set; }
        public int Value { get; set; }
        public string Payday { get; set; } = string.Empty;
        public string? PaydayDate { get; set; }
        public int Installment { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
        public string StatusDate { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string? MyId { get; set; }
        public string? AdditionalInfo { get; set; }
        public string? DatetimeLastSentToOperator { get; set; }
        public string? SubscriptionMyId { get; set; }
        public int SubscriptionGalaxPayId { get; set; }
        public bool PayedOutsideGalaxPay { get; set; }
        public List<object> ConciliationOccurrences { get; set; } = new();
        public BoletoDto Boleto { get; set; } = new();
    }

    /// <summary>
    /// DTO do boleto da transação
    /// </summary>
    public class BoletoDto
    {
        public string Pdf { get; set; } = string.Empty;
        public string? BankLine { get; set; }
        public string? BankNumber { get; set; }
        public string? BarCode { get; set; }
        public string? BankEmissor { get; set; }
        public string? BankAgency { get; set; }
        public string? BankAccount { get; set; }
    }

    /// <summary>
    /// DTO do método de pagamento com cartão de crédito
    /// </summary>
    public class PaymentMethodCreditCardDto
    {
        public string? CardOperatorId { get; set; }
        public bool PreAuthorize { get; set; }
    }

    /// <summary>
    /// Informações do plano na subscription
    /// </summary>
    public class SubscriptionPlanInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BRL";
        public string Interval { get; set; } = string.Empty; // monthly, yearly, etc.
        public int IntervalCount { get; set; }
        public int? TrialPeriodDays { get; set; }
    }

    /// <summary>
    /// Metadata adicional para subscription
    /// </summary>
    public class SubscriptionMetadata
    {
        public string? ClienteId { get; set; }
        public string? VendedorId { get; set; }
        public string? Observacoes { get; set; }
        public Dictionary<string, string>? CustomFields { get; set; }
    }

    /// <summary>
    /// DTO para cancelamento de subscription
    /// </summary>
    public class CelPayCancelSubscriptionDto
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool CancelAtPeriodEnd { get; set; } = false;
    }

    /// <summary>
    /// DTO para alteração de subscription
    /// </summary>
    public class CelPayUpdateSubscriptionDto
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public string? NewPlanId { get; set; }
        public CardInfo? NewCard { get; set; }
        public SubscriptionMetadata? Metadata { get; set; }
    }
}
