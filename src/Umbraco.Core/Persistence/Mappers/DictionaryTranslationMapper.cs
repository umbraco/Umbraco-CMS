using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="DictionaryTranslation"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    public class DictionaryTranslationMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache = new ConcurrentDictionary<string, DtoMapModel>();

        internal static DictionaryTranslationMapper Instance = new DictionaryTranslationMapper();

        private DictionaryTranslationMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override void BuildMap()
        {
            CacheMap<DictionaryTranslation, LanguageTextDto>(src => src.Id, dto => dto.PrimaryKey);
            CacheMap<DictionaryTranslation, LanguageTextDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<DictionaryTranslation, LanguageTextDto>(src => src.Language, dto => dto.LanguageId);
            CacheMap<DictionaryTranslation, LanguageTextDto>(src => src.Value, dto => dto.Value);
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