using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class AiGeneratedQuizConfig : IEntityTypeConfiguration<AiGeneratedQuiz>
    {
        public void Configure(EntityTypeBuilder<AiGeneratedQuiz> builder)
        {
            builder.ToTable("AiGeneratedQuizzes");

            builder.HasKey(q => q.Id);

            builder.Property(q => q.Output)
                .IsRequired();

            builder.Property(q => q.Language)
                .HasMaxLength(8)
                .HasDefaultValue("en");

            builder.Property(q => q.GenerationSource)
                .HasMaxLength(20)
                .HasDefaultValue("Student");

            builder.Property(q => q.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(q => q.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(q => q.ModuleId);
            builder.HasIndex(q => q.GeneratedByUserId);
            builder.HasIndex(q => q.GeneratedForEnrollmentId);

            builder.HasOne(q => q.Module)
                .WithMany()
                .HasForeignKey(q => q.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(q => q.GeneratedByUser)
                .WithMany()
                .HasForeignKey(q => q.GeneratedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(q => q.GeneratedForEnrollment)
                .WithMany()
                .HasForeignKey(q => q.GeneratedForEnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
