using System.ComponentModel;

namespace Projeto.Moope.Core.Enums
{
    public enum StatusAssinatura
    {
        [Description("Ativa")]
        Active = 1,

        [Description("Cancelada")]
        Canceled = 2,

        [Description("Encerrada")]
        Closed = 3,

        [Description("Interrompida")]
        Stopped = 4,

        [Description("Aguardando pagamento")]
        WaitingPayment = 5,

        [Description("Inativa")]
        Inactive = 6
    }
}

