using Umbraco.Cms.Api.Management.ViewModels.Manifest;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.Manifest;

public class ManifestViewModelMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<PackageManifest, ManifestResponseModel>((_, _) => new ManifestResponseModel(), Map);

    // Umbraco.Code.MapAll
    private static void Map(PackageManifest source, ManifestResponseModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Id = source.Id;
        target.Version = source.Version;
        target.Extensions = source.Extensions;
    }
}
