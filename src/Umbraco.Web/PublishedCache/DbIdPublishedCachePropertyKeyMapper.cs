using ClientDependency.Core;
using Microsoft.Owin.Security.Provider;
using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Scoping;
using static Umbraco.Core.Persistence.NPocoSqlExtensions.Statics;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Maps property keys using the property id, falls back to property name if no map
    /// </summary>
    public class DbIdPublishedCachePropertyKeyMapper : IPublishedCachePropertyKeyMapper
    {
        private readonly IScopeProvider _scopeProvider;
        private IDictionary<string, string> _propertyToNucacheKey;
        private IDictionary<string, string> _nucacheKeyToProperty;

        public DbIdPublishedCachePropertyKeyMapper(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
            BuildMap();//Move
        }

        /// <summary>
        /// Builds the map
        /// </summary>
        /// <remarks>Call to rebuild map when properties are added / removed. May need to rebuild nucache to have this work. Multiple properties may be using the same alias</remarks>
        /// 
        public virtual void BuildMap()
        {
            using(var scope = _scopeProvider.CreateScope())
            {
                var sql = scope.SqlContext.Sql()
                    .Select<PropertyTypeDto>(x => x.Id, x => x.Alias)
                    .From<PropertyTypeDto>();

                var propertyTypes = scope.Database.Fetch<PropertyTypeDto>(sql).ToArray();
                _nucacheKeyToProperty = propertyTypes.ToDictionary((x) => x.Id.ToString(), (x) => x.Alias);
                 _propertyToNucacheKey = propertyTypes.DistinctBy(x => x.Alias).ToDictionary(x => x.Alias, x => x.Id.ToString()); // Consider storing the serialization alias in the PropertyTypeDto instead of using a distinct id
                scope.Complete();
            }
        }
        /// <inheritdoc/>
        public string ToCacheAlias(Property property)
        {
            return _propertyToNucacheKey.ContainsKey(property.Alias) ? _propertyToNucacheKey.GetValue(property.Alias) : property.Alias;
        }
        /// <inheritdoc/>
        public string ToCacheAlias(IPublishedPropertyType property)
        {
            return _propertyToNucacheKey.ContainsKey(property.Alias) ? _propertyToNucacheKey.GetValue(property.Alias) : property.Alias;
        }

        /// <inheritdoc/>
        public string ToPropertyAlias(string cachePropertyKey)
        {
            return _nucacheKeyToProperty.ContainsKey(cachePropertyKey) ? _nucacheKeyToProperty.GetValue(cachePropertyKey) : cachePropertyKey;
        }
    }
}
