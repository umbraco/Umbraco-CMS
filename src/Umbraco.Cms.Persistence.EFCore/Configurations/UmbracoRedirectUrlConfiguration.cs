using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoRedirectUrlConfiguration : IEntityTypeConfiguration<UmbracoRedirectUrl>
    {
        public void Configure(EntityTypeBuilder<UmbracoRedirectUrl> builder)
        {
            builder.ToTable("umbracoRedirectUrl");

            builder.HasIndex(e => new { e.UrlHash, e.ContentKey, e.Culture, e.CreateDateUtc }, "IX_umbracoRedirectUrl").IsUnique();

            builder.HasIndex(e => e.CreateDateUtc, "IX_umbracoRedirectUrl_culture_hash");

            builder.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            builder.Property(e => e.ContentKey).HasColumnName("contentKey");
            builder.Property(e => e.CreateDateUtc)
                .HasColumnType("datetime")
                .HasColumnName("createDateUtc");
            builder.Property(e => e.Culture)
                .HasMaxLength(255)
                .HasColumnName("culture");
            builder.Property(e => e.Url)
                .HasMaxLength(255)
                .HasColumnName("url");
            builder.Property(e => e.UrlHash)
                .HasMaxLength(40)
                .HasColumnName("urlHash");

            builder.HasOne(d => d.ContentKeyNavigation).WithMany(p => p.UmbracoRedirectUrls)
                .HasPrincipalKey(p => p.UniqueId)
                .HasForeignKey(d => d.ContentKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoRedirectUrl_umbracoNode_uniqueID");
        }
    }
}
