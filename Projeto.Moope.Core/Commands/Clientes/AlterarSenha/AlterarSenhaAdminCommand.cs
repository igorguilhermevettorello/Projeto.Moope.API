using Projeto.Moope.Core.Commands.Base;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Commands.Clientes.AlterarSenha
{
    public class AlterarSenhaAdminCommand : ICommand<Result>
    {
        public Guid ClienteId { get; set; }
        public string NovaSenha { get; set; }

        public AlterarSenhaAdminCommand(Guid clienteId, string novaSenha)
        {
            ClienteId = clienteId;
            NovaSenha = novaSenha;
        }
    }
}
