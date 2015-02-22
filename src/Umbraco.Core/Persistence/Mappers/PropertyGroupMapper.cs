using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

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


        #region Overrides of BaseMapper

        public PropertyGroupMapper(ISqlSyntaxProvider sqlSyntax) : base(sqlSyntax)
        {
        }

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        protected override void BuildMap()
        {
            CacheMap<PropertyGroup, PropertyTypeGroupDto>(src => src.Id, dto => dto.Id);
            CacheMap<PropertyGroup, PropertyTypeGroupDto>(src => src.ParentId, dto => dto.ParentGroupId);
            CacheMap<PropertyGroup, PropertyTypeGroupDto>(src => src.SortOrder, dto => dto.SortOrder);
            CacheMap<PropertyGroup, PropertyTypeGroupDto>(src => src.Name, dto => dto.Text);
        }

        #endregion
    }
}