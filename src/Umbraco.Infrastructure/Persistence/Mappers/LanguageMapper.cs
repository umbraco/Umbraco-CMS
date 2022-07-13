using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="Language" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(ILanguage))]
[MapperFor(typeof(Language))]
public sealed class LanguageMapper : BaseMapper
{
    public LanguageMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<Language, LanguageDto>(nameof(Language.Id), nameof(LanguageDto.Id));
        DefineMap<Language, LanguageDto>(nameof(Language.IsoCode), nameof(LanguageDto.IsoCode));
        DefineMap<Language, LanguageDto>(nameof(Language.CultureName), nameof(LanguageDto.CultureName));
    }
}
