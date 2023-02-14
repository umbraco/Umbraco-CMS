using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Plugin;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.Package;

public class PluginConfigurationViewModelMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<PluginConfiguration, PluginConfigurationViewModel>((_, _) => new PluginConfigurationViewModel(), Map);

    // Umbraco.Code.MapAll
    private static void Map(PluginConfiguration source, PluginConfigurationViewModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Version = source.Version;
        target.Extensions = source.Extensions;
    }
}
