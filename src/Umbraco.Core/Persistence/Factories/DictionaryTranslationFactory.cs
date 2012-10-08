using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class DictionaryTranslationFactory : IEntityFactory<DictionaryTranslation, LanguageTextDto>
    {
        #region Implementation of IEntityFactory<DictionaryTranslation,LanguageTextDto>

        public DictionaryTranslation BuildEntity(LanguageTextDto dto)
        {
            throw new System.NotImplementedException();
        }

        public LanguageTextDto BuildDto(DictionaryTranslation entity)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}