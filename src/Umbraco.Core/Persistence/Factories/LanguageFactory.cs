using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class LanguageFactory : IEntityFactory<Language, LanguageDto>
    {
        #region Implementation of IEntityFactory<Language,LanguageDto>

        public Language BuildEntity(LanguageDto dto)
        {
            throw new System.NotImplementedException();
        }

        public LanguageDto BuildDto(Language entity)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}