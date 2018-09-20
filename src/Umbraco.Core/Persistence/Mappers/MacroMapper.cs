﻿using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(Macro))]
    [MapperFor(typeof(IMacro))]
    internal sealed class MacroMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            CacheMap<Macro, MacroDto>(src => src.Id, dto => dto.Id);
            CacheMap<Macro, MacroDto>(src => src.Alias, dto => dto.Alias);
            CacheMap<Macro, MacroDto>(src => src.CacheByPage, dto => dto.CacheByPage);
            CacheMap<Macro, MacroDto>(src => src.CacheByMember, dto => dto.CachePersonalized);
            CacheMap<Macro, MacroDto>(src => src.MacroType, dto => dto.MacroType);
            CacheMap<Macro, MacroDto>(src => src.DontRender, dto => dto.DontRender);
            CacheMap<Macro, MacroDto>(src => src.Name, dto => dto.Name);
            CacheMap<Macro, MacroDto>(src => src.CacheDuration, dto => dto.RefreshRate);
            CacheMap<Macro, MacroDto>(src => src.MacroSource, dto => dto.MacroSource);
            CacheMap<Macro, MacroDto>(src => src.UseInEditor, dto => dto.UseInEditor);
        }
    }
}
