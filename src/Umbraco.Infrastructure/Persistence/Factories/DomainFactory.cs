using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class DomainFactory
{
    /// <summary>
    /// Creates an <see cref="Umbraco.Cms.Core.Models.IDomain"/> entity from the specified <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.DomainDto"/>.
    /// </summary>
    /// <param name="dto">The <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.DomainDto"/> containing the domain data to map.</param>
    /// <returns>An <see cref="Umbraco.Cms.Core.Models.IDomain"/> instance representing the mapped domain.</returns>
    public static IDomain BuildEntity(DomainDto dto)
    {
        var domain = new UmbracoDomain(dto.DomainName, dto.IsoCode)
        {
            Id = dto.Id,
            Key = dto.Key,
            LanguageId = dto.DefaultLanguage,
            RootContentId = dto.RootStructureId,
            SortOrder = dto.SortOrder,
        };

        // Reset dirty initial properties (U4-1946)
        domain.ResetDirtyProperties(false);

        return domain;
    }

    /// <summary>
    /// Builds a <see cref="DomainDto"/> from the given <see cref="IDomain"/> entity.
    /// </summary>
    /// <param name="entity">The domain entity to convert to a DTO.</param>
    /// <returns>A <see cref="DomainDto"/> representing the provided domain entity.</returns>
    public static DomainDto BuildDto(IDomain entity)
    {
        var dto = new DomainDto
        {
            Id = entity.Id,
            Key = entity.Key,
            DefaultLanguage = entity.LanguageId,
            RootStructureId = entity.RootContentId,
            DomainName = entity.DomainName,
            SortOrder = entity.SortOrder,
        };

        return dto;
    }
}
