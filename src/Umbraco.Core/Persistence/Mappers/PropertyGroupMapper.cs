using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="PropertyGroup"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(PropertyGroup))]
    public sealed class PropertyGroupMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public PropertyGroupMapper()
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
            CacheMap<PropertyGroup, PropertyTypeGroupDto>(src => src.Id, dto => dto.Id);
            CacheMap<PropertyGroup, PropertyTypeGroupDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<PropertyGroup, PropertyTypeGroupDto>(src => src.SortOrder, dto => dto.SortOrder);
            CacheMap<PropertyGroup, PropertyTypeGroupDto>(src => src.Name, dto => dto.Text);
        }

        #endregion
    }
}