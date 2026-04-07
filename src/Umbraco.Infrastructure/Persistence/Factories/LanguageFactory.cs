using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class LanguageFactory
{
    /// <summary>
    /// Creates an <see cref="ILanguage"/> entity from the specified <see cref="LanguageDto"/>,
    /// optionally assigning a fallback ISO code if provided.
    /// If the <c>CultureName</c> property of the DTO is null, it is set to the English name of the culture corresponding to the ISO code.
    /// </summary>
    /// <param name="dto">The data transfer object containing language information. Must not be null and must have a non-null ISO code.</param>
    /// <param name="fallbackIsoCode">An optional ISO code to use as the fallback language for the created entity.</param>
    /// <returns>An <see cref="ILanguage"/> entity constructed from the provided DTO and fallback ISO code.</returns>
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
            IsDefault = dto.IsDefault,
            IsMandatory = dto.IsMandatory,
            FallbackIsoCode = fallbackIsoCode
        };

        // Reset dirty initial properties
        lang.ResetDirtyProperties(false);

        return lang;
    }

    /// <summary>
    /// Creates a <see cref="LanguageDto"/> instance from the specified <see cref="ILanguage"/> entity, optionally assigning a fallback language ID.
    /// </summary>
    /// <param name="entity">The <see cref="ILanguage"/> entity to convert. Must not be <c>null</c>.</param>
    /// <param name="fallbackLanguageId">An optional fallback language ID to assign to the DTO, or <c>null</c> if not applicable.</param>
    /// <returns>A <see cref="LanguageDto"/> that represents the provided language entity and fallback language ID.</returns>
    public static LanguageDto BuildDto(ILanguage entity, int? fallbackLanguageId)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new LanguageDto
        {
            IsoCode = entity.IsoCode,
            CultureName = entity.CultureName,
            IsDefault = entity.IsDefault,
            IsMandatory = entity.IsMandatory,
            FallbackLanguageId = fallbackLanguageId
        };

        if (entity.HasIdentity)
        {
            dto.Id = (short)entity.Id;
        }

        return dto;
    }
}
