using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoLanguageConfiguration : IEntityTypeConfiguration<UmbracoLanguage>
    {
        public void Configure(EntityTypeBuilder<UmbracoLanguage> builder)
        {
            builder.ToTable("umbracoLanguage");

            builder.HasIndex(e => e.FallbackLanguageId, "IX_umbracoLanguage_fallbackLanguageId");

            builder.HasIndex(e => e.LanguageIsocode, "IX_umbracoLanguage_languageISOCode").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.FallbackLanguageId).HasColumnName("fallbackLanguageId");
            builder.Property(e => e.IsDefaultVariantLang)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("isDefaultVariantLang");
            builder.Property(e => e.LanguageCultureName)
                .HasMaxLength(100)
                .HasColumnName("languageCultureName");
            builder.Property(e => e.LanguageIsocode)
                .HasMaxLength(14)
                .HasColumnName("languageISOCode");
            builder.Property(e => e.Mandatory)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("mandatory");

            builder.HasOne(d => d.FallbackLanguage).WithMany(p => p.InverseFallbackLanguage)
                .HasForeignKey(d => d.FallbackLanguageId)
                .HasConstraintName("FK_umbracoLanguage_umbracoLanguage_id");
        }
    }
}
