using ControlApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PontoMap : IEntityTypeConfiguration<Ponto>
{
    public void Configure(EntityTypeBuilder<Ponto> builder)
    {
        builder.ToTable("PONTOS");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.InicioExpediente).HasColumnName("INICIO_EXPEDIENTE").IsRequired(false);
        builder.Property(p => p.FimExpediente).HasColumnName("FIM_EXPEDIENTE").IsRequired(false);
        builder.Property(p => p.InicioPausa).HasColumnName("INICIO_PAUSA").IsRequired(false);
        builder.Property(p => p.RetornoPausa).HasColumnName("RETORNO_PAUSA").IsRequired(false);

        builder.Property(p => p.HorasTrabalhadas)
               .HasColumnName("HORAS_TRABALHADAS")
               .HasColumnType("TIME");

        builder.Property(p => p.HorasExtras)
               .HasColumnName("HORAS_EXTRAS")
               .HasColumnType("TIME");

        builder.Property(p => p.HorasDevidas)
               .HasColumnName("HORAS_DEVIDAS")
               .HasColumnType("TIME");

        // Latitude e Longitude para o Início de Expediente e Início de Pausa
        builder.Property(p => p.LatitudeInicioExpediente)
                 .HasColumnName("LATITUDE_INICIO_EXPEDIENTE")
                 .HasColumnType("VARCHAR(50)");

        builder.Property(p => p.LongitudeInicioExpediente)
               .HasColumnName("LONGITUDE_INICIO_EXPEDIENTE")
               .HasColumnType("VARCHAR(50)");

        builder.Property(p => p.LatitudeInicioPausa)
               .HasColumnName("LATITUDE_INICIO_PAUSA")
               .HasColumnType("FLOAT");

        builder.Property(p => p.LongitudeInicioPausa)
               .HasColumnName("LONGITUDE_INICIO_PAUSA")
               .HasColumnType("FLOAT");

        builder.Property(p => p.LatitudeFimExpediente)
                 .HasColumnName("LATITUDE_FIM_EXPEDIENTE")
                 .HasColumnType("VARCHAR(50)");

        builder.Property(p => p.LongitudeFimExpediente)
               .HasColumnName("LONGITUDE_FIM_EXPEDIENTE")
               .HasColumnType("VARCHAR(50)");

        builder.Property(p => p.LatitudeRetornoPausa)
               .HasColumnName("LATITUDE_RETORNO_PAUSA")
               .HasColumnType("FLOAT");

        builder.Property(p => p.LongitudeRetornoPausa)
               .HasColumnName("LONGITUDE_RETORNO_PAUSA")
               .HasColumnType("FLOAT");

        // Observações específicas para cada ponto
        builder.Property(p => p.ObservacaoInicioExpediente)
               .HasColumnName("OBSERVACAO_INICIO_EXPEDIENTE")
               .HasMaxLength(500);

        builder.Property(p => p.ObservacaoFimExpediente)
               .HasColumnName("OBSERVACAO_FIM_EXPEDIENTE")
               .HasMaxLength(500);

        builder.Property(p => p.ObservacaoInicioPausa)
               .HasColumnName("OBSERVACAO_INICIO_PAUSA")
               .HasMaxLength(500);

        builder.Property(p => p.ObservacaoFimPausa)
               .HasColumnName("OBSERVACAO_FIM_PAUSA")
               .HasMaxLength(500);

        builder.Property(p => p.UsuarioId)
               .HasColumnName("USUARIO_ID")
               .IsRequired();

        builder.Property(p => p.Ativo)
               .HasColumnName("ATIVO")
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(p => p.FotoInicioExpediente)
               .HasColumnName("FOTO_INICIO_EXPEDIENTE")
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(p => p.FotoFimExpediente)
               .HasColumnName("FOTO_FIM_EXPEDIENTE")
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(p => p.TipoPonto)
               .HasColumnName("TIPO_PONTO")
               .HasConversion<int>()
               .IsRequired();

        // Ajustando o mapeamento do campo DistanciaPercorrida para string
        builder.Property(p => p.DistanciaPercorrida)
               .HasColumnName("DISTANCIA_PERCORRIDA")
               .HasColumnType("VARCHAR(100)")
               .IsRequired(false); // Opcional

        builder.HasOne(p => p.Tecnico)
               .WithMany(t => t.Pontos)
               .HasForeignKey(p => p.UsuarioId);
    }
}
