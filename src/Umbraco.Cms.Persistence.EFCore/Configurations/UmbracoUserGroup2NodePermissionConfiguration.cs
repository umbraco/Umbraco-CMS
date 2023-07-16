using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoUserGroup2NodePermissionConfiguration : IEntityTypeConfiguration<UmbracoUserGroup2NodePermission>
    {
        public void Configure(EntityTypeBuilder<UmbracoUserGroup2NodePermission> builder)
        {
            builder.HasKey(e => new { e.UserGroupId, e.NodeId, e.Permission });

            builder.ToTable("umbracoUserGroup2NodePermission");

            builder.HasIndex(e => e.NodeId, "IX_umbracoUser2NodePermission_nodeId");

            builder.Property(e => e.UserGroupId).HasColumnName("userGroupId");
            builder.Property(e => e.NodeId).HasColumnName("nodeId");
            builder.Property(e => e.Permission)
                .HasMaxLength(255)
                .HasColumnName("permission");

            builder.HasOne(d => d.Node).WithMany(p => p.UmbracoUserGroup2NodePermissions)
                .HasForeignKey(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoUserGroup2NodePermission_umbracoNode_id");

            builder.HasOne(d => d.UserGroup).WithMany(p => p.UmbracoUserGroup2NodePermissions)
                .HasForeignKey(d => d.UserGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoUserGroup2NodePermission_umbracoUserGroup_id");
        }
    }
}
