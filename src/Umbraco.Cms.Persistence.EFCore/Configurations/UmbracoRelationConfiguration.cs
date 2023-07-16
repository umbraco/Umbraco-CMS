using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoRelationConfiguration : IEntityTypeConfiguration<UmbracoRelation>
    {
        public void Configure(EntityTypeBuilder<UmbracoRelation> builder)
        {
            builder.ToTable("umbracoRelation");

            builder.HasIndex(e => new { e.ParentId, e.ChildId, e.RelType }, "IX_umbracoRelation_parentChildType").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.ChildId).HasColumnName("childId");
            builder.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            builder.Property(e => e.Datetime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("datetime");
            builder.Property(e => e.ParentId).HasColumnName("parentId");
            builder.Property(e => e.RelType).HasColumnName("relType");

            builder.HasOne(d => d.Child).WithMany(p => p.UmbracoRelationChildren)
                .HasForeignKey(d => d.ChildId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoRelation_umbracoNode1");

            builder.HasOne(d => d.Parent).WithMany(p => p.UmbracoRelationParents)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoRelation_umbracoNode");

            builder.HasOne(d => d.RelTypeNavigation).WithMany(p => p.UmbracoRelations)
                .HasForeignKey(d => d.RelType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoRelation_umbracoRelationType_id");
        }
    }
}
