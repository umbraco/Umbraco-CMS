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
    internal sealed class PropertyGroupMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache = new ConcurrentDictionary<string, DtoMapModel>();

        internal static PropertyGroupMapper Instance = new PropertyGroupMapper();

        private PropertyGroupMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override void BuildMap()
        {
            CacheMap<PropertyGroup, PropertyTypeGroupDto>(src => src.Id, dto => dto.Id);
            CacheMap<PropertyGroup, PropertyTypeGroupDto>(src => src.ParentId, dto => dto.ParentGroupId);
            CacheMap<PropertyGroup, PropertyTypeGroupDto>(src => src.SortOrder, dto => dto.SortOrder);
            CacheMap<PropertyGroup, PropertyTypeGroupDto>(src => src.Name, dto => dto.Text);
        }

        internal override string Map(string propertyName)
        {
            var dtoTypeProperty = PropertyInfoCache[propertyName];

            return base.GetColumnName(dtoTypeProperty.Type, dtoTypeProperty.PropertyInfo);
        }

        internal override void CacheMap<TSource, TDestination>(Expression<Func<TSource, object>> sourceMember, Expression<Func<TDestination, object>> destinationMember)
        {
            var property = base.ResolveMapping(sourceMember, destinationMember);
            PropertyInfoCache.AddOrUpdate(property.SourcePropertyName, property, (x, y) => property);
        }

        #endregion
    }
}