using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class LanguageFactory
    {
        public static ILanguage BuildEntity(LanguageDto dto)
        {
            var lang = new Language(dto.IsoCode) { CultureName = dto.CultureName, Id = dto.Id, IsDefaultVariantLanguage = dto.IsDefaultVariantLanguage, Mandatory = dto.Mandatory, FallbackLanguageId = dto.FallbackLanguageId };
            // reset dirty initial properties (U4-1946)
            lang.ResetDirtyProperties(false);
            return lang;
        }

        public static LanguageDto BuildDto(ILanguage entity)
        {
            var dto = new LanguageDto { CultureName = entity.CultureName, IsoCode = entity.IsoCode, IsDefaultVariantLanguage = entity.IsDefaultVariantLanguage, Mandatory = entity.Mandatory, FallbackLanguageId = entity.FallbackLanguageId };
            if (entity.HasIdentity)
            {
                dto.Id = short.Parse(entity.Id.ToString(CultureInfo.InvariantCulture));
            }

            return dto;
        }
    }
}
