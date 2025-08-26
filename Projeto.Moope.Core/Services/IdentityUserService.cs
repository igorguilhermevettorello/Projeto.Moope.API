using Microsoft.AspNetCore.Identity;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Services.Base;

namespace Projeto.Moope.Core.Services
{
    public class IdentityUserService : BaseService, IIdentityUserService
    {
        private readonly UserManager<IdentityUser<Guid>> _userManager;
        private readonly IClienteRepository _clienteRepository;
        private readonly IVendedorRepository _vendedorRepository;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly INotificador _notificador;

        public IdentityUserService(
            UserManager<IdentityUser<Guid>> userManager,
            IClienteRepository clienteRepository,
            IVendedorRepository vendedorRepository,
            RoleManager<IdentityRole<Guid>> roleManager,
            INotificador notificador) : base(notificador)
        {
            _userManager = userManager;
            _clienteRepository = clienteRepository;
            _vendedorRepository = vendedorRepository;
            _roleManager = roleManager;
            _notificador = notificador;
        }

        public async Task<ResultUser<IdentityUser<Guid>>> CriarUsuarioAsync(string email, string senha, string telefone = null, TipoUsuario tipoUsuario = TipoUsuario.Cliente)
        {
            try
            {
                var usuario = new IdentityUser<Guid>
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = telefone,
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0
                };

                var usuarioExiste = await _userManager.FindByEmailAsync(email);
                if (usuarioExiste != null)
                {
                    var clienteExistente = await _clienteRepository.BuscarPorIdAsync(usuarioExiste.Id);
                    var vendedorExistente = await  _vendedorRepository.BuscarPorIdAsync(usuarioExiste.Id);
                    if (tipoUsuario == TipoUsuario.Vendedor && vendedorExistente != null)
                    {
                        Notificar("Email", $"O usuário '{email}' já está em uso.");
                        return new ResultUser<IdentityUser<Guid>>()
                        {
                            Status = false,
                            Mensagem = "Falha ao criar usuário. Corriga o erro e envie novamente."
                        };    
                    }
                    
                    if (tipoUsuario == TipoUsuario.Cliente && clienteExistente != null)
                    {
                        Notificar("Email", $"O usuário '{email}' já está em uso.");
                        return new ResultUser<IdentityUser<Guid>>()
                        {
                            Status = false,
                            Mensagem = "Falha ao criar usuário. Corriga o erro e envie novamente."
                        };
                    }

                    return new ResultUser<IdentityUser<Guid>>()
                    {
                        Status = true,
                        Dados = usuarioExiste,
                        UsuarioExiste = true
                    };
                    // var clienteExistente = await _clienteRepository.BuscarPorIdAsync(usuarioExiste.Id);
                    // var vendedorExistente = await  _vendedorRepository.BuscarPorIdAsync(usuarioExiste.Id);
                    // if (TipoUsuario.Vendedor == tipoUsuario && clienteExistente != null)
                    // {
                    //     return new ResultUser<IdentityUser<Guid>>()
                    //     {
                    //         Status = true,
                    //         Dados = usuarioExiste
                    //     };     
                    // }

                    //"mensagem": "O usuário 'cliente-vendedor-cnpj@email.com' já está em uso."
                    // if (TipoUsuario.Vendedor == tipoUsuario)
                    // {
                    //     var clienteExistente = await _clienteRepository.BuscarPorIdAsync(usuarioExiste.Id);
                    //     var vendedorExistente = await  _vendedorRepository.BuscarPorIdAsync(usuarioExiste.Id);
                    // }    
                }

                

                var resultado = await _userManager.CreateAsync(usuario, senha);

                if (resultado.Succeeded)
                {
                    await _userManager.AddToRoleAsync(usuario, tipoUsuario.ToString());
                }
                else
                {
                    foreach (var error in resultado.Errors)
                    {
                        if (error.Code.Equals("PasswordRequiresUpper"))
                        {
                            Notificar("Senha", error.Description);
                        }
                        else if (error.Code.Equals("PasswordRequiresLower"))
                        {
                            Notificar("Senha", error.Description);
                        }
                        else if (error.Code.Equals("PasswordRequiresDigit"))
                        {
                            Notificar("Senha", error.Description);
                        }
                        else if (error.Code.Equals("PasswordRequiresNonAlphanumeric"))
                        {
                            Notificar("Senha", error.Description);
                        }
                        else if (error.Code.Equals("DuplicateUserName"))
                        {
                            Notificar("Email", error.Description);
                        }
                        else
                        {
                            Notificar("Senha", error.Description);
                        }
                    }
                    
                    return new ResultUser<IdentityUser<Guid>>()
                    {
                        Status = false,
                        Mensagem = "Falha ao criar usuário. Corriga o erro e envie novamente."
                    };
                }

                return new ResultUser<IdentityUser<Guid>>()
                {
                    Status = true,
                    Dados = usuario
                };
            }
            catch (Exception ex)
            {
                return new ResultUser<IdentityUser<Guid>>()
                {
                    Status = false,
                    Mensagem = $"Falha ao criar usuário: {ex.Message}"
                };
            }
        }

