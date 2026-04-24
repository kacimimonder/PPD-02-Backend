using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class QuizAssignmentConfig : IEntityTypeConfiguration<QuizAssignment>
    {
        public void Configure(EntityTypeBuilder<QuizAssignment> builder)
        {
            builder.ToTable("QuizAssignments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.AssignedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(a => a.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(a => new { a.AiGeneratedQuizId, a.EnrollmentId })
                .IsUnique();

            builder.HasIndex(a => a.AssignedByInstructorId);

            builder.HasOne(a => a.AiGeneratedQuiz)
                .WithMany(q => q.Assignments)
                .HasForeignKey(a => a.AiGeneratedQuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Enrollment)
                .WithMany()
                .HasForeignKey(a => a.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.AssignedByInstructor)
                .WithMany()
                .HasForeignKey(a => a.AssignedByInstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
