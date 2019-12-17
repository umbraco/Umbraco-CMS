using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class AuditEntryFactory
    {
        public static IEnumerable<IAuditEntry> BuildEntities(IEnumerable<AuditEntryDto> dtos)
        {
            return dtos.Select(BuildEntity).ToList();
        }

        public static IAuditEntry BuildEntity(AuditEntryDto dto)
        {
            var entity = new AuditEntry
            {
                Id = dto.Id,
                PerformingUserId = dto.PerformingUserId,
                PerformingDetails = dto.PerformingDetails,
                PerformingIp = dto.PerformingIp,
                EventDateUtc = dto.EventDateUtc,
                AffectedUserId = dto.AffectedUserId,
                AffectedDetails = dto.AffectedDetails,
                EventType = dto.EventType,
                EventDetails = dto.EventDetails
            };

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            entity.ResetDirtyProperties(false);
            return entity;
        }

        public static AuditEntryDto BuildDto(IAuditEntry entity)
        {
            return new AuditEntryDto
            {
                Id = entity.Id,
                PerformingUserId = entity.PerformingUserId,
                PerformingDetails = entity.PerformingDetails,
                PerformingIp = entity.PerformingIp,
                EventDateUtc = entity.EventDateUtc,
                AffectedUserId = entity.AffectedUserId,
                AffectedDetails = entity.AffectedDetails,
                EventType = entity.EventType,
                EventDetails = entity.EventDetails
            };
        }
    }
}
