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
    [MapperFor(typeof(DictionaryTranslation))]
    [MapperFor(typeof(IDictionaryTranslation))]
    public class DictionaryTranslationMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public DictionaryTranslationMapper()
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
            CacheMap<DictionaryTranslation, LanguageTextDto>(src => src.Id, dto => dto.PrimaryKey);
            CacheMap<DictionaryTranslation, LanguageTextDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<DictionaryTranslation, LanguageTextDto>(src => src.Language, dto => dto.LanguageId);
            CacheMap<DictionaryTranslation, LanguageTextDto>(src => src.Value, dto => dto.Value);
        }

        #endregion
    }
}