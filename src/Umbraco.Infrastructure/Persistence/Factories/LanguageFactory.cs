using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories
{
    internal static class LanguageFactory
    {
        public static ILanguage BuildEntity(LanguageDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            if (dto.IsoCode == null || dto.CultureName == null)
            {
                throw new InvalidOperationException("Language ISO code and/or culture name can't be null.");
            }

            var lang = new Language(dto.IsoCode, dto.CultureName)
            {
                Id = dto.Id,
                IsDefault = dto.IsDefault,
                IsMandatory = dto.IsMandatory,
                FallbackLanguageId = dto.FallbackLanguageId
            };

            // Reset dirty initial properties
            lang.ResetDirtyProperties(false);

            return lang;
        }

        public static LanguageDto BuildDto(ILanguage entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var dto = new LanguageDto
            {
                IsoCode = entity.IsoCode,
                CultureName = entity.CultureName,
                IsDefault = entity.IsDefault,
                IsMandatory = entity.IsMandatory,
                FallbackLanguageId = entity.FallbackLanguageId
            };

            if (entity.HasIdentity)
            {
                dto.Id = (short)entity.Id;
            }

            return dto;
        }
    }
}
