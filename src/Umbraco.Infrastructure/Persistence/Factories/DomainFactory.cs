using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class DomainFactory
{
    public static IDomain BuildEntity(DomainDto dto, string? isoCode)
    {
        var domain = new UmbracoDomain(dto.DomainName, isoCode ?? string.Empty)
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

    public static DomainDto BuildDto(IDomain entity) =>
        new()
        {
            Id = entity.Id,
            Key = entity.Key,
            DefaultLanguage = entity.LanguageId,
            RootStructureId = entity.RootContentId,
            DomainName = entity.DomainName,
            SortOrder = entity.SortOrder,
        };
}
