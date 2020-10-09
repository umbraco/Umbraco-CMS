﻿using System;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(Property))]
    public sealed class PropertyMapper : BaseMapper
    {
        public PropertyMapper(Lazy<ISqlContext> sqlContext)
            : base(sqlContext)
        { }

        protected override void DefineMaps()
        {
            DefineMap<Property, PropertyDataDto>(nameof(Property.Id), nameof(PropertyDataDto.Id));
            DefineMap<Property, PropertyDataDto>(nameof(Property.PropertyTypeId), nameof(PropertyDataDto.PropertyTypeId));
        }
    }
}
