using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsLanguageTextConfiguration : IEntityTypeConfiguration<CmsLanguageText>
    {
        public void Configure(EntityTypeBuilder<CmsLanguageText> builder)
        {
            builder.HasKey(e => e.Pk);

            builder.ToTable("cmsLanguageText");

            builder.HasIndex(e => new { e.LanguageId, e.UniqueId }, "IX_cmsLanguageText_languageId").IsUnique();

            builder.Property(e => e.Pk).HasColumnName("pk");
            builder.Property(e => e.LanguageId).HasColumnName("languageId");
            builder.Property(e => e.Value)
                .HasMaxLength(1000)
                .HasColumnName("value");

            builder.HasOne(d => d.Language).WithMany(p => p.CmsLanguageTexts)
                .HasForeignKey(d => d.LanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsLanguageText_umbracoLanguage_id");

            builder.HasOne(d => d.Unique).WithMany(p => p.CmsLanguageTexts)
                .HasPrincipalKey(p => p.Id)
                .HasForeignKey(d => d.UniqueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsLanguageText_cmsDictionary_id");
        }
    }
}
