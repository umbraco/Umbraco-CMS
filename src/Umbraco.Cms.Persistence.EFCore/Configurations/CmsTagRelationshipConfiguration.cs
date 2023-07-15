using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsTagRelationshipConfiguration : IEntityTypeConfiguration<CmsTagRelationship>
    {
        public void Configure(EntityTypeBuilder<CmsTagRelationship> builder)
        {
            builder.HasKey(e => new { e.NodeId, e.PropertyTypeId, e.TagId });

            builder.ToTable("cmsTagRelationship");

            builder.HasIndex(e => new { e.TagId, e.NodeId }, "IX_cmsTagRelationship_tagId_nodeId");

            builder.Property(e => e.NodeId).HasColumnName("nodeId");
            builder.Property(e => e.PropertyTypeId).HasColumnName("propertyTypeId");
            builder.Property(e => e.TagId).HasColumnName("tagId");

            builder.HasOne(d => d.Node).WithMany(p => p.CmsTagRelationships)
                .HasForeignKey(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsTagRelationship_cmsContent");

            builder.HasOne(d => d.PropertyType).WithMany(p => p.CmsTagRelationships)
                .HasForeignKey(d => d.PropertyTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsTagRelationship_cmsPropertyType");

            builder.HasOne(d => d.Tag).WithMany(p => p.CmsTagRelationships)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsTagRelationship_cmsTags_id");
        }
    }
}
