using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsDocumentTypeConfiguration : IEntityTypeConfiguration<CmsDocumentType>
    {
        public void Configure(EntityTypeBuilder<CmsDocumentType> builder)
        {
            builder.HasKey(e => new { e.ContentTypeNodeId, e.TemplateNodeId });
            builder.ToTable("cmsDocumentType");

            builder.Property(e => e.ContentTypeNodeId).HasColumnName("contentTypeNodeId");
            builder.Property(e => e.TemplateNodeId).HasColumnName("templateNodeId");
            builder.Property(e => e.IsDefault)
                .IsRequired()
                .HasDefaultValueSql("('0')");

            builder.HasOne(d => d.ContentTypeNode).WithMany(p => p.CmsDocumentTypes)
                .HasForeignKey(d => d.ContentTypeNodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsDocumentType_umbracoNode_id");

            builder.HasOne(d => d.ContentTypeNodeNavigation).WithMany(p => p.CmsDocumentTypes)
                .HasPrincipalKey(p => p.NodeId)
                .HasForeignKey(d => d.ContentTypeNodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsDocumentType_cmsContentType_nodeId");

            builder.HasOne(d => d.TemplateNode).WithMany(p => p.CmsDocumentTypes)
                .HasPrincipalKey(p => p.NodeId)
                .HasForeignKey(d => d.TemplateNodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsDocumentType_cmsTemplate_nodeId");
        }
    }
}
