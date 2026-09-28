using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Manifest;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.Manifest;

/// <summary>
/// Provides mapping configuration between manifest data models and their corresponding view models in the Umbraco CMS API management layer.
/// </summary>
public class ManifestViewModelMapDefinition : IMapDefinition
{
    private readonly IOptionsMonitor<UmbracoPluginSettings> _pluginSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestViewModelMapDefinition"/> class.
    /// </summary>
    /// <param name="pluginSettings">The plugin settings, used to read the host cache-buster.</param>
    public ManifestViewModelMapDefinition(IOptionsMonitor<UmbracoPluginSettings> pluginSettings)
        => _pluginSettings = pluginSettings;

    /// <summary>
    /// Configures object-object mappings between <see cref="PackageManifest"/> and <see cref="ManifestResponseModel"/> for the Umbraco API management layer.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance on which to define the mappings.</param>
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<PackageManifest, ManifestResponseModel>((_, _) => new ManifestResponseModel(), Map);

    // Umbraco.Code.MapAll
    private void Map(PackageManifest source, ManifestResponseModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Id = source.Id;
        target.Version = source.Version;
        target.CacheBuster = source.AllowCacheBusting
            ? PackageManifestCacheBuster.ComputeCacheBuster(source.Version, _pluginSettings.CurrentValue.Cachebuster)
            : null;
        target.Extensions = source.Extensions;
    }
}
