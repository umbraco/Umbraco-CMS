using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class AuditEntryFactory
{
    public static IEnumerable<IAuditEntry> BuildEntities(IEnumerable<AuditEntryDto> dtos) =>
        dtos.Select(BuildEntity).ToList();

    public static IAuditEntry BuildEntity(AuditEntryDto dto)
    {
        var entity = new AuditEntry
        {
            Id = dto.Id,
            PerformingUserId = dto.PerformingUserId,
            PerformingUserKey = dto.PerformingUserKey,
            PerformingDetails = dto.PerformingDetails,
            PerformingIp = dto.PerformingIp,
            EventDate = dto.EventDate.EnsureUtc(),
            AffectedUserId = dto.AffectedUserId,
            AffectedUserKey = dto.AffectedUserKey,
            AffectedDetails = dto.AffectedDetails,
            EventType = dto.EventType,
            EventDetails = dto.EventDetails,
        };

        // on initial construction we don't want to have dirty properties tracked
        // http://issues.umbraco.org/issue/U4-1946
        entity.ResetDirtyProperties(false);
        return entity;
    }

    public static AuditEntryDto BuildDto(IAuditEntry entity) =>
        new AuditEntryDto
        {
            Id = entity.Id,
            PerformingUserId = entity.PerformingUserId,
            PerformingUserKey = entity.PerformingUserKey,
            PerformingDetails = entity.PerformingDetails,
            PerformingIp = entity.PerformingIp,
            EventDate = entity.EventDate,
            AffectedUserId = entity.AffectedUserId,
            AffectedUserKey = entity.AffectedUserKey,
            AffectedDetails = entity.AffectedDetails,
            EventType = entity.EventType,
            EventDetails = entity.EventDetails,
        };
}
