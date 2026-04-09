using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="DictionaryTranslation" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(DictionaryTranslation))]
[MapperFor(typeof(IDictionaryTranslation))]
public sealed class DictionaryTranslationMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryTranslationMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">The lazy-loaded SQL context for database operations.</param>
    /// <param name="maps">The configuration store for mapper settings.</param>
    public DictionaryTranslationMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<DictionaryTranslation, LanguageTextDto>(
            nameof(DictionaryTranslation.Id),
            nameof(LanguageTextDto.PrimaryKey));
        DefineMap<DictionaryTranslation, LanguageTextDto>(
            nameof(DictionaryTranslation.Key),
            nameof(LanguageTextDto.UniqueId));
        DefineMap<DictionaryTranslation, LanguageTextDto>(
            nameof(DictionaryTranslation.Value),
            nameof(LanguageTextDto.Value));
    }
}
