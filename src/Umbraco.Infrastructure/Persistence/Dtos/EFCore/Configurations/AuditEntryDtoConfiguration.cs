using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class AuditEntryDtoConfiguration : IEntityTypeConfiguration<AuditEntryDto>
{
    public void Configure(EntityTypeBuilder<AuditEntryDto> builder)
    {
        builder.ToTable(AuditEntryDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(AuditEntryDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.PerformingUserId)
            .HasColumnName(AuditEntryDto.PerformingUserIdColumnName);

        builder.Property(x => x.PerformingUserKey)
            .HasColumnName(AuditEntryDto.PerformingUserKeyColumnName);

        builder.Property(x => x.PerformingDetails)
            .HasColumnName(AuditEntryDto.PerformingDetailsColumnName)
            .HasMaxLength(Constants.Audit.DetailsLength);

        builder.Property(x => x.PerformingIp)
            .HasColumnName(AuditEntryDto.PerformingIpColumnName)
            .HasMaxLength(Constants.Audit.IpLength);

        builder.Property(x => x.EventDate)
            .HasColumnName(AuditEntryDto.EventDateColumnName);

        builder.Property(x => x.AffectedUserId)
            .HasColumnName(AuditEntryDto.AffectedUserIdColumnName);

        builder.Property(x => x.AffectedUserKey)
            .HasColumnName(AuditEntryDto.AffectedUserKeyColumnName);

        builder.Property(x => x.AffectedDetails)
            .HasColumnName(AuditEntryDto.AffectedDetailsColumnName)
            .HasMaxLength(Constants.Audit.DetailsLength);

        builder.Property(x => x.EventType)
            .HasColumnName(AuditEntryDto.EventTypeColumnName)
            .HasMaxLength(Constants.Audit.EventTypeLength);

        builder.Property(x => x.EventDetails)
            .HasColumnName(AuditEntryDto.EventDetailsColumnName)
            .HasMaxLength(Constants.Audit.DetailsLength);
    }
}
