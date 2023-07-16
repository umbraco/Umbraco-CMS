using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoServerConfiguration : IEntityTypeConfiguration<UmbracoServer>
    {
        public void Configure(EntityTypeBuilder<UmbracoServer> builder)
        {
            builder.ToTable("umbracoServer");

            builder.HasIndex(e => e.ComputerName, "IX_computerName").IsUnique();

            builder.HasIndex(e => e.IsActive, "IX_umbracoServer_isActive");

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
            builder.Property(e => e.ComputerName)
                .HasMaxLength(255)
                .HasColumnName("computerName");
            builder.Property(e => e.IsActive).HasColumnName("isActive");
            builder.Property(e => e.IsSchedulingPublisher).HasColumnName("isSchedulingPublisher");
            builder.Property(e => e.LastNotifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("lastNotifiedDate");
            builder.Property(e => e.RegisteredDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("registeredDate");
        }
    }
}
