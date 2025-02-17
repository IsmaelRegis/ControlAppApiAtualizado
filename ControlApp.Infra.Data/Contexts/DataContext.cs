using ControlApp.Domain.Entities;
using ControlApp.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace ControlApp.Infra.Data.Contexts
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Ponto> Pontos { get; set; }
        public DbSet<Trajeto> Trajetos { get; set; }
        public DbSet<Localizacao> Localizacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new PontoMap());
            modelBuilder.ApplyConfiguration(new UsuarioMap());
            modelBuilder.ApplyConfiguration(new TrajetoMap());
            modelBuilder.ApplyConfiguration(new LocalizacaoMap());

        }
    }
}