        public async Task RemoverAoFalharAsync(IdentityUser<Guid> usuario)
        {
            await _userManager.DeleteAsync(usuario);
        }

        public async Task<Result<IdentityUser<Guid>>> AlterarUsuarioAsync(Guid userId, string email, string telefone = null)
        {
            try
            {
                var usuario = await _userManager.FindByIdAsync(userId.ToString());
                if (usuario == null)
                {
                    Notificar("Email", "Usuário não encontrado");
                    throw new Exception("Usuário não encontrado");
                }
                
                usuario.Email = email;
                usuario.UserName = email;
                usuario.PhoneNumber = telefone;

                var resultado = await _userManager.UpdateAsync(usuario);
                if (!resultado.Succeeded)
                {
                    foreach (var error in resultado.Errors)
                    {
                        if (error.Code.Equals("PasswordRequiresUpper"))
                        {
                            Notificar("Senha", error.Description);
                        }
                        else if (error.Code.Equals("PasswordRequiresLower"))
                        {
                            Notificar("Senha", error.Description);
                        }
                        else if (error.Code.Equals("PasswordRequiresDigit"))
                        {
                            Notificar("Senha", error.Description);
                        }
                        else if (error.Code.Equals("PasswordRequiresNonAlphanumeric"))
                        {
                            Notificar("Senha", error.Description);
                        }
                        else if (error.Code.Equals("DuplicateUserName"))
                        {
                            Notificar("Email", error.Description);
                        }
                        else
                        {
                            Notificar("Senha", error.Description);
                        }
                    }
                    
                    return new Result<IdentityUser<Guid>>()
                    {
                        Status = false,
                        Mensagem = "Falha ao criar usuário. Corriga o erro e envie novamente."
                    };
                }

                return new Result<IdentityUser<Guid>>()
                {
                    Status = true,
                    Dados = usuario
                };
            }
            catch (Exception ex)
            {
                return new Result<IdentityUser<Guid>>()
                {
                    Status = false,
                    Mensagem = $"Falha ao alterar usuário: {ex.Message}"
                };
            }
        }


