using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Provides mapping configuration between the <c>KeyValue</c> entity and its corresponding database schema.
/// Used by the persistence layer to translate entity properties to database columns for CRUD operations.
/// </summary>
[MapperFor(typeof(KeyValue))]
[MapperFor(typeof(IKeyValue))]
public sealed class KeyValueMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">A <see cref="Lazy{ISqlContext}"/> providing access to the SQL context for database operations.</param>
    /// <param name="maps">The <see cref="MapperConfigurationStore"/> containing mapper configurations.</param>
    public KeyValueMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<KeyValue, KeyValueDto>(nameof(KeyValue.Identifier), nameof(KeyValueDto.Key));
        DefineMap<KeyValue, KeyValueDto>(nameof(KeyValue.Value), nameof(KeyValueDto.Value));
        DefineMap<KeyValue, KeyValueDto>(nameof(KeyValue.UpdateDate), nameof(KeyValueDto.UpdateDate));
    }
}
