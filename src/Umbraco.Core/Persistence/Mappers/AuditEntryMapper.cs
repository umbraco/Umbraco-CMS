using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a mapper for audit entry entities.
    /// </summary>
    [MapperFor(typeof(IAuditEntry))]
    [MapperFor(typeof(AuditEntry))]
    public sealed class AuditEntryMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance
            = new ConcurrentDictionary<string, DtoMapModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditEntryMapper"/> class.
        /// </summary>
        public AuditEntryMapper()
        {
            // note: why the base ctor does not invoke BuildMap is a mystery to me
            BuildMap();
        }

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        internal override void BuildMap()
        {
            CacheMap<AuditEntry, AuditEntryDto>(entity => entity.Id, dto => dto.Id);
            CacheMap<AuditEntry, AuditEntryDto>(entity => entity.PerformingUserId, dto => dto.PerformingUserId);
            CacheMap<AuditEntry, AuditEntryDto>(entity => entity.PerformingDetails, dto => dto.PerformingDetails);
            CacheMap<AuditEntry, AuditEntryDto>(entity => entity.PerformingIp, dto => dto.PerformingIp);
            CacheMap<AuditEntry, AuditEntryDto>(entity => entity.EventDateUtc, dto => dto.EventDateUtc);
            CacheMap<AuditEntry, AuditEntryDto>(entity => entity.AffectedUserId, dto => dto.AffectedUserId);
            CacheMap<AuditEntry, AuditEntryDto>(entity => entity.AffectedDetails, dto => dto.AffectedDetails);
            CacheMap<AuditEntry, AuditEntryDto>(entity => entity.EventType, dto => dto.EventType);
            CacheMap<AuditEntry, AuditEntryDto>(entity => entity.EventDetails, dto => dto.EventDetails);
        }
    }
}
