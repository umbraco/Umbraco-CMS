using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="DictionaryItem"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(DictionaryItem))]
    [MapperFor(typeof(IDictionaryItem))]
    public sealed class DictionaryMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            CacheMap<DictionaryItem, DictionaryDto>(src => src.Id, dto => dto.PrimaryKey);
            CacheMap<DictionaryItem, DictionaryDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<DictionaryItem, DictionaryDto>(src => src.ItemKey, dto => dto.Key);
            CacheMap<DictionaryItem, DictionaryDto>(src => src.ParentId, dto => dto.Parent);
        }
    }
}