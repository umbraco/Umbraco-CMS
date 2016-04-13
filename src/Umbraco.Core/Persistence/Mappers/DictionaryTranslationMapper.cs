using System.Collections.Concurrent;
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
    public sealed class DictionaryTranslationMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            CacheMap<DictionaryTranslation, LanguageTextDto>(src => src.Id, dto => dto.PrimaryKey);
            CacheMap<DictionaryTranslation, LanguageTextDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<DictionaryTranslation, LanguageTextDto>(src => src.Language, dto => dto.LanguageId);
            CacheMap<DictionaryTranslation, LanguageTextDto>(src => src.Value, dto => dto.Value);
        }
    }
}