using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoAccessConfiguration : IEntityTypeConfiguration<UmbracoAccess>
    {
        public void Configure(EntityTypeBuilder<UmbracoAccess> builder)
        {
            builder.ToTable("umbracoAccess");

            builder.HasIndex(e => e.NodeId, "IX_umbracoAccess_nodeId").IsUnique();

            builder.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            builder.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            builder.Property(e => e.LoginNodeId).HasColumnName("loginNodeId");
            builder.Property(e => e.NoAccessNodeId).HasColumnName("noAccessNodeId");
            builder.Property(e => e.NodeId).HasColumnName("nodeId");
            builder.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
            .HasColumnName("updateDate");

            builder.HasOne(d => d.LoginNode).WithMany(p => p.UmbracoAccessLoginNodes)
                .HasForeignKey(d => d.LoginNodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoAccess_umbracoNode_id1");

            builder.HasOne(d => d.NoAccessNode).WithMany(p => p.UmbracoAccessNoAccessNodes)
                .HasForeignKey(d => d.NoAccessNodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoAccess_umbracoNode_id2");

            builder.HasOne(d => d.Node).WithOne(p => p.UmbracoAccessNode)
                .HasForeignKey<UmbracoAccess>(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoAccess_umbracoNode_id");
        }
    }
}
