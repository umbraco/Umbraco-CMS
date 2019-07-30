﻿using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="PropertyGroup"/> to DTO mapper used to translate the properties of the public api
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(PropertyGroup))]
    public sealed class PropertyGroupMapper : BaseMapper
    {
        public PropertyGroupMapper(Lazy<ISqlContext> sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<PropertyGroup, PropertyTypeGroupDto>(nameof(PropertyGroup.Id), nameof(PropertyTypeGroupDto.Id));
            DefineMap<PropertyGroup, PropertyTypeGroupDto>(nameof(PropertyGroup.Key), nameof(PropertyTypeGroupDto.UniqueId));
            DefineMap<PropertyGroup, PropertyTypeGroupDto>(nameof(PropertyGroup.SortOrder), nameof(PropertyTypeGroupDto.SortOrder));
            DefineMap<PropertyGroup, PropertyTypeGroupDto>(nameof(PropertyGroup.Name), nameof(PropertyTypeGroupDto.Text));
        }
    }
}
