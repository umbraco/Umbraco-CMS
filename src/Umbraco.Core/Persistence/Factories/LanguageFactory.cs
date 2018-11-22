using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class LanguageFactory 
    {
        public ILanguage BuildEntity(LanguageDto dto)
        {
            var lang = new Language(dto.IsoCode) { CultureName = dto.CultureName, Id = dto.Id };
            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            lang.ResetDirtyProperties(false);
            return lang;
        }

        public LanguageDto BuildDto(ILanguage entity)
        {
            var dto = new LanguageDto { CultureName = entity.CultureName, IsoCode = entity.IsoCode };
            if (entity.HasIdentity)
                dto.Id = short.Parse(entity.Id.ToString(CultureInfo.InvariantCulture));

            return dto;
        }
    }
}