using Microsoft.AspNetCore.Identity;
using Projeto.Moope.Core.Commands.Base;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Notifications;

namespace Projeto.Moope.Core.Commands.Clientes.AlterarSenha
{
    public class AlterarSenhaAdminCommandHandler : ICommandHandler<AlterarSenhaAdminCommand, Result>
    {
        private readonly UserManager<IdentityUser<Guid>> _userManager;
        private readonly IClienteRepository _clienteRepository;
        private readonly INotificador _notificador;

        public AlterarSenhaAdminCommandHandler(
            UserManager<IdentityUser<Guid>> userManager,
            IClienteRepository clienteRepository,
            INotificador notificador)
        {
            _userManager = userManager;
            _clienteRepository = clienteRepository;
            _notificador = notificador;
        }

        public async Task<Result> Handle(AlterarSenhaAdminCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar se o cliente existe
                var cliente = await _clienteRepository.BuscarPorIdAsync(request.ClienteId);
                if (cliente == null)
                {
                    //_notificador.AdicionarNotificacao("Cliente", "Cliente não encontrado");
                    _notificador.Handle(new Notificacao
                    {
                        Campo = "Mensagem",
                        Mensagem = "Cliente não encontrado"
                    });
                    return new Result
                    {
                        Status = false,
                        Mensagem = "Cliente não encontrado"
                    };
                }

                // Buscar o usuário no Identity
                var usuario = await _userManager.FindByIdAsync(request.ClienteId.ToString());
                if (usuario == null)
                {
                    //_notificador.AdicionarNotificacao("Usuário", "Usuário não encontrado no sistema de autenticação");
                    _notificador.Handle(new Notificacao
                    {
                        Campo = "Mensagem",
                        Mensagem = "Usuário não encontrado no sistema de autenticação"
                    });
                    return new Result
                    {
                        Status = false,
                        Mensagem = "Usuário não encontrado no sistema de autenticação"
                    };
                }

                // Gerar token para reset de senha (método mais seguro para administradores)
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                
                // Resetar a senha usando o token
                var resultado = await _userManager.ResetPasswordAsync(usuario, token, request.NovaSenha);

                if (!resultado.Succeeded)
                {
                    foreach (var error in resultado.Errors)
                    {
                        //_notificador.AdicionarNotificacao("Senha", error.Description);
                        _notificador.Handle(new Notificacao
                        {
                            Campo = "Senha",
                            Mensagem = error.Description
                        });
                    }
                    
                    return new Result
                    {
                        Status = false,
                        Mensagem = "Falha ao alterar senha"
                    };
                }

                return new Result
                {
                    Status = true,
                    Mensagem = "Senha alterada com sucesso"
                };
            }
            catch (Exception ex)
            {
                //_notificador.AdicionarNotificacao("Sistema", $"Erro interno: {ex.Message}");
                _notificador.Handle(new Notificacao
                {
                    Campo = "Senha",
                    Mensagem = $"Erro interno: {ex.Message}"
                });
                return new Result
                {
                    Status = false,
                    Mensagem = "Erro interno do sistema"
                };
            }
        }
    }
}
