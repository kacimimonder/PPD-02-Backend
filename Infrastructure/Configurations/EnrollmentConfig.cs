using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class EnrollmentConfig : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.ToTable("Enrollments");

            builder.HasKey(e => e.Id); // Use this if you're using a separate Id field
            // builder.HasKey(e => new { e.StudentId, e.CourseId }); // Use this if you're using composite keys instead of an Id

            builder.Property(e => e.EnrollmentDate)
                   .HasDefaultValueSql("GETDATE()"); // SQL Server default to current time

            builder.Property(e => e.IsCompleted)
                   .HasDefaultValue(false);

            builder.HasOne(e => e.Student)
                   .WithMany(u => u.Enrollments)
                   .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // <--- this prevents cascade path issues

            builder.HasOne(e => e.Course)
                   .WithMany(c => c.Enrollments)
                   .HasForeignKey(e => e.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => new { e.StudentId, e.CourseId })
                   .IsUnique(); // Ensure that a student can only enroll in a course once
        }

    }
}
