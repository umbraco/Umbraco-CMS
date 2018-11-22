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
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            if (PropertyInfoCache.IsEmpty == false) return;

            CacheMap<PropertyType, PropertyTypeDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<PropertyType, PropertyTypeDto>(src => src.Id, dto => dto.Id);
            CacheMap<PropertyType, PropertyTypeDto>(src => src.Alias, dto => dto.Alias);
            CacheMap<PropertyType, PropertyTypeDto>(src => src.DataTypeId, dto => dto.DataTypeId);
            CacheMap<PropertyType, PropertyTypeDto>(src => src.Description, dto => dto.Description);
            CacheMap<PropertyType, PropertyTypeDto>(src => src.Mandatory, dto => dto.Mandatory);
            CacheMap<PropertyType, PropertyTypeDto>(src => src.Name, dto => dto.Name);
            CacheMap<PropertyType, PropertyTypeDto>(src => src.SortOrder, dto => dto.SortOrder);
            CacheMap<PropertyType, PropertyTypeDto>(src => src.ValidationRegExp, dto => dto.ValidationRegExp);
            CacheMap<PropertyType, DataTypeDto>(src => src.PropertyEditorAlias, dto => dto.EditorAlias);
            CacheMap<PropertyType, DataTypeDto>(src => src.ValueStorageType, dto => dto.DbType);
        }
    }
}
