﻿using System.Collections.Generic;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    
    internal class MacroFactory 
    {
        public IMacro BuildEntity(MacroDto dto)
        {
            var model = new Macro(dto.Id, dto.UniqueId, dto.UseInEditor, dto.RefreshRate, dto.Alias, dto.Name, dto.ScriptType, dto.ScriptAssembly, dto.Xslt, dto.CacheByPage, dto.CachePersonalized, dto.DontRender, dto.Python);

            try
            {
                model.DisableChangeTracking();

                foreach (var p in dto.MacroPropertyDtos)
                {
                    model.Properties.Add(new MacroProperty(p.Id, p.UniqueId, p.Alias, p.Name, p.SortOrder, p.EditorAlias));
                }

                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                model.ResetDirtyProperties(false);
                return model;
            }
            finally
            {
                model.EnableChangeTracking();
            }
        }

        public MacroDto BuildDto(IMacro entity)
        {
            var dto = new MacroDto
            {
                    UniqueId = entity.Key,
                    Alias = entity.Alias,
                    CacheByPage = entity.CacheByPage,
                    CachePersonalized = entity.CacheByMember,
                    DontRender = entity.DontRender,
                    Name = entity.Name,
                    Python = entity.ScriptPath,
                    RefreshRate = entity.CacheDuration,
                    ScriptAssembly = entity.ControlAssembly,
                    ScriptType = entity.ControlType,
                    UseInEditor = entity.UseInEditor,
                    Xslt = entity.XsltPath,
                    MacroPropertyDtos = BuildPropertyDtos(entity)
                };

            if (entity.HasIdentity)
                dto.Id = int.Parse(entity.Id.ToString(CultureInfo.InvariantCulture));

            return dto;
        }

        private List<MacroPropertyDto> BuildPropertyDtos(IMacro entity)
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