        // public async Task<IdentityResult> AlterarSenhaAsync(string userId, string senhaAtual, string novaSenha)
        // {
        //     try
        //     {
        //         var usuario = await _userManager.FindByIdAsync(userId);
        //         if (usuario == null)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário não encontrado");
        //             return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado" });
        //         }
        //
        //         var resultado = await _userManager.ChangePasswordAsync(usuario, senhaAtual, novaSenha);
        //
        //         if (resultado.Succeeded)
        //         {
        //             _notificador.AdicionarNotificacao("Senha alterada com sucesso");
        //         }
        //         else
        //         {
        //             foreach (var erro in resultado.Errors)
        //             {
        //                 _notificador.AdicionarNotificacao(erro.Description);
        //             }
        //         }
        //
        //         return resultado;
        //     }
        //     catch (Exception ex)
        //     {
        //         _notificador.AdicionarNotificacao($"Erro ao alterar senha: {ex.Message}");
        //         return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        //     }
        // }
        //
        // public async Task<IdentityResult> ResetarSenhaAsync(string email, string token, string novaSenha)
        // {
        //     try
        //     {
        //         var usuario = await _userManager.FindByEmailAsync(email);
        //         if (usuario == null)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário não encontrado");
        //             return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado" });
        //         }
        //
        //         var resultado = await _userManager.ResetPasswordAsync(usuario, token, novaSenha);
        //
        //         if (resultado.Succeeded)
        //         {
        //             _notificador.AdicionarNotificacao("Senha resetada com sucesso");
        //         }
        //         else
        //         {
        //             foreach (var erro in resultado.Errors)
        //             {
        //                 _notificador.AdicionarNotificacao(erro.Description);
        //             }
        //         }
        //
        //         return resultado;
        //     }
        //     catch (Exception ex)
        //     {
        //         _notificador.AdicionarNotificacao($"Erro ao resetar senha: {ex.Message}");
        //         return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        //     }
        // }
        //
        // public async Task<string> GerarTokenResetSenhaAsync(string email)
        // {
        //     try
        //     {
        //         var usuario = await _userManager.FindByEmailAsync(email);
        //         if (usuario == null)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário não encontrado");
        //             return null;
        //         }
        //
        //         var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        //         return token;
        //     }
        //     catch (Exception ex)
        //     {
        //         _notificador.AdicionarNotificacao($"Erro ao gerar token de reset: {ex.Message}");
        //         return null;
        //     }
        // }
        //
        // public async Task<bool> ConfirmarEmailAsync(string userId, string token)
        // {
        //     try
        //     {
        //         var usuario = await _userManager.FindByIdAsync(userId);
        //         if (usuario == null)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário não encontrado");
        //             return false;
        //         }
        //
        //         var resultado = await _userManager.ConfirmEmailAsync(usuario, token);
        //
        //         if (resultado.Succeeded)
        //         {
        //             _notificador.AdicionarNotificacao("Email confirmado com sucesso");
        //             return true;
        //         }
        //         else
        //         {
        //             foreach (var erro in resultado.Errors)
        //             {
        //                 _notificador.AdicionarNotificacao(erro.Description);
        //             }
        //             return false;
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _notificador.AdicionarNotificacao($"Erro ao confirmar email: {ex.Message}");
        //         return false;
        //     }
        // }
        //
        // public async Task<string> GerarTokenConfirmacaoEmailAsync(string userId)
        // {
        //     try
        //     {
        //         var usuario = await _userManager.FindByIdAsync(userId);
        //         if (usuario == null)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário não encontrado");
        //             return null;
        //         }
        //
        //         var token = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);
        //         return token;
        //     }
        //     catch (Exception ex)
        //     {
        //         _notificador.AdicionarNotificacao($"Erro ao gerar token de confirmação: {ex.Message}");
        //         return null;
        //     }
        // }
        //
        public async Task<IdentityUser<Guid>> BuscarPorEmailAsync(string email)
        {
            try
            {
                return await _userManager.FindByEmailAsync(email);
            }
            catch (Exception ex)
            {
                Notificar("Email", $"Erro ao buscar usuário por email: {ex.Message}");
                return null;
            }
        }
        //
        // public async Task<IdentityUser> BuscarPorIdAsync(string id)
        // {
        //     try
        //     {
        //         return await _userManager.FindByIdAsync(id);
        //     }
        //     catch (Exception ex)
        //     {
        //         _notificador.AdicionarNotificacao($"Erro ao buscar usuário por ID: {ex.Message}");
        //         return null;
        //     }
        // }
        //
        // public async Task<bool> VerificarSenhaAsync(string userId, string senha)
        // {
        //     try
        //     {
        //         var usuario = await _userManager.FindByIdAsync(userId);
        //         if (usuario == null)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário não encontrado");
        //             return false;
        //         }
        //
        //         return await _userManager.CheckPasswordAsync(usuario, senha);
        //     }
        //     catch (Exception ex)
        //     {
        //         _notificador.AdicionarNotificacao($"Erro ao verificar senha: {ex.Message}");
        //         return false;
        //     }
        // }
        //
        // public async Task<IdentityResult> BloquearUsuarioAsync(string userId)
        // {
        //     try
        //     {
        //         var usuario = await _userManager.FindByIdAsync(userId);
        //         if (usuario == null)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário não encontrado");
        //             return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado" });
        //         }
        //
        //         var resultado = await _userManager.SetLockoutEndDateAsync(usuario, DateTimeOffset.MaxValue);
        //
        //         if (resultado.Succeeded)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário bloqueado com sucesso");
        //         }
        //         else
        //         {
        //             foreach (var erro in resultado.Errors)
        //             {
        //                 _notificador.AdicionarNotificacao(erro.Description);
        //             }
        //         }
        //
        //         return resultado;
        //     }
        //     catch (Exception ex)
        //     {
        //         _notificador.AdicionarNotificacao($"Erro ao bloquear usuário: {ex.Message}");
        //         return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        //     }
        // }
        //
        // public async Task<IdentityResult> DesbloquearUsuarioAsync(string userId)
        // {
        //     try
        //     {
        //         var usuario = await _userManager.FindByIdAsync(userId);
        //         if (usuario == null)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário não encontrado");
        //             return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado" });
        //         }
        //
        //         var resultado = await _userManager.SetLockoutEndDateAsync(usuario, null);
        //
        //         if (resultado.Succeeded)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário desbloqueado com sucesso");
        //         }
        //         else
        //         {
        //             foreach (var erro in resultado.Errors)
        //             {
        //                 _notificador.AdicionarNotificacao(erro.Description);
        //             }
        //         }
        //
        //         return resultado;
        //     }
        //     catch (Exception ex)
        //     {
        //         _notificador.AdicionarNotificacao($"Erro ao desbloquear usuário: {ex.Message}");
        //         return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        //     }
        // }
        //
        // public async Task<IdentityResult> AtivarUsuarioAsync(string userId)
        // {
        //     try
        //     {
        //         var usuario = await _userManager.FindByIdAsync(userId);
        //         if (usuario == null)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário não encontrado");
        //             return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado" });
        //         }
        //
        //         var resultado = await _userManager.SetLockoutEnabledAsync(usuario, false);
        //
        //         if (resultado.Succeeded)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário ativado com sucesso");
        //         }
        //         else
        //         {
        //             foreach (var erro in resultado.Errors)
        //             {
        //                 _notificador.AdicionarNotificacao(erro.Description);
        //             }
        //         }
        //
        //         return resultado;
        //     }
        //     catch (Exception ex)
        //     {
        //         _notificador.AdicionarNotificacao($"Erro ao ativar usuário: {ex.Message}");
        //         return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        //     }
        // }
        //
        // public async Task<IdentityResult> DesativarUsuarioAsync(string userId)
        // {
        //     try
        //     {
        //         var usuario = await _userManager.FindByIdAsync(userId);
        //         if (usuario == null)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário não encontrado");
        //             return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado" });
        //         }
        //
        //         var resultado = await _userManager.SetLockoutEnabledAsync(usuario, true);
        //
        //         if (resultado.Succeeded)
        //         {
        //             _notificador.AdicionarNotificacao("Usuário desativado com sucesso");
        //         }
        //         else
        //         {
        //             foreach (var erro in resultado.Errors)
        //             {
        //                 _notificador.AdicionarNotificacao(erro.Description);
        //             }
        //         }
        //
        //         return resultado;
        //     }
        //     catch (Exception ex)
        //     {
        //         _notificador.AdicionarNotificacao($"Erro ao desativar usuário: {ex.Message}");
        //         return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        //     }
        // }
    }
}
