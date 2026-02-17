using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Configurations
{
    public class ModuleContentConfig : IEntityTypeConfiguration<ModuleContent>
    {
        public void Configure(EntityTypeBuilder<ModuleContent> builder)
        {
            builder.HasKey(moduleContent => moduleContent.Id);

            builder.Property(moduleContent => moduleContent.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(moduleContent => moduleContent.VideoUrl)
                   .IsRequired()
                   .HasMaxLength(200);
        }

    }


}
