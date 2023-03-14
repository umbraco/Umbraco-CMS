using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class MacroFactory
{
    public static IMacro BuildEntity(IShortStringHelper shortStringHelper, MacroDto dto)
    {
        var model = new Macro(shortStringHelper, dto.Id, dto.UniqueId, dto.UseInEditor, dto.RefreshRate, dto.Alias,
            dto.Name, dto.CacheByPage, dto.CachePersonalized, dto.DontRender, dto.MacroSource);

        try
        {
            model.DisableChangeTracking();

            foreach (MacroPropertyDto p in dto.MacroPropertyDtos.EmptyNull())
            {
                model.Properties.Add(new MacroProperty(p.Id, p.UniqueId, p.Alias, p.Name, p.SortOrder, p.EditorAlias));
            }

            // reset dirty initial properties (U4-1946)
            model.ResetDirtyProperties(false);
            return model;
        }
        finally
        {
            model.EnableChangeTracking();
        }
    }

    public static MacroDto BuildDto(IMacro entity)
    {
        var dto = new MacroDto
        {
            UniqueId = entity.Key,
            Alias = entity.Alias,
            CacheByPage = entity.CacheByPage,
            CachePersonalized = entity.CacheByMember,
            DontRender = entity.DontRender,
            Name = entity.Name,
            MacroSource = entity.MacroSource,
            RefreshRate = entity.CacheDuration,
            UseInEditor = entity.UseInEditor,
            MacroPropertyDtos = BuildPropertyDtos(entity),
            MacroType = 7, // PartialView
        };

        if (entity.HasIdentity)
        {
            dto.Id = entity.Id;
        }

        return dto;
    }

    private static List<MacroPropertyDto> BuildPropertyDtos(IMacro entity)
    {
        var list = new List<MacroPropertyDto>();
        foreach (IMacroProperty p in entity.Properties)
        {
            var text = new MacroPropertyDto
            {
                UniqueId = p.Key,
                Alias = p.Alias,
                Name = p.Name,
                Macro = entity.Id,
                SortOrder = (byte)p.SortOrder,
                EditorAlias = p.EditorAlias,
                Id = p.Id,
            };

            list.Add(text);
        }

        return list;
    }
}
