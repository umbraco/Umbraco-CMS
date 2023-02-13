using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.Package;

public class ExtensionManifestViewModelMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<ExtensionManifest, ExtensionManifestViewModel>((_, _) => new ExtensionManifestViewModel(), Map);

    // Umbraco.Code.MapAll
    private static void Map(ExtensionManifest source, ExtensionManifestViewModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Version = source.Version;
        target.Extensions = source.Extensions;
    }
}
