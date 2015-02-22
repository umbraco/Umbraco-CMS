using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(Property))]
    public sealed class PropertyMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        public PropertyMapper(ISqlSyntaxProvider sqlSyntax) : base(sqlSyntax)
        {
        }

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        protected override void BuildMap()
        {
            CacheMap<Property, PropertyDataDto>(src => src.Id, dto => dto.Id);
            CacheMap<Property, PropertyDataDto>(src => src.Version, dto => dto.VersionId);
            CacheMap<Property, PropertyDataDto>(src => src.PropertyTypeId, dto => dto.PropertyTypeId);
        }

        
    }
}