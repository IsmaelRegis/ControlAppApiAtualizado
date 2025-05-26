
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ControlApp.Infra.Data.Mappings
{
    public class AuditoriaMap : IEntityTypeConfiguration<Auditoria>
    {
        public void Configure(EntityTypeBuilder<Auditoria> builder)
        {
            builder.ToTable("AUDITORIAS");

            builder.HasKey(a => a.AuditoriaId);

            builder.Property(a => a.AuditoriaId)
                .IsRequired();

            builder.Property(a => a.UsuarioId)
                .IsRequired();

            builder.Property(a => a.NomeUsuario)
                .IsRequired(false)
                .HasMaxLength(150);

            builder.Property(a => a.Acao)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(a => a.Resumo)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(a => a.Papel)
               .IsRequired()
               .HasConversion<string>() // ou .HasConversion<int>() se quiser salvar como int
               .HasMaxLength(50);

            builder.Property(a => a.DataHora)
                .IsRequired(false);
        }
    }
}
