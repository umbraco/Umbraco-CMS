using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(AuditItem))]
    [MapperFor(typeof(IAuditItem))]
    public sealed class AuditMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        public AuditMapper()
        {
            BuildMap();
        }

        protected override void BuildMap()
        {
            if (PropertyInfoCache.IsEmpty)
            {
                CacheMap<AuditItem, LogDto>(src => src.Id, dto => dto.NodeId);
                CacheMap<AuditItem, LogDto>(src => src.CreateDate, dto => dto.Datestamp);
                CacheMap<AuditItem, LogDto>(src => src.UserId, dto => dto.UserId);
                CacheMap<AuditItem, LogDto>(src => src.AuditType, dto => dto.Header);
                CacheMap<AuditItem, LogDto>(src => src.Comment, dto => dto.Comment);
            }
        }
    }
}
