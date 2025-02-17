using ControlApp.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class LocalizacaoMap : IEntityTypeConfiguration<Localizacao>
{
    public void Configure(EntityTypeBuilder<Localizacao> builder)
    {
        builder.ToTable("LOCALIZACOES");
        builder.HasKey(l => l.LocalizacaoId);

        builder.Property(l => l.Latitude)
            .HasColumnName("LATITUDE")
            .HasColumnType("FLOAT");

        builder.Property(l => l.Longitude)
            .HasColumnName("LONGITUDE")
            .HasColumnType("FLOAT");

        builder.Property(l => l.DataHora)
            .HasColumnName("DATA_HORA");

        builder.Property(l => l.Precisao)
            .HasColumnName("PRECISAO");

        // Relacionamento com Trajeto apenas
        builder.HasOne(l => l.Trajeto)
            .WithMany(t => t.Localizacoes)
            .HasForeignKey(l => l.TrajetoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
