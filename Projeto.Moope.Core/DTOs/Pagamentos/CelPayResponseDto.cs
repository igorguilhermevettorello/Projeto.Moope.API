namespace Projeto.Moope.Core.DTOs.Pagamentos
{
    public class CelPayResponseDto
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string ExternalId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public CardResponse Card { get; set; }
        public CustomerResponse Customer { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
    }

    public class CardResponse
    {
        public string Last4 { get; set; }
        public string Brand { get; set; }
        public string HolderName { get; set; }
    }

    public class CustomerResponse
    {
        public string Name { get; set; }
        public string Emails { get; set; }
        public string Phone { get; set; }
    }
}
