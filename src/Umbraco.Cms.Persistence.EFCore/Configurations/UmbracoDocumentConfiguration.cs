using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoDocumentConfiguration : IEntityTypeConfiguration<UmbracoDocument>
    {
        public void Configure(EntityTypeBuilder<UmbracoDocument> builder)
        {
            builder.HasKey(e => e.NodeId);
            builder.ToTable("umbracoDocument");

            builder.HasIndex(e => e.Published, "IX_umbracoDocument_Published");

            builder.Property(e => e.NodeId)
                .ValueGeneratedNever()
                .HasColumnName("nodeId");
            builder.Property(e => e.Edited).HasColumnName("edited");
            builder.Property(e => e.Published).HasColumnName("published");

            builder.HasOne(d => d.Node).WithOne(p => p.UmbracoDocument)
                .HasForeignKey<UmbracoDocument>(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
