using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

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

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public PropertyTypeMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        internal override void BuildMap()
        {
            if(PropertyInfoCache.IsEmpty)
            {
                CacheMap<PropertyType, PropertyTypeDto>(src => src.Id, dto => dto.Id);
                CacheMap<PropertyType, PropertyTypeDto>(src => src.Alias, dto => dto.Alias);
                CacheMap<PropertyType, PropertyTypeDto>(src => src.DataTypeDefinitionId, dto => dto.DataTypeId);
                CacheMap<PropertyType, PropertyTypeDto>(src => src.Description, dto => dto.Description);
                CacheMap<PropertyType, PropertyTypeDto>(src => src.Mandatory, dto => dto.Mandatory);
                CacheMap<PropertyType, PropertyTypeDto>(src => src.Name, dto => dto.Name);
                CacheMap<PropertyType, PropertyTypeDto>(src => src.SortOrder, dto => dto.SortOrder);
                CacheMap<PropertyType, PropertyTypeDto>(src => src.ValidationRegExp, dto => dto.ValidationRegExp);
                CacheMap<PropertyType, DataTypeDto>(src => src.PropertyEditorAlias, dto => dto.PropertyEditorAlias);
                CacheMap<PropertyType, DataTypeDto>(src => src.DataTypeDatabaseType, dto => dto.DbType);
            }
        }

        #endregion
    }
}