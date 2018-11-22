using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="Tag"/> to DTO mapper used to translate the properties of the public api
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(Tag))]
    [MapperFor(typeof(ITag))]
    public sealed class TagMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            if (PropertyInfoCache.IsEmpty == false) return;

            CacheMap<Tag, TagDto>(src => src.Id, dto => dto.Id);
            CacheMap<Tag, TagDto>(src => src.Text, dto => dto.Text);
            CacheMap<Tag, TagDto>(src => src.Group, dto => dto.Group);
        }
    }
}
