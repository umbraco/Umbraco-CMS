using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class DomainFactory
{
    public static IDomain BuildEntity(DomainDto dto)
    {
        var domain = new UmbracoDomain(dto.DomainName, dto.IsoCode)
        {
            Id = dto.Id,
            LanguageId = dto.DefaultLanguage,
            RootContentId = dto.RootStructureId,
            SortOrder = dto.SortOrder,
        };

        // Reset dirty initial properties (U4-1946)
        domain.ResetDirtyProperties(false);

        return domain;
    }

    public static DomainDto BuildDto(IDomain entity)
    {
        var dto = new DomainDto
        {
            Id = entity.Id,
            DefaultLanguage = entity.LanguageId,
            RootStructureId = entity.RootContentId,
            DomainName = entity.DomainName,
            SortOrder = entity.SortOrder,
        };

        return dto;
    }
}
