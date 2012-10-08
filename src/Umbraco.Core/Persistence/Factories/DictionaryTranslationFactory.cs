using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class DictionaryTranslationFactory : IEntityFactory<DictionaryTranslation, LanguageTextDto>
    {
        private readonly Guid _uniqueId;
        private Language _language;

        public DictionaryTranslationFactory(Guid uniqueId, Language language)
        {
            _uniqueId = uniqueId;
            _language = language;
        }

        #region Implementation of IEntityFactory<DictionaryTranslation,LanguageTextDto>

        public DictionaryTranslation BuildEntity(LanguageTextDto dto)
        {
            return new DictionaryTranslation(_language, dto.Value, _uniqueId) 
                                            {Id = dto.PrimaryKey};
        }

        public LanguageTextDto BuildDto(DictionaryTranslation entity)
        {
            var text = new LanguageTextDto
                           {
                               LanguageId = entity.Language.Id,
                               UniqueId = entity.Key,
                               Value = entity.Value
                           };

            if (entity.HasIdentity)
                text.PrimaryKey = entity.Id;

            return text;
        }

        #endregion
    }
}