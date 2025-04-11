using ControlApp.Domain.Entities;
using ControlApp.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace ControlApp.Infra.Data.Contexts
{
    public class DataContext : DbContext
    {
        #region Construtor
        public DataContext(DbContextOptions<DataContext> options) : base(options) { } // Configura o contexto com as opções passadas
        #endregion

        #region DbSets
        public DbSet<Usuario> Usuarios { get; set; }      // Define a tabela de usuários no banco
        public DbSet<Ponto> Pontos { get; set; }          // Define a tabela de pontos (registros de entrada/saída)
        public DbSet<Trajeto> Trajetos { get; set; }      // Define a tabela de trajetos
        public DbSet<Localizacao> Localizacoes { get; set; } // Define a tabela de localizações
        public DbSet<Empresa> Empresas { get; set; }      // Define a tabela de empresas, adicionada pra suportar essa entidade
        public DbSet<UserToken> UserTokens { get; set; }  // Define a tabela de tokens de usuários
        #endregion

        #region Configuração do Modelo
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Aplica os mapeamentos pras entidades, dizendo como elas devem ser no banco
            modelBuilder.ApplyConfiguration(new PontoMap());
            modelBuilder.ApplyConfiguration(new UsuarioMap());
            modelBuilder.ApplyConfiguration(new TrajetoMap());
            modelBuilder.ApplyConfiguration(new LocalizacaoMap());
            modelBuilder.ApplyConfiguration(new EmpresaMap());
            modelBuilder.ApplyConfiguration(new TecnicoMap());
            modelBuilder.ApplyConfiguration(new UserTokenMap()); // Adiciona o mapeamento do UserToken
        }
        #endregion
    }
}