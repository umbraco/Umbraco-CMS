namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for package manifest settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigGlobal)]
public class PackageManifestSettings
{
    public TimeSpan CacheTimeout { get; set; } = TimeSpan.FromMinutes(10);
}
