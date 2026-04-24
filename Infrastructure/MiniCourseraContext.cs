using System;
using Infrastructure.Configurations;
using Infrastructure.Configurations.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure
{
    public class MiniCourseraContext:DbContext
    {
        public DbSet<Domain.Entities.Course> Courses { get; set; }
        public DbSet<Domain.Entities.User> Users { get; set; }
        public DbSet<Domain.Entities.Enrollment> Enrollments { get; set; }
        public DbSet<Domain.Entities.CourseModule> CourseModules { get; set; }
        public DbSet<Domain.Entities.ModuleContent> ModuleContents { get; set; }
        public DbSet<Domain.Entities.EnrollmentProgress> EnrollmentProgresses { get; set; }
        public DbSet<Domain.Entities.RefreshToken> RefreshTokens { get; set; }
        public DbSet<Domain.Entities.AiGeneratedQuiz> AiGeneratedQuizzes { get; set; }
        public DbSet<Domain.Entities.QuizAssignment> QuizAssignments { get; set; }
        public DbSet<Domain.Entities.StudentQuizAttempt> StudentQuizAttempts { get; set; }

        public MiniCourseraContext(DbContextOptions<MiniCourseraContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SubjectConfig());
            modelBuilder.ApplyConfiguration(new LanguageConfig());
            modelBuilder.ApplyConfiguration(new UserConfig());
            modelBuilder.ApplyConfiguration(new EnrollmentConfig());
            modelBuilder.ApplyConfiguration(new EnrollmentProgressConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new AiGeneratedQuizConfig());
            modelBuilder.ApplyConfiguration(new QuizAssignmentConfig());
            modelBuilder.ApplyConfiguration(new StudentQuizAttemptConfig());
        }
    }
}
