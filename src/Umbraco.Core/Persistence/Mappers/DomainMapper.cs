using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(IDomain))]
    [MapperFor(typeof(UmbracoDomain))]
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
            CacheMap<UmbracoDomain, DomainDto>(src => src.Id, dto => dto.Id);
            CacheMap<UmbracoDomain, DomainDto>(src => src.RootContentId, dto => dto.RootStructureId);
            CacheMap<UmbracoDomain, DomainDto>(src => src.LanguageId, dto => dto.DefaultLanguage);
            CacheMap<UmbracoDomain, DomainDto>(src => src.DomainName, dto => dto.DomainName);
        }
    }
}