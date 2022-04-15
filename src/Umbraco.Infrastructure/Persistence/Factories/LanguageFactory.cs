using System.Globalization;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories
{
    internal static class LanguageFactory
    {
        public static ILanguage BuildEntity(GlobalSettings globalSettings, LanguageDto dto)
        {
            var lang = new Language(globalSettings, dto.IsoCode)
            {
                CultureName = dto.CultureName,
                Id = dto.Id,
                IsDefault = dto.IsDefault,
                IsMandatory = dto.IsMandatory,
                FallbackLanguageId = dto.FallbackLanguageId
            };

            // reset dirty initial properties (U4-1946)
            lang.ResetDirtyProperties(false);
            return lang;
        }

        public static LanguageDto BuildDto(ILanguage entity)
        {
            var dto = new LanguageDto
            {
                CultureName = entity.CultureName,
                IsoCode = entity.IsoCode,
                IsDefault = entity.IsDefault,
                IsMandatory = entity.IsMandatory,
                FallbackLanguageId = entity.FallbackLanguageId
            };

            if (entity.HasIdentity)
            {
                dto.Id = short.Parse(entity.Id.ToString(CultureInfo.InvariantCulture));
            }

            return dto;
        }
    }
}
