using Projeto.Moope.Core.Commands.Base;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Commands.Clientes.AlterarSenha
{
    public class AlterarSenhaClienteCommand : ICommand<Result>
    {
        public Guid ClienteId { get; set; }
        public string SenhaAtual { get; set; }
        public string NovaSenha { get; set; }

        public AlterarSenhaClienteCommand(Guid clienteId, string senhaAtual, string novaSenha)
        {
            ClienteId = clienteId;
            SenhaAtual = senhaAtual;
            NovaSenha = novaSenha;
        }
    }
}
