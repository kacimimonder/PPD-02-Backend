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
    public class SubjectConfig : IEntityTypeConfiguration<Subject>
    {
        public void Configure(EntityTypeBuilder<Subject> builder)
        {
            builder.HasKey(s => s.SubjectId);

            builder.Property(s => s.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasMany(s => s.Courses)
                   .WithOne(c => c.Subject)
                   .HasForeignKey(c => c.SubjectID);

            // Seed data
            builder.HasData(
                new Subject { SubjectId = 1, Name = "Business" },
                new Subject { SubjectId = 2, Name = "Computer Science" },
                new Subject { SubjectId = 3, Name = "Information Technology" },
                new Subject { SubjectId = 4, Name = "Data Science" },
                new Subject { SubjectId = 5, Name = "Health" },
                new Subject { SubjectId = 6, Name = "Physical Science and Engineering" },
                new Subject { SubjectId = 7, Name = "Social Sciences" },
                new Subject { SubjectId = 8, Name = "Arts and Humanities" },
                new Subject { SubjectId = 9, Name = "Personal Development" },
                new Subject { SubjectId = 10, Name = "Language Learning" },
                new Subject { SubjectId = 11, Name = "Math and Logic" }
            );
        }
    }

}
