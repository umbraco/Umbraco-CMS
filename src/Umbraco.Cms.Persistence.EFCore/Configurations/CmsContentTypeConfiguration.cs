using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsContentTypeConfiguration : IEntityTypeConfiguration<CmsContentType>
    {
        public void Configure(EntityTypeBuilder<CmsContentType> builder)
        {
            builder.HasKey(e => e.Pk);
            builder.ToTable("cmsContentType");

            builder.HasIndex(e => e.NodeId, "IX_cmsContentType").IsUnique();

            builder.HasIndex(e => e.Icon, "IX_cmsContentType_icon");

            builder.Property(e => e.Pk).HasColumnName("pk");
            builder.Property(e => e.Alias)
                .HasMaxLength(255)
                .HasColumnName("alias");
            builder.Property(e => e.AllowAtRoot)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("allowAtRoot");
            builder.Property(e => e.Description)
                .HasMaxLength(1500)
                .HasColumnName("description");
            builder.Property(e => e.Icon)
                .HasMaxLength(255)
                .HasColumnName("icon");
            builder.Property(e => e.IsContainer)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("isContainer");
            builder.Property(e => e.IsElement)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("isElement");
            builder.Property(e => e.NodeId).HasColumnName("nodeId");
            builder.Property(e => e.Thumbnail)
                .HasMaxLength(255)
                .HasDefaultValueSql("('folder.png')")
                .HasColumnName("thumbnail");
            builder.Property(e => e.Variations)
                .HasDefaultValueSql("('1')")
                .HasColumnName("variations");

            builder.HasOne(d => d.Node).WithOne(p => p.CmsContentType)
                .HasForeignKey<CmsContentType>(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsContentType_umbracoNode_id");
        }
    }
}
