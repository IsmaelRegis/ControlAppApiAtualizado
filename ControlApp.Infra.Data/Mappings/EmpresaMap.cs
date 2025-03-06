using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ControlApp.Domain.Entities;

namespace ControlApp.Infra.Data.Mappings
{
    public class EmpresaMap : IEntityTypeConfiguration<Empresa>
    {
        public void Configure(EntityTypeBuilder<Empresa> builder)
        {
            builder.ToTable("EMPRESAS");

            builder.HasKey(e => e.EmpresaId);

            builder.Property(e => e.NomeDaEmpresa)
                   .HasColumnName("NOME_EMPRESA")
                   .HasMaxLength(255)
                   .IsRequired();

            builder.Property(e => e.Ativo)
                   .HasColumnName("ATIVO")
                   .HasDefaultValue(true);

            builder.OwnsOne(e => e.Endereco, endereco =>
            {
                endereco.Property(x => x.Cep)
                        .HasColumnName("CEP")
                        .HasMaxLength(10);

                endereco.Property(x => x.Logradouro)
                        .HasColumnName("LOGRADOURO")
                        .HasMaxLength(255);

                endereco.Property(x => x.Bairro)
                        .HasColumnName("BAIRRO")
                        .HasMaxLength(255);

                endereco.Property(x => x.Cidade)
                        .HasColumnName("CIDADE")
                        .HasMaxLength(255);

                endereco.Property(x => x.Estado)
                        .HasColumnName("ESTADO")
                        .HasMaxLength(50);

                endereco.Property(x => x.Complemento)
                        .HasColumnName("COMPLEMENTO")
                        .HasMaxLength(255);

                endereco.Property(x => x.Numero)
                        .HasColumnName("NUMERO")
                        .HasMaxLength(50);
            });

            builder.HasMany(e => e.Tecnicos)
                   .WithOne(t => t.Empresa)
                   .HasForeignKey(t => t.EmpresaId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}