using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(AuditItem))]
    public sealed class AuditItemMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            CacheMap<AuditItem, LogDto>(src => src.Id, dto => dto.Id);
            CacheMap<AuditItem, LogDto>(src => src.Comment, dto => dto.Comment);
            CacheMap<AuditItem, LogDto>(src => src.AuditType, dto => dto.Header);
            CacheMap<AuditItem, LogDto>(src => src.UserId, dto => dto.UserId);
            CacheMap<AuditItem, LogDto>(src => src.CreateDate, dto => dto.Datestamp);
        }        
    }
}