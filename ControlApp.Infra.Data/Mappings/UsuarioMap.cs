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
                .IsRequired(false); 

            builder.HasIndex(u => u.UserName)
                .IsUnique();

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

            builder.HasDiscriminator<string>("TipoUsuario")
                .HasValue<Usuario>("Usuario") 
                .HasValue<Tecnico>("Tecnico")
                .HasValue<Administrador>("Administrador")
                .HasValue<SuperAdministrador>("SuperAdministrador")
                .HasValue<Visitante>("Visitante");
        }
    }
}