using System.Collections.Concurrent;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(DomainRepository.CacheableDomain))]
    public sealed class DomainMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        public DomainMapper()
        {
            BuildMap();
        }

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        internal override void BuildMap()
        {
            CacheMap<DomainRepository.CacheableDomain, DomainDto>(src => src.Id, dto => dto.Id);
            CacheMap<DomainRepository.CacheableDomain, DomainDto>(src => src.RootContentId, dto => dto.RootStructureId);
            CacheMap<DomainRepository.CacheableDomain, DomainDto>(src => src.DefaultLanguageId, dto => dto.DefaultLanguage);
        }
    }
}