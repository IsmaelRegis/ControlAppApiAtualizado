using ControlApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlApp.Infra.Data.Mappings
{
    public class UserTokenMap : IEntityTypeConfiguration<UserToken>
    {
        public void Configure(EntityTypeBuilder<UserToken> builder)
        {
            builder.ToTable("USER_TOKENS");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("ID")
                .IsRequired();

            builder.Property(t => t.UserId)
                .HasColumnName("USER_ID")
                .IsRequired();

            builder.Property(t => t.Token)
                .HasColumnName("TOKEN")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(t => t.CreatedAt)
                .HasColumnName("CREATED_AT")
                .IsRequired();

            builder.Property(t => t.ExpiresAt) // Nova propriedade
                .HasColumnName("EXPIRES_AT")
                .IsRequired();
            
            builder.Property(t => t.IsActive)
                .HasColumnName("IS_ACTIVE")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(t => t.DeviceInfo)
                .HasColumnName("DEVICE_INFO")
                .HasMaxLength(500)
                .IsRequired(false);

            // Cria um índice para melhorar a performance de consulta
            builder.HasIndex(t => t.UserId);

            // Cria um índice para consulta por token
            builder.HasIndex(t => t.Token);

            // Adiciona índice para ExpiresAt para melhorar performance ao buscar tokens expirados
            builder.HasIndex(t => t.ExpiresAt);
        }
    }
}