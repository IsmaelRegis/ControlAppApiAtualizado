using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ControlApp.Domain.Entities;

namespace ControlApp.Infra.Data.Mappings
{
    public class TecnicoMap : IEntityTypeConfiguration<Tecnico>
    {
        public void Configure(EntityTypeBuilder<Tecnico> builder)
        {
            // Não precisamos definir ToTable ou HasKey, pois isso é herdado de UsuarioMap

            builder.Property(t => t.Cpf)
                .HasColumnName("CPF")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(t => t.Cpf)
                .IsUnique();

            builder.Property(t => t.HoraEntrada)
                .HasColumnName("HORA_ENTRADA")
                .HasColumnType("TIME")
                .HasDefaultValueSql("CAST('00:00:00' AS TIME)")
                .IsRequired();

            builder.Property(t => t.HoraSaida)
                .HasColumnName("HORA_SAIDA")
                .HasColumnType("TIME")
                .HasDefaultValueSql("CAST('00:00:00' AS TIME)");

            builder.Property(t => t.HoraAlmocoInicio)
                .HasColumnName("HORA_ALMOCO_INICIO")
                .HasColumnType("TIME")
                .HasDefaultValueSql("CAST('00:00:00' AS TIME)");

            builder.Property(t => t.HoraAlmocoFim)
                .HasColumnName("HORA_ALMOCO_FIM")
                .HasColumnType("TIME")
                .HasDefaultValueSql("CAST('00:00:00' AS TIME)");

            builder.Property(t => t.IsOnline)
                .HasColumnName("IS_ONLINE")
                .HasDefaultValue(false);

            builder.Property(t => t.LatitudeAtual)
                .HasColumnName("LATITUDE_ATUAL")
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(t => t.LongitutdeAtual) // Corrigido o typo de "LongitutdeAtual"
                .HasColumnName("LONGITUDE_ATUAL")
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(t => t.NumeroMatricula)
                .HasColumnName("NUMERO_MATRICULA") // Corrigido "NUMERO_MATRRICULA"
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(t => t.EmpresaId)
                .HasColumnName("EMPRESA_ID")
                .IsRequired(false);

            // Relacionamento com Empresa
            builder.HasOne(t => t.Empresa)
                .WithMany(e => e.Tecnicos)
                .HasForeignKey(t => t.EmpresaId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}