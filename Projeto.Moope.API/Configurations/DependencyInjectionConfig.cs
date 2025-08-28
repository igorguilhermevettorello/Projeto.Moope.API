using FluentValidation;
using Projeto.Moope.API.DTOs.Clientes;
using Projeto.Moope.API.DTOs.Planos;
using Projeto.Moope.API.DTOs.Validators;
using Projeto.Moope.API.Utils;
using Projeto.Moope.Core.Commands.Clientes.AlterarSenha;
using Projeto.Moope.Core.Commands.Clientes.Atualizar;
using Projeto.Moope.Core.Commands.Emails;
using Projeto.Moope.Core.Commands.Vendas;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Interfaces.UnitOfWork;
using Projeto.Moope.Core.Interfaces.Utils;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Core.Services;
using Projeto.Moope.Core.Utils;
using Projeto.Moope.Infrastructure.Data;
using Projeto.Moope.Infrastructure.Data.UnitOfWork;
using Projeto.Moope.Infrastructure.Repositories;

namespace Projeto.Moope.API.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddDependencyInjectionConfig(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterApplicationDependencies(services, configuration);
            RegisterRepositories(services);
            RegisterMediatR(services);
            RegisterServices(services);
            RegisterValidators(services);
            return services;
        }

        private static void RegisterApplicationDependencies(IServiceCollection service, IConfiguration configuration)
        {
            service.AddScoped<AppDbContext>();
            service.AddScoped<INotificador, Notificador>();
            service.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            service.AddScoped<IUser, AspNetUser>();
            service.AddScoped<IPasswordGenerator, PasswordGenerator>();
        }

        private static void RegisterRepositories(IServiceCollection service)
        {
            service.AddScoped<IPlanoRepository, PlanoRepository>();
            service.AddScoped<IClienteRepository, ClienteRepository>();
            service.AddScoped<IVendedorRepository, VendedorRepository>();
            service.AddScoped<IEnderecoRepository, EnderecoRepository>();
            service.AddScoped<IUsuarioRepository, UsuarioRepository>();
            service.AddScoped<IPessoaFisicaRepository, PessoaFisicaRepository>();
            service.AddScoped<IPessoaJuridicaRepository, PessoaJuridicaRepository>();
            service.AddScoped<IPapelRepository, PapelRepository>();
            service.AddScoped<IPedidoRepository, PedidoRepository>();
            service.AddScoped<ITransacaoRepository, TransacaoRepository>();
            service.AddScoped<IEmailRepository, EmailRepository>();
            service.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        private static void RegisterServices(IServiceCollection service)
        {
            service.AddScoped<IPlanoService, PlanoService>();
            service.AddScoped<IPapelService, PapelService>();
            service.AddScoped<IClienteService, ClienteService>();
            service.AddScoped<IVendedorService, VendedorService>();
            service.AddScoped<IEnderecoService, EnderecoService>();
            service.AddScoped<IUsuarioService, UsuarioService>();
            service.AddScoped<IIdentityUserService, IdentityUserService>();
            service.AddScoped<IVendaService, VendaService>();
            service.AddScoped<IEmailService, EmailService>();
            service.AddHttpClient<IGoogleRecaptchaService, GoogleRecaptchaService>();
        }

        private static void RegisterValidators(IServiceCollection service)
        {
            service.AddScoped<IValidator<CreatePlanoDto>, PlanoDtoValidator>();
            service.AddScoped<IValidator<CreateClienteDto>, CreateClienteDtoValidator>();
            service.AddScoped<IValidator<UpdateClienteDto>, UpdateClienteDtoValidator>();
            service.AddScoped<IValidator<AlterarSenhaClienteDto>, AlterarSenhaClienteDtoValidator>();
            service.AddScoped<IValidator<AlterarSenhaAdminDto>, AlterarSenhaAdminDtoValidator>();
            // service.AddScoped<IValidator<CreateVendaDto>, CreateVendaDtoValidator>();
        }

        private static void RegisterMediatR(IServiceCollection service)
        {
            service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProcessarVendaCommand).Assembly));
            service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SalvarEmailCommand).Assembly));
            service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AtualizarClienteCommand).Assembly));
            service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AlterarSenhaClienteCommand).Assembly));
            service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AlterarSenhaAdminCommand).Assembly));
        }
    }
}