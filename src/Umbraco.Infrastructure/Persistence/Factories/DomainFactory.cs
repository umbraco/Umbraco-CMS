using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class DomainFactory
{
    /// <summary>
    /// Creates an <see cref="Umbraco.Cms.Core.Models.IDomain"/> entity from the specified <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.DomainDto"/>.
    /// </summary>
    /// <param name="dto">The <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.DomainDto"/> containing the domain data to map.</param>
    /// <param name="isoCode">The ISO code for the language.</param>
    /// <param name="rootContentKey">The key (Guid) of the root content node, resolved from the umbracoNode table.</param>
    /// <returns>An <see cref="Umbraco.Cms.Core.Models.IDomain"/> instance representing the mapped domain.</returns>
    public static IDomain BuildEntity(DomainDto dto, string? isoCode, Guid? rootContentKey = null)
    {
        var domain = new UmbracoDomain(dto.DomainName, isoCode ?? string.Empty)
        {
            Id = dto.Id,
            Key = dto.Key,
            LanguageId = dto.DefaultLanguage,
            RootContentId = dto.RootStructureId,
            RootContentKey = rootContentKey,
            SortOrder = dto.SortOrder,
        };

        // Reset dirty initial properties (U4-1946)
        domain.ResetDirtyProperties(false);

        return domain;
    }

    /// <summary>
    /// Creates a collection of <see cref="IDomain"/> entities from the specified DTOs,
    /// using pre-loaded lookup dictionaries for language ISO codes and content node keys.
    /// </summary>
    /// <param name="dtos">The domain DTOs to map.</param>
    /// <param name="isoCodeLookup">A dictionary mapping language IDs to their ISO codes.</param>
    /// <param name="nodeKeyLookup">A dictionary mapping node IDs to their unique keys (Guids).</param>
    /// <returns>A collection of <see cref="IDomain"/> entities.</returns>
    public static IEnumerable<IDomain> BuildEntities(
        List<DomainDto> dtos,
        Dictionary<int, string> isoCodeLookup,
        Dictionary<int, Guid> nodeKeyLookup)
    {
        return dtos.Select(dto =>
        {
            string? isoCode = dto.DefaultLanguage.HasValue
                && isoCodeLookup.TryGetValue(dto.DefaultLanguage.Value, out var code)
                ? code
                : null;

            Guid? rootContentKey = dto.RootStructureId.HasValue
                && nodeKeyLookup.TryGetValue(dto.RootStructureId.Value, out Guid key)
                ? key
                : null;

            return BuildEntity(dto, isoCode, rootContentKey);
        });
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
