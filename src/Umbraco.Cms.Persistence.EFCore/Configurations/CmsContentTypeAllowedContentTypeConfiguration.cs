using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsContentTypeAllowedContentTypeConfiguration : IEntityTypeConfiguration<CmsContentTypeAllowedContentType>
    {
        public void Configure(EntityTypeBuilder<CmsContentTypeAllowedContentType> builder)
        {
            builder.HasKey(e => new { e.Id, e.AllowedId });

            builder.ToTable("cmsContentTypeAllowedContentType");

            builder.Property(e => e.SortOrder).HasDefaultValueSql("('0')");

            builder.HasOne(d => d.Allowed).WithMany(p => p.CmsContentTypeAllowedContentTypeAlloweds)
                .HasPrincipalKey(p => p.NodeId)
                .HasForeignKey(d => d.AllowedId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsContentTypeAllowedContentType_cmsContentType1");

            builder.HasOne(d => d.IdNavigation).WithMany(p => p.CmsContentTypeAllowedContentTypeIdNavigations)
                .HasPrincipalKey(p => p.NodeId)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsContentTypeAllowedContentType_cmsContentType");
        }
    }
}
