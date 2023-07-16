using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsTemplateConfiguration : IEntityTypeConfiguration<CmsTemplate>
    {
        public void Configure(EntityTypeBuilder<CmsTemplate> builder)
        {
            builder.HasKey(e => e.Pk);
            builder.ToTable("cmsTemplate");

            builder.HasIndex(e => e.NodeId, "IX_cmsTemplate_nodeId").IsUnique();

            builder.Property(e => e.Pk).HasColumnName("pk");
            builder.Property(e => e.Alias)
                .HasMaxLength(100)
                .HasColumnName("alias");
            builder.Property(e => e.NodeId).HasColumnName("nodeId");

            builder.HasOne(d => d.Node).WithOne(p => p.CmsTemplate)
                .HasForeignKey<CmsTemplate>(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsTemplate_umbracoNode");
        }
    }
}
