using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="PropertyGroup" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(PropertyGroup))]
public sealed class PropertyGroupMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyGroupMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">A lazily initialized <see cref="ISqlContext"/> used for SQL operations.</param>
    /// <param name="maps">The <see cref="MapperConfigurationStore"/> containing mapping configurations.</param>
    public PropertyGroupMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<PropertyGroup, PropertyTypeGroupDto>(nameof(PropertyGroup.Id), nameof(PropertyTypeGroupDto.Id));
        DefineMap<PropertyGroup, PropertyTypeGroupDto>(nameof(PropertyGroup.Key), nameof(PropertyTypeGroupDto.UniqueId));
        DefineMap<PropertyGroup, PropertyTypeGroupDto>(nameof(PropertyGroup.Type), nameof(PropertyTypeGroupDto.Type));
        DefineMap<PropertyGroup, PropertyTypeGroupDto>(nameof(PropertyGroup.Name), nameof(PropertyTypeGroupDto.Text));
        DefineMap<PropertyGroup, PropertyTypeGroupDto>(nameof(PropertyGroup.Alias), nameof(PropertyTypeGroupDto.Alias));
        DefineMap<PropertyGroup, PropertyTypeGroupDto>(nameof(PropertyGroup.SortOrder), nameof(PropertyTypeGroupDto.SortOrder));
    }
}
