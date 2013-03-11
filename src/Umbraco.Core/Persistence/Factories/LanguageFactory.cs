using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class LanguageFactory : IEntityFactory<ILanguage, LanguageDto>
    {
        #region Implementation of IEntityFactory<Language,LanguageDto>

        public ILanguage BuildEntity(LanguageDto dto)
        {
            return new Language(dto.IsoCode){CultureName = dto.CultureName, Id = dto.Id};
        }

        public LanguageDto BuildDto(ILanguage entity)
        {
            var dto = new LanguageDto{ CultureName = entity.CultureName, IsoCode = entity.IsoCode};
            if (entity.HasIdentity)
                dto.Id = short.Parse(entity.Id.ToString(CultureInfo.InvariantCulture));

            return dto;
        }

        #endregion
    }
}