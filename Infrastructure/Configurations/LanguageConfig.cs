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
    public class LanguageConfig : IEntityTypeConfiguration<Language>
    {
        public void Configure(EntityTypeBuilder<Language> builder)
        {
            builder.HasKey(l => l.LanguageId);

            builder.Property(l => l.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasMany(l => l.Courses)
                   .WithOne(c => c.Language)
                   .HasForeignKey(c => c.LanguageID);

            // Seed data
            builder.HasData(
                new Language { LanguageId = 1, Name = "English" },
                new Language { LanguageId = 2, Name = "Spanish" },
                new Language { LanguageId = 3, Name = "French" },
                new Language { LanguageId = 4, Name = "Arabic" },
                new Language { LanguageId = 5, Name = "Portuguese (Brazil)" },
                new Language { LanguageId = 6, Name = "German" },
                new Language { LanguageId = 7, Name = "Chinese (China)" },
                new Language { LanguageId = 8, Name = "Japanese" },
                new Language { LanguageId = 9, Name = "Indonesian" },
                new Language { LanguageId = 10, Name = "Russian" },
                new Language { LanguageId = 11, Name = "Korean" },
                new Language { LanguageId = 12, Name = "Hindi" },
                new Language { LanguageId = 13, Name = "Turkish" },
                new Language { LanguageId = 14, Name = "Ukrainian" },
                new Language { LanguageId = 15, Name = "Italian" },
                new Language { LanguageId = 16, Name = "Thai" },
                new Language { LanguageId = 17, Name = "Polish" },
                new Language { LanguageId = 18, Name = "Dutch" },
                new Language { LanguageId = 19, Name = "Swedish" },
                new Language { LanguageId = 20, Name = "Greek" },
                new Language { LanguageId = 21, Name = "Kazakh" },
                new Language { LanguageId = 22, Name = "Hungarian" },
                new Language { LanguageId = 23, Name = "Azerbaijani" },
                new Language { LanguageId = 24, Name = "Vietnamese" },
                new Language { LanguageId = 25, Name = "Pushto" },
                new Language { LanguageId = 26, Name = "Chinese (Traditional)" },
                new Language { LanguageId = 27, Name = "Hebrew" },
                new Language { LanguageId = 28, Name = "Portuguese" },
                new Language { LanguageId = 29, Name = "Portuguese (Portugal)" },
                new Language { LanguageId = 30, Name = "Catalan" },
                new Language { LanguageId = 31, Name = "Croatian" },
                new Language { LanguageId = 32, Name = "Kannada" },
                new Language { LanguageId = 33, Name = "Swahili" }
            );
        }
    }
}
