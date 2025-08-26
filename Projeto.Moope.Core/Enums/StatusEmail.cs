namespace Projeto.Moope.Core.Enums
{
    /// <summary>
    /// Status possíveis para um email
    /// </summary>
    public enum StatusEmail
    {
        /// <summary>
        /// Email criado e aguardando envio
        /// </summary>
        Pendente = 1,

        /// <summary>
        /// Email sendo processado/enviado
        /// </summary>
        Processando = 2,

        /// <summary>
        /// Email enviado com sucesso
        /// </summary>
        Enviado = 3,

        /// <summary>
        /// Falha no envio do email
        /// </summary>
        Falha = 4,

        /// <summary>
        /// Email cancelado (não será enviado)
        /// </summary>
        Cancelado = 5,

        /// <summary>
        /// Email agendado para envio futuro
        /// </summary>
        Agendado = 6,

        /// <summary>
        /// Email rejeitado pelo servidor de destino
        /// </summary>
        Rejeitado = 7
    }
}
