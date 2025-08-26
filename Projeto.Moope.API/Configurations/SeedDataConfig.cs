using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Infrastructure.Data;

namespace Projeto.Moope.API.Configurations
{
    public static class SeedDataConfig
    {
        public static async void UseSeedData(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
                var contextIdentity = serviceScope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                await context.Database.MigrateAsync();
                await contextIdentity.Database.MigrateAsync();
                await SeedRolesAsync(roleManager);
                await SeedUsersAsync(userManager, context);
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            foreach (var roleName in Enum.GetNames(typeof(TipoUsuario)))
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<IdentityUser<Guid>> userManager, AppDbContext context)
        {
            var emailAdmin = "admin@moope.com.br";
            if (await userManager.FindByEmailAsync(emailAdmin) == null)
            {
                var user = new IdentityUser<Guid>
                {
                    UserName = emailAdmin,
                    Email = emailAdmin,
                    EmailConfirmed = true,
                    LockoutEnabled = true
                };

                var result = await userManager.CreateAsync(user, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, TipoUsuario.Administrador.ToString());

                    var novoUsuario = new Usuario
                    {
                        Nome = "Administrador do Sistema",
                        TipoUsuario = TipoUsuario.Administrador,
                        Id = user.Id,
                        Created = DateTime.UtcNow,
                        Updated = DateTime.UtcNow
                    };
                    await context.Usuarios.AddAsync(novoUsuario);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
} 