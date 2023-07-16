using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoContentVersionCleanupPolicyConfiguration : IEntityTypeConfiguration<UmbracoContentVersionCleanupPolicy>
    {
        public void Configure(EntityTypeBuilder<UmbracoContentVersionCleanupPolicy> builder)
        {
            builder.HasKey(e => e.ContentTypeId);

            builder.ToTable("umbracoContentVersionCleanupPolicy");

            builder.Property(e => e.ContentTypeId)
                .ValueGeneratedNever()
                .HasColumnName("contentTypeId");
            builder.Property(e => e.KeepAllVersionsNewerThanDays).HasColumnName("keepAllVersionsNewerThanDays");
            builder.Property(e => e.KeepLatestVersionPerDayForDays).HasColumnName("keepLatestVersionPerDayForDays");
            builder.Property(e => e.PreventCleanup).HasColumnName("preventCleanup");
            builder.Property(e => e.Updated)
                .HasColumnType("datetime")
                .HasColumnName("updated");

            builder.HasOne(d => d.ContentType).WithOne(p => p.UmbracoContentVersionCleanupPolicy)
                .HasPrincipalKey<CmsContentType>(p => p.NodeId)
                .HasForeignKey<UmbracoContentVersionCleanupPolicy>(d => d.ContentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoContentVersionCleanupPolicy_cmsContentType_nodeId");
        }
    }
}
