using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Endereco> Enderecos { get; set; }
        public DbSet<Vendedor> Vendedores { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<PessoaFisica> PessoasFisicas { get; set; }
        public DbSet<PessoaJuridica> PessoasJuridicas { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }
        public DbSet<Plano> Planos { get; set; }
        public DbSet<Papel> Papeis { get; set; }
        public DbSet<Email> Emails { get; set; }
        
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(Guid) || p.PropertyType == typeof(Guid?));

                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.Name).Property(property.Name)
                        .HasColumnType("char(36)");
                }
            }

            // modelBuilder.Entity<PessoaFisica>()
            //     .HasOne(p => p.Cliente)
            //     .WithOne() // ou WithMany(), dependendo do relacionamento
            //     .HasForeignKey<PessoaFisica>(p => p.ClienteId);

            //modelBuilder.Entity<Usuario>()
            //    .HasOne<IdentityUser>()
            //    .WithMany()
            //    .HasForeignKey(u => u.IdentityUserId)
            //    .HasPrincipalKey(u => u.Id)
            //    .OnDelete(DeleteBehavior.Restrict)
            //    .IsRequired();

            // modelBuilder.Entity<Usuario>()
            //     .Property(u => u.IdentityUserId)
            //     .IsRequired();
            
            // modelBuilder.Entity<Cliente>()
            //     .HasOne(c => c.Usuario)
            //     .WithOne()
            //     .HasForeignKey<Cliente>(c => c.Id);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
