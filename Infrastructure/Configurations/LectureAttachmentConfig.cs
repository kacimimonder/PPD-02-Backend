using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class LectureAttachmentConfig : IEntityTypeConfiguration<LectureAttachment>
    {
        public void Configure(EntityTypeBuilder<LectureAttachment> builder)
        {
            builder.ToTable("LectureAttachments");

            builder.HasKey(attachment => attachment.Id);

            builder.Property(attachment => attachment.FileName)
                .IsRequired()
                .HasMaxLength(260);

            builder.Property(attachment => attachment.FileUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(attachment => attachment.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(attachment => attachment.AttachmentType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(attachment => attachment.CreatedAtUtc)
                .IsRequired();

            builder.HasIndex(attachment => attachment.ModuleContentId)
                .HasDatabaseName("IX_LectureAttachments_ModuleContentId");

            builder.HasOne(attachment => attachment.ModuleContent)
                .WithMany(moduleContent => moduleContent.LectureAttachments)
                .HasForeignKey(attachment => attachment.ModuleContentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
