using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

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
        public DictionaryMapper(ISqlContext sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        {
            DefineMap<DictionaryItem, DictionaryDto>(nameof(DictionaryItem.Id), nameof(DictionaryDto.PrimaryKey));
            DefineMap<DictionaryItem, DictionaryDto>(nameof(DictionaryItem.Key), nameof(DictionaryDto.UniqueId));
            DefineMap<DictionaryItem, DictionaryDto>(nameof(DictionaryItem.ItemKey), nameof(DictionaryDto.Key));
            DefineMap<DictionaryItem, DictionaryDto>(nameof(DictionaryItem.ParentId), nameof(DictionaryDto.Parent));
        }
    }
}
