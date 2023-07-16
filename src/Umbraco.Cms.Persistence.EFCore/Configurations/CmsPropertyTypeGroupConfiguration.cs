using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsPropertyTypeGroupConfiguration : IEntityTypeConfiguration<CmsPropertyTypeGroup>
    {
        public void Configure(EntityTypeBuilder<CmsPropertyTypeGroup> builder)
        {
            builder.ToTable("cmsPropertyTypeGroup");

            builder.HasIndex(e => e.UniqueId, "IX_cmsPropertyTypeGroupUniqueID").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Alias)
                .HasMaxLength(255)
                .HasColumnName("alias");
            builder.Property(e => e.ContenttypeNodeId).HasColumnName("contenttypeNodeId");
            builder.Property(e => e.Sortorder).HasColumnName("sortorder");
            builder.Property(e => e.Text)
                .HasMaxLength(255)
                .HasColumnName("text");
            builder.Property(e => e.Type)
                .HasDefaultValueSql("('0')")
                .HasColumnName("type");
            builder.Property(e => e.UniqueId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("uniqueID");

            builder.HasOne(d => d.ContenttypeNode).WithMany(p => p.CmsPropertyTypeGroups)
                .HasPrincipalKey(p => p.NodeId)
                .HasForeignKey(d => d.ContenttypeNodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsPropertyTypeGroup_cmsContentType_nodeId");
        }
    }
}
