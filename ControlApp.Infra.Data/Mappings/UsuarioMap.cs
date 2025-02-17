using ControlApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlApp.Infra.Data.Mappings
{
    public class UsuarioMap : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("USUARIOS");  // Nome da tabela no banco
            builder.HasKey(u => u.UsuarioId);  // Definindo a chave primária

            builder.Property(u => u.UsuarioId)
                .IsRequired();

            builder.Property(u => u.Nome)
                .HasColumnName("NOME")
                .HasMaxLength(100)
                .IsRequired();  // Nome do usuário

            builder.Property(u => u.Email)
                .HasColumnName("EMAIL")
                .HasMaxLength(150)
                .IsRequired();  // Email do usuário

            builder.Property(u => u.Senha)
                .HasColumnName("SENHA")
                .HasMaxLength(100)
                .IsRequired();  // Senha do usuário

            builder.Property(u => u.Role)
                .HasColumnName("ROLE")
                .HasConversion<int>()  // Conversão de Enum para int
                .IsRequired();  // Role (tipo do usuário)

            builder.Property(u => u.Ativo)
                .HasColumnName("ATIVO")
                .IsRequired()
                .HasDefaultValue(true);  // Indicador de se o usuário está ativo

            // Propriedade para a URL da foto
            builder.Property(u => u.FotoUrl)
                .HasColumnName("FOTOURL")
                .HasMaxLength(250)
                .IsRequired(false);

            builder.Property(u => u.DataHoraUltimaAutenticacao)
               .HasColumnName("DATAHORAULTIMAAUTENTICACAO")
               .IsRequired(false); // Como é nullable, deixamos opcional


            // Configuração do Discriminador para os tipos de usuário (Tabela por Hierarquia - TPH)
            builder.HasDiscriminator<string>("TipoUsuario")  // Discriminador: TipoUsuario
                .HasValue<Usuario>("Usuario")  // Tipo base: Usuario
                .HasValue<Tecnico>("Tecnico")  // Tipo derivado: Tecnico
                .HasValue<Administrador>("Administrador");  // Tipo derivado: Administrador
        }
    }
}
