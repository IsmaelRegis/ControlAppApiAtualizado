using System;
using ControlApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlApp.Infra.Data.Mappings
{
    public class TecnicoMap : IEntityTypeConfiguration<Tecnico>
    {
        public void Configure(EntityTypeBuilder<Tecnico> builder)
        {
            builder.ToTable("USUARIOS");

            builder.HasKey(t => t.UsuarioId);

            builder.Property(t => t.Cpf)
                .HasColumnName("CPF")
                .HasMaxLength(100)
                .IsRequired(); // Garantindo que o CPF seja obrigatório

            builder.HasIndex(t => t.Cpf)  // Adiciona o índice único no CPF
                .IsUnique(); // Garante que o CPF seja único no banco de dados

            builder.Property(t => t.HoraEntrada)
                .HasColumnName("HORA_ENTRADA")
                .HasColumnType("TIME")
                .HasDefaultValueSql("CAST('00:00:00' AS TIME)")
                .IsRequired();  // Hora de entrada é obrigatória

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

            // Mapeamento das novas propriedades LatitudeAtual e LongitudeAtual
            builder.Property(t => t.LatitudeAtual)
                .HasColumnName("LATITUDE_ATUAL")
                .HasMaxLength(50)  // Define um limite de caracteres para a latitude
                .IsRequired(false); // Pode ser nulo

            builder.Property(t => t.LongitutdeAtual)
                .HasColumnName("LONGITUDE_ATUAL")
                .HasMaxLength(50)  // Define um limite de caracteres para a longitude
                .IsRequired(false); // Pode ser nulo

            builder.Property(t => t.NumeroMatricula)
                .HasColumnName("NUMERO_MATRRICULA")
                .HasMaxLength(50) // Ajuste o tamanho conforme necessário
                .IsRequired(false); // Opcional

            builder.Property(t => t.EmpresaId)
                .HasColumnName("EMPRESA_ID")
                .IsRequired(false); //

            builder.HasOne<Usuario>()
                .WithOne()
                .HasForeignKey<Tecnico>(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasBaseType<Usuario>(); // Garantindo que Tecnico é uma subclasse de Usuario
        }
    }
}
