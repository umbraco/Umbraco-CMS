using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a mapper for audit entry entities.
/// </summary>
[MapperFor(typeof(IAuditEntry))]
[MapperFor(typeof(AuditEntry))]
public sealed class AuditEntryMapper : BaseMapper
{
    public AuditEntryMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.Id), nameof(AuditEntryDto.Id));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.PerformingUserId), nameof(AuditEntryDto.PerformingUserId));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.PerformingDetails), nameof(AuditEntryDto.PerformingDetails));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.PerformingIp), nameof(AuditEntryDto.PerformingIp));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.EventDateUtc), nameof(AuditEntryDto.EventDateUtc));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.AffectedUserId), nameof(AuditEntryDto.AffectedUserId));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.AffectedDetails), nameof(AuditEntryDto.AffectedDetails));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.EventType), nameof(AuditEntryDto.EventType));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.EventDetails), nameof(AuditEntryDto.EventDetails));
    }
}
