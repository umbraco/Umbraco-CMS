using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class ContentVersionCleanupPolicyDtoConfiguration : IEntityTypeConfiguration<ContentVersionCleanupPolicyDto>
{
    public void Configure(EntityTypeBuilder<ContentVersionCleanupPolicyDto> builder)
    {
        builder.ToTable(ContentVersionCleanupPolicyDto.TableName);

        builder.HasKey(x => x.ContentTypeId)
            .HasName($"PK_{ContentVersionCleanupPolicyDto.TableName}");

        builder.Property(x => x.ContentTypeId)
            .HasColumnName(ContentVersionCleanupPolicyDto.ContentTypeIdColumnName)
            .ValueGeneratedNever();

        builder.Property(x => x.PreventCleanup)
            .HasColumnName(ContentVersionCleanupPolicyDto.PreventCleanupColumnName);

        builder.Property(x => x.KeepAllVersionsNewerThanDays)
            .HasColumnName(ContentVersionCleanupPolicyDto.KeepAllVersionsNewerThanDaysColumnName);

        builder.Property(x => x.KeepLatestVersionPerDayForDays)
            .HasColumnName(ContentVersionCleanupPolicyDto.KeepLatestVersionPerDayForDaysColumnName);

        builder.Property(x => x.Updated)
            .HasColumnName(ContentVersionCleanupPolicyDto.UpdatedColumnName);

        // The FK to cmsContentType.NodeId is enforced at the database level by NPoco's schema.
        // No EF Core navigation is declared here because ContentTypeDto is not yet part of the EF Core model
        // on this branch — to be added once ContentTypeDto lands.
    }
}
