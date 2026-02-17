using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Configurations
{
    public class CourseModuleConfig : IEntityTypeConfiguration<CourseModule>
    {
        public void Configure(EntityTypeBuilder<CourseModule> builder)
        {
            builder.ToTable("CourseModules");

            builder.HasKey(courseModule => courseModule.Id);

            builder.Property(courseModule => courseModule.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(courseModule => courseModule.Description)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.HasMany(courseModule => courseModule.ModuleContents)
                   .WithOne(moduleContent => moduleContent.courseModule)
                   .HasForeignKey(moduleContent => moduleContent.CourseModuleID);

        }

    }

}
