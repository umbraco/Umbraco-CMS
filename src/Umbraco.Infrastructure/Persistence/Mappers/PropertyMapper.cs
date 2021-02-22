using System;
using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Infrastructure.Persistence.Mappers;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(Property))]
    public sealed class PropertyMapper : BaseMapper
    {
        public PropertyMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<Property, PropertyDataDto>(nameof(Property.Id), nameof(PropertyDataDto.Id));
            DefineMap<Property, PropertyDataDto>(nameof(Property.PropertyTypeId), nameof(PropertyDataDto.PropertyTypeId));
        }
    }
}
