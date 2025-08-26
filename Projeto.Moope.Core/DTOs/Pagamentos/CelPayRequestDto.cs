namespace Projeto.Moope.Core.DTOs.Pagamentos
{
    public enum Periodicity
    {
        Monthly,
        Quarterly,
        SemiAnnual,
        Annual
    }

    public enum MainPaymentMethodId
    {
        CreditCard,
        DebitCard,
        PIX,
        BankTransfer,
        Boleto
    }

    public class CelPayRequestDto
    {
        public string ExternalId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BRL";
        public CardInfo Card { get; set; }
        public CustomerInfo Customer { get; set; }
        public string Description { get; set; }
        public string Capture { get; set; } = "true";
        public string Installments { get; set; } = "1";
    }

    public class CardInfo
    {
        public string Number { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public string Cvv { get; set; }
        public string HolderName { get; set; }
    }

    public class CustomerInfo
    {
        public string Name { get; set; }
        public string[] Emails { get; set; }
    }
}
