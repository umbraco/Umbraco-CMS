using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoContentVersionConfiguration : IEntityTypeConfiguration<UmbracoContentVersion>
    {
        public void Configure(EntityTypeBuilder<UmbracoContentVersion> builder)
        {
            builder.ToTable("umbracoContentVersion");

            builder.HasIndex(e => e.Current, "IX_umbracoContentVersion_Current");

            builder.HasIndex(e => new { e.NodeId, e.Current }, "IX_umbracoContentVersion_NodeId");

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Current).HasColumnName("current");
            builder.Property(e => e.NodeId).HasColumnName("nodeId");
            builder.Property(e => e.PreventCleanup)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("preventCleanup");
            builder.Property(e => e.Text)
                .HasMaxLength(255)
                .HasColumnName("text");
            builder.Property(e => e.UserId).HasColumnName("userId");
            builder.Property(e => e.VersionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("versionDate");

            builder.HasOne(d => d.Node).WithMany(p => p.UmbracoContentVersions)
                .HasForeignKey(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(d => d.User).WithMany(p => p.UmbracoContentVersions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_umbracoContentVersion_umbracoUser_id");
        }
    }
}
