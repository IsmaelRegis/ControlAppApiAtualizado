using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class TrajetoMap : IEntityTypeConfiguration<Trajeto>
{
    public void Configure(EntityTypeBuilder<Trajeto> builder)
    {
        builder.ToTable("TRAJETOS");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Data)
               .HasColumnName("DATA")
               .IsRequired();

        builder.Property(t => t.DistanciaTotalKm)
               .HasColumnName("DISTANCIA_TOTAL_KM")
               .HasColumnType("DECIMAL(10,2)")
               .IsRequired();

        builder.Property(t => t.DuracaoTotal)
               .HasColumnName("DURACAO_TOTAL")
               .HasColumnType("TIME")
               .HasDefaultValueSql("CAST('00:00:00' AS TIME)")
               .IsRequired();

        builder.Property(t => t.UsuarioId)
               .HasColumnName("USUARIO_ID")
               .IsRequired();

        builder.HasOne(t => t.Tecnico)
               .WithMany(tecnico => tecnico.Trajetos)
               .HasForeignKey(t => t.UsuarioId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Localizacoes)
               .WithOne(l => l.Trajeto)
               .HasForeignKey(l => l.TrajetoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.Status)
               .HasColumnName("STATUS")
               .HasMaxLength(50)
               .HasDefaultValue("Em andamento")
               .IsRequired();
    }
}
