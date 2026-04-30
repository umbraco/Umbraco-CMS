using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class AuditEntryFactory
{
    /// <summary>
    /// Builds a collection of <see cref="IAuditEntry"/> entities from the given collection of <see cref="AuditEntryDto"/> data transfer objects.
    /// </summary>
    /// <param name="dtos">The collection of <see cref="AuditEntryDto"/> objects to convert.</param>
    /// <returns>A collection of <see cref="IAuditEntry"/> entities.</returns>
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

    /// <summary>
    /// Builds an <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.AuditEntryDto"/> from the given <see cref="Umbraco.Cms.Core.Models.IAuditEntry"/> entity.
    /// </summary>
    /// <param name="entity">The audit entry entity to convert to a DTO.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.AuditEntryDto"/> representing the audit entry.</returns>
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
