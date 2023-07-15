using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsTagConfiguration : IEntityTypeConfiguration<CmsTag>
    {
        public void Configure(EntityTypeBuilder<CmsTag> builder)
        {
            builder.ToTable("cmsTags");

            builder.HasIndex(e => new { e.Group, e.Tag, e.LanguageId }, "IX_cmsTags").IsUnique();

            builder.HasIndex(e => e.LanguageId, "IX_cmsTags_LanguageId");

            builder.HasIndex(e => new { e.LanguageId, e.Group }, "IX_cmsTags_languageId_group");

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Group)
                .HasMaxLength(100)
                .HasColumnName("group");
            builder.Property(e => e.LanguageId).HasColumnName("languageId");
            builder.Property(e => e.Tag)
                .HasMaxLength(200)
                .HasColumnName("tag");

            builder.HasOne(d => d.Language).WithMany(p => p.CmsTags)
                .HasForeignKey(d => d.LanguageId)
                .HasConstraintName("FK_cmsTags_umbracoLanguage_id");
        }
    }
}
