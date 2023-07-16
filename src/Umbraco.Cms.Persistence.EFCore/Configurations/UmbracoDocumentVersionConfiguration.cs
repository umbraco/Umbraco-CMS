using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoDocumentVersionConfiguration : IEntityTypeConfiguration<UmbracoDocumentVersion>
    {
        public void Configure(EntityTypeBuilder<UmbracoDocumentVersion> builder)
        {
            builder.ToTable("umbracoDocumentVersion");

            builder.HasIndex(e => new { e.Id, e.Published }, "IX_umbracoDocumentVersion_id_published");

            builder.HasIndex(e => e.Published, "IX_umbracoDocumentVersion_published");

            builder.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            builder.Property(e => e.Published).HasColumnName("published");
            builder.Property(e => e.TemplateId).HasColumnName("templateId");

            builder.HasOne(d => d.IdNavigation).WithOne(p => p.UmbracoDocumentVersion)
                .HasForeignKey<UmbracoDocumentVersion>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(d => d.Template).WithMany(p => p.UmbracoDocumentVersions)
                .HasPrincipalKey(p => p.NodeId)
                .HasForeignKey(d => d.TemplateId)
                .HasConstraintName("FK_umbracoDocumentVersion_cmsTemplate_nodeId");
        }
    }
}
