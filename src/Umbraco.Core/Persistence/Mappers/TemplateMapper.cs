using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="Template"/> to DTO mapper used to translate the properties of the public api
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(Template))]
    [MapperFor(typeof(ITemplate))]
    public sealed class TemplateMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            if (PropertyInfoCache.IsEmpty == false) return;

            CacheMap<Template, TemplateDto>(src => src.Id, dto => dto.NodeId);
            CacheMap<Template, NodeDto>(src => src.MasterTemplateId, dto => dto.ParentId);
            CacheMap<Template, NodeDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<Template, TemplateDto>(src => src.Alias, dto => dto.Alias);
        }
    }
}
