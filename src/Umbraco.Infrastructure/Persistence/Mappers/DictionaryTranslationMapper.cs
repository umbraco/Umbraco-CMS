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
            nameof(DictionaryTranslation.Language),
            nameof(LanguageTextDto.LanguageId));
        DefineMap<DictionaryTranslation, LanguageTextDto>(
            nameof(DictionaryTranslation.Value),
            nameof(LanguageTextDto.Value));
    }
}
