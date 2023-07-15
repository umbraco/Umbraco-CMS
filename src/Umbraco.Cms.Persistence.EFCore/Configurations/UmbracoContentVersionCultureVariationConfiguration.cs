using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoContentVersionCultureVariationConfiguration : IEntityTypeConfiguration<UmbracoContentVersionCultureVariation>
    {
        public void Configure(EntityTypeBuilder<UmbracoContentVersionCultureVariation> builder)
        {
            builder.ToTable("umbracoContentVersionCultureVariation");

            builder.HasIndex(e => e.LanguageId, "IX_umbracoContentVersionCultureVariation_LanguageId");

            builder.HasIndex(e => new { e.VersionId, e.LanguageId }, "IX_umbracoContentVersionCultureVariation_VersionId").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.AvailableUserId).HasColumnName("availableUserId");
            builder.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            builder.Property(e => e.LanguageId).HasColumnName("languageId");
            builder.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            builder.Property(e => e.VersionId).HasColumnName("versionId");

            builder.HasOne(d => d.AvailableUser).WithMany(p => p.UmbracoContentVersionCultureVariations)
                .HasForeignKey(d => d.AvailableUserId)
                .HasConstraintName("FK_umbracoContentVersionCultureVariation_umbracoUser_id");

            builder.HasOne(d => d.Language).WithMany(p => p.UmbracoContentVersionCultureVariations)
                .HasForeignKey(d => d.LanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoContentVersionCultureVariation_umbracoLanguage_id");

            builder.HasOne(d => d.Version).WithMany(p => p.UmbracoContentVersionCultureVariations)
                .HasForeignKey(d => d.VersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoContentVersionCultureVariation_umbracoContentVersion_id");
        }
    }
}
