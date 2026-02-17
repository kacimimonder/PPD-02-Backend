using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.TokenId);

            builder.Property(rt => rt.Token)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(rt => rt.UserId)
                   .IsRequired();

            builder.Property(rt => rt.ExpiresOn)
                   .IsRequired();

            builder.Property(rt => rt.CreatedOn)
                   .IsRequired();

            builder.Property(rt => rt.RevokedOn)
                   .IsRequired(false); // Nullable

            builder.Ignore(rt => rt.IsExpired); // computed
            builder.Ignore(rt => rt.IsActive);  // computed

            builder.HasIndex(rt => rt.UserId);

            // ✅ Unique index on Token
            builder.HasIndex(rt => rt.Token).IsUnique();
        }

    }

}
