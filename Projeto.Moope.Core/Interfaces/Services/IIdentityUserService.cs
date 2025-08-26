using Microsoft.AspNetCore.Identity;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Interfaces.Services
{
    public interface IIdentityUserService
    {
        Task<ResultUser<IdentityUser<Guid>>> CriarUsuarioAsync(
            string email, 
            string senha,
            string telefone = null, 
            TipoUsuario tipoUsuario = TipoUsuario.Cliente);
        
        Task RemoverAoFalharAsync(IdentityUser<Guid> usuario);
        Task<Result<IdentityUser<Guid>>> AlterarUsuarioAsync(Guid userId, string email, string telefone = null);
        // Task<IdentityResult> AlterarSenhaAsync(string userId, string senhaAtual, string novaSenha);
        // Task<IdentityResult> ResetarSenhaAsync(string email, string token, string novaSenha);
        // Task<string> GerarTokenResetSenhaAsync(string email);
        // Task<bool> ConfirmarEmailAsync(string userId, string token);
        // Task<string> GerarTokenConfirmacaoEmailAsync(string userId);
        Task<IdentityUser<Guid>> BuscarPorEmailAsync(string email);
        // Task<IdentityUser> BuscarPorIdAsync(string id);
        // Task<bool> VerificarSenhaAsync(string userId, string senha);
        // Task<IdentityResult> BloquearUsuarioAsync(string userId);
        // Task<IdentityResult> DesbloquearUsuarioAsync(string userId);
        // Task<IdentityResult> AtivarUsuarioAsync(string userId);
        // Task<IdentityResult> DesativarUsuarioAsync(string userId);
    }
}
