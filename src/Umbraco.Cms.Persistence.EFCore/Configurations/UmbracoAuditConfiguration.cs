using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoAuditConfiguration : IEntityTypeConfiguration<UmbracoAudit>
    {
        public void Configure(EntityTypeBuilder<UmbracoAudit> builder)
        {
            builder.ToTable("umbracoAudit");

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.AffectedDetails)
                .HasMaxLength(1024)
                .HasColumnName("affectedDetails");
            builder.Property(e => e.AffectedUserId).HasColumnName("affectedUserId");
            builder.Property(e => e.EventDateUtc)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("eventDateUtc");
            builder.Property(e => e.EventDetails)
                .HasMaxLength(1024)
                .HasColumnName("eventDetails");
            builder.Property(e => e.EventType)
                .HasMaxLength(256)
                .HasColumnName("eventType");
            builder.Property(e => e.PerformingDetails)
                .HasMaxLength(1024)
                .HasColumnName("performingDetails");
            builder.Property(e => e.PerformingIp)
                .HasMaxLength(64)
                .HasColumnName("performingIp");
            builder.Property(e => e.PerformingUserId).HasColumnName("performingUserId");
        }
    }
}
