using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Core.Models.Mapping;

public class TextFileMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IScript, ScriptFile>((_, _) => new ScriptFile { FileName = string.Empty }, Map);
    }

    private void Map(IScript source, ScriptFile target, MapperContext context)
    {
        target.FileName = source.Name ?? string.Empty;
        target.Path = source.Path;
        target.Content = source.Content ?? string.Empty;
        target.UpdateDate = source.UpdateDate;
        target.CreateDate = source.CreateDate;
    }
}
