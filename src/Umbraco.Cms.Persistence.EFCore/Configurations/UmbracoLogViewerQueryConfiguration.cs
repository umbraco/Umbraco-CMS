using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoLogViewerQueryConfiguration : IEntityTypeConfiguration<UmbracoLogViewerQuery>
    {
        public void Configure(EntityTypeBuilder<UmbracoLogViewerQuery> builder)
        {
            builder.ToTable("umbracoLogViewerQuery");

            builder.HasIndex(e => e.Name, "IX_LogViewerQuery_name").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            builder.Property(e => e.Query)
                .HasMaxLength(255)
                .HasColumnName("query");
        }
    }
}
