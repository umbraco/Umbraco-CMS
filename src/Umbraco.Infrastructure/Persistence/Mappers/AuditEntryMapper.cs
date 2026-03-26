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
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditEntryMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">A lazily-initialized SQL context used for database operations.</param>
    /// <param name="maps">The configuration store containing mapping definitions.</param>
    public AuditEntryMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.Id), nameof(AuditEntryDto.Id));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.PerformingUserId), nameof(AuditEntryDto.PerformingUserId));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.PerformingUserKey), nameof(AuditEntryDto.PerformingUserKey));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.PerformingDetails), nameof(AuditEntryDto.PerformingDetails));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.PerformingIp), nameof(AuditEntryDto.PerformingIp));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.EventDate), nameof(AuditEntryDto.EventDate));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.AffectedUserId), nameof(AuditEntryDto.AffectedUserId));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.AffectedUserKey), nameof(AuditEntryDto.AffectedUserKey));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.AffectedDetails), nameof(AuditEntryDto.AffectedDetails));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.EventType), nameof(AuditEntryDto.EventType));
        DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.EventDetails), nameof(AuditEntryDto.EventDetails));
    }
}
