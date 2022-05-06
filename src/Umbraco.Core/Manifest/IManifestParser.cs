namespace Umbraco.Cms.Core.Manifest;

public interface IManifestParser
{
    string AppPluginsPath { get; set; }

    /// <summary>
    ///     Gets all manifests, merged into a single manifest object.
    /// </summary>
    /// <returns></returns>
    CompositePackageManifest CombinedManifest { get; }

    /// <summary>
    ///     Parses a manifest.
    /// </summary>
    PackageManifest ParseManifest(string text);

    /// <summary>
    ///     Returns all package individual manifests
    /// </summary>
    /// <returns></returns>
    IEnumerable<PackageManifest> GetManifests();
}
