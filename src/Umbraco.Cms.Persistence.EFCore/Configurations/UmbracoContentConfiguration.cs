using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoContentConfiguration : IEntityTypeConfiguration<UmbracoContent>
    {
        public void Configure(EntityTypeBuilder<UmbracoContent> builder)
        {
            builder.HasKey(e => e.NodeId);
            builder.ToTable("umbracoContent");

            builder.Property(e => e.NodeId)
                .ValueGeneratedNever()
                .HasColumnName("nodeId");
            builder.Property(e => e.ContentTypeId).HasColumnName("contentTypeId");

            builder.HasOne(d => d.ContentType).WithMany(p => p.UmbracoContents)
                .HasPrincipalKey(p => p.NodeId)
                .HasForeignKey(d => d.ContentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoContent_cmsContentType_NodeId");

            builder.HasOne(d => d.Node).WithOne(p => p.UmbracoContent)
                .HasForeignKey<UmbracoContent>(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoContent_umbracoNode_id");
        }
    }
}
