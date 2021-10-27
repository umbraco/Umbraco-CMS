using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="PropertyType"/> to DTO mapper used to translate the properties of the public api
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(PropertyType))]
    public sealed class PropertyTypeMapper : BaseMapper
    {
        public PropertyTypeMapper(Lazy<ISqlContext> sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<PropertyType, PropertyTypeDto>(nameof(PropertyType.Key), nameof(PropertyTypeDto.UniqueId));
            DefineMap<PropertyType, PropertyTypeDto>(nameof(PropertyType.Id), nameof(PropertyTypeDto.Id));
            DefineMap<PropertyType, PropertyTypeDto>(nameof(PropertyType.Alias), nameof(PropertyTypeDto.Alias));
            DefineMap<PropertyType, PropertyTypeDto>(nameof(PropertyType.DataTypeId), nameof(PropertyTypeDto.DataTypeId));
            DefineMap<PropertyType, PropertyTypeDto>(nameof(PropertyType.Description), nameof(PropertyTypeDto.Description));
            DefineMap<PropertyType, PropertyTypeDto>(nameof(PropertyType.Mandatory), nameof(PropertyTypeDto.Mandatory));
            DefineMap<PropertyType, PropertyTypeDto>(nameof(PropertyType.MandatoryMessage), nameof(PropertyTypeDto.MandatoryMessage));
            DefineMap<PropertyType, PropertyTypeDto>(nameof(PropertyType.Name), nameof(PropertyTypeDto.Name));
            DefineMap<PropertyType, PropertyTypeDto>(nameof(PropertyType.SortOrder), nameof(PropertyTypeDto.SortOrder));
            DefineMap<PropertyType, PropertyTypeDto>(nameof(PropertyType.ValidationRegExp), nameof(PropertyTypeDto.ValidationRegExp));
            DefineMap<PropertyType, PropertyTypeDto>(nameof(PropertyType.ValidationRegExpMessage), nameof(PropertyTypeDto.ValidationRegExpMessage));
            DefineMap<PropertyType, PropertyTypeDto>(nameof(PropertyType.LabelOnTop), nameof(PropertyTypeDto.LabelOnTop));
            DefineMap<PropertyType, DataTypeDto>(nameof(PropertyType.PropertyEditorAlias), nameof(DataTypeDto.EditorAlias));
            DefineMap<PropertyType, DataTypeDto>(nameof(PropertyType.ValueStorageType), nameof(DataTypeDto.DbType));
        }
    }
}
