using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Configurations
{
    public class EnrollmentProgressConfiguration : IEntityTypeConfiguration<EnrollmentProgress>
    {
        public void Configure(EntityTypeBuilder<EnrollmentProgress> builder)
        {
            builder.ToTable("EnrollmentProgresses");

            builder.HasIndex(e => new { e.EnrollmentId, e.ModuleContentId })
                   .IsUnique();

            builder.HasIndex(e => e.EnrollmentId);

            builder.HasOne(e => e.Enrollment)
                   .WithMany(e => e.enrollmentProgresses)
                   .HasForeignKey(e => e.EnrollmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ModuleContent)
                   .WithMany() // no navigation property on ModuleContent
                   .HasForeignKey(e => e.ModuleContentId);


        }
    }

}
