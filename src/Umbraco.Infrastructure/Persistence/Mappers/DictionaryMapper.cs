using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="DictionaryItem" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(DictionaryItem))]
[MapperFor(typeof(IDictionaryItem))]
public sealed class DictionaryMapper : BaseMapper
{
    public DictionaryMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<DictionaryItem, DictionaryDto>(nameof(DictionaryItem.Id), nameof(DictionaryDto.PrimaryKey));
        DefineMap<DictionaryItem, DictionaryDto>(nameof(DictionaryItem.Key), nameof(DictionaryDto.UniqueId));
        DefineMap<DictionaryItem, DictionaryDto>(nameof(DictionaryItem.ItemKey), nameof(DictionaryDto.Key));
        DefineMap<DictionaryItem, DictionaryDto>(nameof(DictionaryItem.ParentId), nameof(DictionaryDto.Parent));
    }
}
