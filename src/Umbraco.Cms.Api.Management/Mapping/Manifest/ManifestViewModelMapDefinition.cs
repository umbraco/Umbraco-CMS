using Umbraco.Cms.Api.Management.ViewModels.Manifest;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.Manifest;

/// <summary>
/// Provides mapping configuration between manifest data models and their corresponding view models in the Umbraco CMS API management layer.
/// </summary>
public class ManifestViewModelMapDefinition : IMapDefinition
{
    /// <summary>
    /// Configures object-object mappings between <see cref="PackageManifest"/> and <see cref="ManifestResponseModel"/> for the Umbraco API management layer.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance on which to define the mappings.</param>
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
