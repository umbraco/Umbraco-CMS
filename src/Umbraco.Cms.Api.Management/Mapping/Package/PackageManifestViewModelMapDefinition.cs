using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.Package;

public class PackageManifestViewModelMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<PackageManifest, PackageManifestViewModel>((_, _) => new PackageManifestViewModel(), Map);

    // Umbraco.Code.MapAll
    private static void Map(PackageManifest source, PackageManifestViewModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Version = source.Version;
        target.Extensions = source.Extensions;
    }
}
