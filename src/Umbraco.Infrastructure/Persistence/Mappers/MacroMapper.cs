using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

[MapperFor(typeof(Macro))]
[MapperFor(typeof(IMacro))]
internal sealed class MacroMapper : BaseMapper
{
    public MacroMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<Macro, MacroDto>(nameof(Macro.Id), nameof(MacroDto.Id));
        DefineMap<Macro, MacroDto>(nameof(Macro.Alias), nameof(MacroDto.Alias));
        DefineMap<Macro, MacroDto>(nameof(Macro.CacheByPage), nameof(MacroDto.CacheByPage));
        DefineMap<Macro, MacroDto>(nameof(Macro.CacheByMember), nameof(MacroDto.CachePersonalized));
        DefineMap<Macro, MacroDto>(nameof(Macro.DontRender), nameof(MacroDto.DontRender));
        DefineMap<Macro, MacroDto>(nameof(Macro.Name), nameof(MacroDto.Name));
        DefineMap<Macro, MacroDto>(nameof(Macro.CacheDuration), nameof(MacroDto.RefreshRate));
        DefineMap<Macro, MacroDto>(nameof(Macro.MacroSource), nameof(MacroDto.MacroSource));
        DefineMap<Macro, MacroDto>(nameof(Macro.UseInEditor), nameof(MacroDto.UseInEditor));
    }
}
