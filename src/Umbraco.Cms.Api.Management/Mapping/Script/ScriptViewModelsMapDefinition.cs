using Umbraco.Cms.Api.Management.ViewModels.Script;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.Script;

public class ScriptViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<CreateScriptRequestModel, ScriptCreateModel>((_, _) => new ScriptCreateModel{ Name = string.Empty }, Map);
    }

    private void Map(CreateScriptRequestModel source, ScriptCreateModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.ParentPath = source.ParentPath;
        target.Content = source.Content;
    }
}
