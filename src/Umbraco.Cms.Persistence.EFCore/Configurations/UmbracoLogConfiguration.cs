using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoLogConfiguration : IEntityTypeConfiguration<UmbracoLog>
    {
        public void Configure(EntityTypeBuilder<UmbracoLog> builder)
        {
            builder.ToTable("umbracoLog");

            builder.HasIndex(e => e.NodeId, "IX_umbracoLog");

            builder.HasIndex(e => new { e.Datestamp, e.UserId, e.NodeId }, "IX_umbracoLog_datestamp");

            builder.HasIndex(e => new { e.Datestamp, e.LogHeader }, "IX_umbracoLog_datestamp_logheader");

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Datestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            builder.Property(e => e.EntityType)
                .HasMaxLength(50)
                .HasColumnName("entityType");
            builder.Property(e => e.LogComment)
                .HasMaxLength(4000)
                .HasColumnName("logComment");
            builder.Property(e => e.LogHeader)
                .HasMaxLength(50)
                .HasColumnName("logHeader");
            builder.Property(e => e.Parameters)
                .HasMaxLength(500)
                .HasColumnName("parameters");
            builder.Property(e => e.UserId).HasColumnName("userId");

            builder.HasOne(d => d.User).WithMany(p => p.UmbracoLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_umbracoLog_umbracoUser_id");
        }
    }
}
