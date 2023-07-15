using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoNodeConfiguration : IEntityTypeConfiguration<UmbracoNode>
    {
        public void Configure(EntityTypeBuilder<UmbracoNode> builder)
        {
            builder.ToTable("umbracoNode");

            builder.HasIndex(e => new { e.Level, e.ParentId, e.SortOrder, e.NodeObjectType, e.Trashed }, "IX_umbracoNode_Level");

            builder.HasIndex(e => new { e.NodeObjectType, e.Trashed }, "IX_umbracoNode_ObjectType");

            builder.HasIndex(e => new { e.NodeObjectType, e.Trashed, e.SortOrder, e.Id }, "IX_umbracoNode_ObjectType_trashed_sorted");

            builder.HasIndex(e => e.Path, "IX_umbracoNode_Path");

            builder.HasIndex(e => e.Trashed, "IX_umbracoNode_Trashed");

            builder.HasIndex(e => e.UniqueId, "IX_umbracoNode_UniqueId").IsUnique();

            builder.HasIndex(e => new { e.ParentId, e.NodeObjectType }, "IX_umbracoNode_parentId_nodeObjectType");

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            builder.Property(e => e.Level).HasColumnName("level");
            builder.Property(e => e.NodeObjectType).HasColumnName("nodeObjectType");
            builder.Property(e => e.NodeUser).HasColumnName("nodeUser");
            builder.Property(e => e.ParentId).HasColumnName("parentId");
            builder.Property(e => e.Path)
                .HasMaxLength(150)
                .HasColumnName("path");
            builder.Property(e => e.SortOrder).HasColumnName("sortOrder");
            builder.Property(e => e.Text)
                .HasMaxLength(255)
                .HasColumnName("text");
            builder.Property(e => e.Trashed)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("trashed");
            builder.Property(e => e.UniqueId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("uniqueId");

            builder.HasOne(d => d.NodeUserNavigation).WithMany(p => p.UmbracoNodes)
                .HasForeignKey(d => d.NodeUser)
                .HasConstraintName("FK_umbracoNode_umbracoUser_id");

            builder.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoNode_umbracoNode_id");

            builder.HasMany(d => d.ChildContentTypes).WithMany(p => p.ParentContentTypes)
                .UsingEntity<Dictionary<string, object>>(
                    "CmsContentType2ContentType",
                    r => r.HasOne<UmbracoNode>().WithMany()
                        .HasForeignKey("ChildContentTypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_cmsContentType2ContentType_umbracoNode_child"),
                    l => l.HasOne<UmbracoNode>().WithMany()
                        .HasForeignKey("ParentContentTypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_cmsContentType2ContentType_umbracoNode_parent"),
                    j =>
                    {
                        j.HasKey("ParentContentTypeId", "ChildContentTypeId");
                        j.ToTable("cmsContentType2ContentType");
                        j.IndexerProperty<int>("ParentContentTypeId").HasColumnName("parentContentTypeId");
                        j.IndexerProperty<int>("ChildContentTypeId").HasColumnName("childContentTypeId");
                    });

            builder.HasMany(d => d.ParentContentTypes).WithMany(p => p.ChildContentTypes)
                .UsingEntity<Dictionary<string, object>>(
                    "CmsContentType2ContentType",
                    r => r.HasOne<UmbracoNode>().WithMany()
                        .HasForeignKey("ParentContentTypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_cmsContentType2ContentType_umbracoNode_parent"),
                    l => l.HasOne<UmbracoNode>().WithMany()
                        .HasForeignKey("ChildContentTypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_cmsContentType2ContentType_umbracoNode_child"),
                    j =>
                    {
                        j.HasKey("ParentContentTypeId", "ChildContentTypeId");
                        j.ToTable("cmsContentType2ContentType");
                        j.IndexerProperty<int>("ParentContentTypeId").HasColumnName("parentContentTypeId");
                        j.IndexerProperty<int>("ChildContentTypeId").HasColumnName("childContentTypeId");
                    });
        }
    }
}
