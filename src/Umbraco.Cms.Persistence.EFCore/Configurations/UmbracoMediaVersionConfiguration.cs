using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoMediaVersionConfiguration : IEntityTypeConfiguration<UmbracoMediaVersion>
    {
        public void Configure(EntityTypeBuilder<UmbracoMediaVersion> builder)
        {
            builder.ToTable("umbracoMediaVersion");

            builder.HasIndex(e => new { e.Id, e.Path }, "IX_umbracoMediaVersion").IsUnique();

            builder.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            builder.Property(e => e.Path)
                .HasMaxLength(255)
                .HasColumnName("path");

            builder.HasOne(d => d.IdNavigation).WithOne(p => p.UmbracoMediaVersion)
                .HasForeignKey<UmbracoMediaVersion>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
