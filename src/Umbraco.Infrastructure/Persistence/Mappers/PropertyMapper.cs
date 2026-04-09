using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Provides mapping logic between the Property entity and its corresponding database schema representation.
/// </summary>
[MapperFor(typeof(Property))]
public sealed class PropertyMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">The lazy-loaded SQL context for database operations.</param>
    /// <param name="maps">The configuration store containing mapping definitions.</param>
    public PropertyMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<Property, PropertyDataDto>(nameof(Property.Id), nameof(PropertyDataDto.Id));
        DefineMap<Property, PropertyDataDto>(nameof(Property.PropertyTypeId), nameof(PropertyDataDto.PropertyTypeId));
    }
}
