using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="Tag" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(Tag))]
[MapperFor(typeof(ITag))]
public sealed class TagMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TagMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">A lazily-initialized <see cref="ISqlContext"/> used for SQL operations.</param>
    /// <param name="maps">The <see cref="MapperConfigurationStore"/> containing mapping configurations.</param>
    public TagMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<Tag, TagDto>(nameof(Tag.Id), nameof(TagDto.Id));
        DefineMap<Tag, TagDto>(nameof(Tag.Text), nameof(TagDto.Text));
        DefineMap<Tag, TagDto>(nameof(Tag.Group), nameof(TagDto.Group));
        DefineMap<Tag, TagDto>(nameof(Tag.LanguageId), nameof(TagDto.LanguageId));
    }
}
