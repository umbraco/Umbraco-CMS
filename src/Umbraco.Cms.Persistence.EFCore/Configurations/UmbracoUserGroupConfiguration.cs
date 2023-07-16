using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoUserGroupConfiguration : IEntityTypeConfiguration<UmbracoUserGroup>
    {
        public void Configure(EntityTypeBuilder<UmbracoUserGroup> builder)
        {
            builder.ToTable("umbracoUserGroup");

            builder.HasIndex(e => e.UserGroupAlias, "IX_umbracoUserGroup_userGroupAlias").IsUnique();

            builder.HasIndex(e => e.UserGroupName, "IX_umbracoUserGroup_userGroupName").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            builder.Property(e => e.HasAccessToAllLanguages).HasColumnName("hasAccessToAllLanguages");
            builder.Property(e => e.Icon)
                .HasMaxLength(255)
                .HasColumnName("icon");
            builder.Property(e => e.StartContentId).HasColumnName("startContentId");
            builder.Property(e => e.StartMediaId).HasColumnName("startMediaId");
            builder.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updateDate");
            builder.Property(e => e.UserGroupAlias)
                .HasMaxLength(200)
                .HasColumnName("userGroupAlias");
            builder.Property(e => e.UserGroupDefaultPermissions)
                .HasMaxLength(50)
                .HasColumnName("userGroupDefaultPermissions");
            builder.Property(e => e.UserGroupName)
                .HasMaxLength(200)
                .HasColumnName("userGroupName");

            builder.HasOne(d => d.StartContent).WithMany(p => p.UmbracoUserGroupStartContents)
                .HasForeignKey(d => d.StartContentId)
                .HasConstraintName("FK_startContentId_umbracoNode_id");

            builder.HasOne(d => d.StartMedia).WithMany(p => p.UmbracoUserGroupStartMedia)
                .HasForeignKey(d => d.StartMediaId)
                .HasConstraintName("FK_startMediaId_umbracoNode_id");

            builder.HasMany(d => d.Languages).WithMany(p => p.UserGroups)
                .UsingEntity<Dictionary<string, object>>(
                    "UmbracoUserGroup2Language",
                    r => r.HasOne<UmbracoLanguage>().WithMany()
                        .HasForeignKey("LanguageId")
                        .HasConstraintName("FK_umbracoUserGroup2Language_umbracoLanguage_id"),
                    l => l.HasOne<UmbracoUserGroup>().WithMany()
                        .HasForeignKey("UserGroupId")
                        .HasConstraintName("FK_umbracoUserGroup2Language_umbracoUserGroup_id"),
                    j =>
                    {
                        j.HasKey("UserGroupId", "LanguageId").HasName("PK_userGroup2language");
                        j.ToTable("umbracoUserGroup2Language");
                        j.IndexerProperty<int>("UserGroupId").HasColumnName("userGroupId");
                        j.IndexerProperty<int>("LanguageId").HasColumnName("languageId");
                    });

            builder.HasMany(d => d.Nodes).WithMany(p => p.UserGroups)
                .UsingEntity<Dictionary<string, object>>(
                    "UmbracoUserGroup2Node",
                    r => r.HasOne<UmbracoNode>().WithMany()
                        .HasForeignKey("NodeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_umbracoUserGroup2Node_umbracoNode_id"),
                    l => l.HasOne<UmbracoUserGroup>().WithMany()
                        .HasForeignKey("UserGroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_umbracoUserGroup2Node_umbracoUserGroup_id"),
                    j =>
                    {
                        j.HasKey("UserGroupId", "NodeId");
                        j.ToTable("umbracoUserGroup2Node");
                        j.HasIndex(new[] { "NodeId" }, "IX_umbracoUserGroup2Node_nodeId");
                        j.IndexerProperty<int>("UserGroupId").HasColumnName("userGroupId");
                        j.IndexerProperty<int>("NodeId").HasColumnName("nodeId");
                    });
        }
    }
}
