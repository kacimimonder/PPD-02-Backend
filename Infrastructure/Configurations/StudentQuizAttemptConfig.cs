using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class StudentQuizAttemptConfig : IEntityTypeConfiguration<StudentQuizAttempt>
    {
        public void Configure(EntityTypeBuilder<StudentQuizAttempt> builder)
        {
            builder.ToTable("StudentQuizAttempts");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.StudentResponses)
                .IsRequired();

            builder.Property(a => a.Score)
                .HasColumnType("decimal(5,2)");

            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasIndex(a => new { a.EnrollmentId, a.AiGeneratedQuizId, a.CreatedAt });
            builder.HasIndex(a => a.QuizAssignmentId);

            builder.HasOne(a => a.AiGeneratedQuiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(a => a.AiGeneratedQuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Enrollment)
                .WithMany()
                .HasForeignKey(a => a.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.QuizAssignment)
                .WithMany(q => q.Attempts)
                .HasForeignKey(a => a.QuizAssignmentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
