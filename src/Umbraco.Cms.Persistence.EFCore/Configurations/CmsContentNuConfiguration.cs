using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsContentNuConfiguration : IEntityTypeConfiguration<CmsContentNu>
    {
        public void Configure(EntityTypeBuilder<CmsContentNu> builder)
        {
            builder.HasKey(e => new { e.NodeId, e.Published });
            builder.ToTable("cmsContentNu");

            builder.HasIndex(e => new { e.Published, e.NodeId, e.Rv }, "IX_cmsContentNu_published");

            builder.Property(e => e.NodeId).HasColumnName("nodeId");
            builder.Property(e => e.Published).HasColumnName("published");
            builder.Property(e => e.Data).HasColumnName("data");
            builder.Property(e => e.DataRaw).HasColumnName("dataRaw");
            builder.Property(e => e.Rv).HasColumnName("rv");

            builder.HasOne(d => d.Node).WithMany(p => p.CmsContentNus).HasForeignKey(d => d.NodeId);
        }
    }
}
