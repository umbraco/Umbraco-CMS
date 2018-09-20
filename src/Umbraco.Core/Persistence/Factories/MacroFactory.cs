using System.Collections.Generic;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class MacroFactory
    {
        public static IMacro BuildEntity(MacroDto dto)
        {
            var model = new Macro(dto.Id, dto.UniqueId, dto.UseInEditor, dto.RefreshRate, dto.Alias, dto.Name, dto.CacheByPage, dto.CachePersonalized, dto.DontRender, dto.MacroSource, (MacroTypes)dto.MacroType);

            try
            {
                model.DisableChangeTracking();

                foreach (var p in dto.MacroPropertyDtos.EmptyNull())
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
                    MacroType = (int)entity.MacroType
                };

            if (entity.HasIdentity)
                dto.Id = int.Parse(entity.Id.ToString(CultureInfo.InvariantCulture));

            return dto;
        }

        private static List<MacroPropertyDto> BuildPropertyDtos(IMacro entity)
        {
            var list = new List<MacroPropertyDto>();
            foreach (var p in entity.Properties)
            {
                var text = new MacroPropertyDto
                {
                    UniqueId = p.Key,
                    Alias = p.Alias,
                    Name = p.Name,
                    Macro = entity.Id,
                    SortOrder = (byte)p.SortOrder,
                    EditorAlias = p.EditorAlias,
                    Id = p.Id
                };

                list.Add(text);
            }
            return list;
        }
    }
}
