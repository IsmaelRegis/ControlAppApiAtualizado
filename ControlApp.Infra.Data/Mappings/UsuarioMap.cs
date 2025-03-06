using ControlApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlApp.Infra.Data.Mappings
{
    public class UsuarioMap : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("USUARIOS");

            // Chave primária definida apenas na classe base
            builder.HasKey(u => u.UsuarioId);

            builder.Property(u => u.Nome)
                .HasColumnName("NOME")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.Email)
                .HasColumnName("EMAIL")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(u => u.Senha)
                .HasColumnName("SENHA")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.UserName)
                .HasColumnName("USERNAME")
                .HasMaxLength(100)
                .IsRequired(false); // Opcional, já que nem todos os usuários podem ter UserName

            builder.Property(u => u.Role)
                .HasColumnName("ROLE")
                .HasConversion<int>()
                .IsRequired();

            builder.Property(u => u.Ativo)
                .HasColumnName("ATIVO")
                .HasDefaultValue(true);

            builder.Property(u => u.FotoUrl)
                .HasColumnName("FOTO_URL")
                .HasMaxLength(250)
                .IsRequired(false);

            builder.Property(u => u.DataHoraUltimaAutenticacao)
                .HasColumnName("DATA_HORA_ULTIMA_AUTENTICACAO")
                .IsRequired(false);

            // Configuração do discriminador para TPH
            builder.HasDiscriminator<string>("TipoUsuario")
                .HasValue<Usuario>("Usuario") // Classe base (pode ser abstrata ou genérica)
                .HasValue<Tecnico>("Tecnico")
                .HasValue<Administrador>("Administrador");
        }
    }
}