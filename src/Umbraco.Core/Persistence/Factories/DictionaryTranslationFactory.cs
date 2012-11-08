using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class DictionaryTranslationFactory : IEntityFactory<IDictionaryTranslation, LanguageTextDto>
    {
        private readonly Guid _uniqueId;
        private ILanguage _language;

        public DictionaryTranslationFactory(Guid uniqueId, ILanguage language)
        {
            _uniqueId = uniqueId;
            _language = language;
        }

        #region Implementation of IEntityFactory<DictionaryTranslation,LanguageTextDto>

        public IDictionaryTranslation BuildEntity(LanguageTextDto dto)
        {
            return new DictionaryTranslation(_language, dto.Value, _uniqueId) 
                                            {Id = dto.PrimaryKey};
        }

        public LanguageTextDto BuildDto(IDictionaryTranslation entity)
        {
            var text = new LanguageTextDto
                           {
                               LanguageId = entity.Language.Id,
                               UniqueId = _uniqueId,
                               Value = entity.Value
                           };

            if (entity.HasIdentity)
                text.PrimaryKey = entity.Id;

            return text;
        }

        #endregion
    }
}