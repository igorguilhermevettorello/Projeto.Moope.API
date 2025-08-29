using System.ComponentModel;

namespace Projeto.Moope.Core.Enums
{
    public enum StatusPagamento
    {
        [Description("Ainda não enviada para operadora de Cartão")]
        NotSend = 1,

        [Description("Autorizado")]
        Authorized = 2,

        [Description("Capturada na Operadora de Cartão")]
        Captured = 3,

        [Description("Negada na Operadora de Cartão")]
        Denied = 4,

        [Description("Estornada na Operadora de Cartão")]
        Reversed = 5,

        [Description("Boleto em aberto")]
        PendingBoleto = 6,

        [Description("Estorno por Chargeback")]
        Chargeback = 7,

        [Description("Boleto compensado")]
        PayedBoleto = 8,

        [Description("Boleto baixado por decurso de prazo")]
        NotCompensated = 9,

        [Description("Pix em aberto")]
        PendingPix = 10,

        [Description("Pix pago")]
        PayedPix = 11,

        [Description("Pix indisponível para pagamento")]
        UnavailablePix = 12,

        [Description("Cancelada manualmente")]
        Cancel = 13,

        [Description("Paga fora do sistema")]
        PayExternal = 14,

        [Description("Cancelada ao cancelar a cobrança")]
        CancelByContract = 15,

        [Description("Isento")]
        Free = 16
    }
}

