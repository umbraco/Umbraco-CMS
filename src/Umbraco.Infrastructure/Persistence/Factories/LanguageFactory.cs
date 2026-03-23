using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class LanguageFactory
{
    public static ILanguage BuildEntity(LanguageDto dto, string? fallbackIsoCode)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.IsoCode is null)
        {
            throw new InvalidOperationException("Language ISO code can't be null.");
        }

        dto.CultureName ??= CultureInfo.GetCultureInfo(dto.IsoCode).EnglishName;

        var lang = new Language(dto.IsoCode, dto.CultureName)
        {
            Id = dto.Id,
            Key = dto.LanguageKey,
            IsDefault = dto.IsDefault,
            IsMandatory = dto.IsMandatory,
            FallbackIsoCode = fallbackIsoCode
        };

        // Reset dirty initial properties
        lang.ResetDirtyProperties(false);

        return lang;
    }

    public static LanguageDto BuildDto(ILanguage entity, int? fallbackLanguageId)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new LanguageDto
        {
            LanguageKey = entity.Key,
            IsoCode = entity.IsoCode,
            CultureName = entity.CultureName,
            IsDefault = entity.IsDefault,
            IsMandatory = entity.IsMandatory,
            FallbackLanguageId = fallbackLanguageId
        };

        if (entity.HasIdentity)
        {
            dto.Id = entity.Id;
        }

        return dto;
    }
}
