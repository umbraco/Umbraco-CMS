using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="Language"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    internal sealed class LanguageMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public LanguageMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override void BuildMap()
        {
            CacheMap<Language, LanguageDto>(src => src.Id, dto => dto.Id);
            CacheMap<Language, LanguageDto>(src => src.IsoCode, dto => dto.IsoCode);
            CacheMap<Language, LanguageDto>(src => src.CultureName, dto => dto.CultureName);
        }

        internal override string Map(string propertyName)
        {
            if (!PropertyInfoCache.ContainsKey(propertyName))
                return string.Empty;

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