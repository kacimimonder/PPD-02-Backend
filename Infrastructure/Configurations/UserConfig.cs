using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations
{
    using Domain.Entities;
    using Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    namespace Infrastructure.Persistence.Configurations
    {
        public class UserConfig : IEntityTypeConfiguration<User>
        {
            public void Configure(EntityTypeBuilder<User> builder)
            {
                builder.ToTable("Users"); // This tells EF to use "Users" table

                // Primary Key
                builder.HasKey(u => u.Id);

                // Properties
                builder.Property(u => u.Id)
                    .ValueGeneratedOnAdd(); // Auto-increment

                // Add this for UserTypeEnum
                builder.Property(u => u.UserType)
                    .IsRequired()
                    .HasConversion<int>()
                    .HasColumnType("tinyint")
                    .HasComment("1=Student, 2=Instructor"); // Documentation

                // Optional: Add index for UserType if you frequently query by role
                builder.HasIndex(u => u.UserType)
                    .HasDatabaseName("IX_Users_UserType");

                builder.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(20);

                builder.Property(u => u.LastName)
                    .HasMaxLength(20);

                builder.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false); // More efficient for email storage

                builder.Property(u => u.Password)
                    .IsRequired()
                    .HasMaxLength(255); // Store hashed passwords only

                builder.Property(u => u.PhotoUrl)
                    .HasMaxLength(500)
                    .IsRequired(false);

                // Indexes
                builder.HasIndex(u => u.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email"); // Named index

                // Seed Data (Optional)
                builder.HasData(
                    new User
                    {
                        Id = 1,
                        FirstName = "Admin",
                        LastName = "System",
                        Email = "admin@example.com",
                        Password = "hashed_password_here", // Always store hashed!
                        PhotoUrl = null
                    }
                );
            }
        }
    }
}
