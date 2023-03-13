namespace Umbraco.Cms.Core.Manifest;

public interface ILegacyManifestParser
{
    string AppPluginsPath { get; set; }

    /// <summary>
    ///     Gets all manifests, merged into a single manifest object.
    /// </summary>
    /// <returns></returns>
    CompositeLegacyPackageManifest CombinedManifest { get; }

    /// <summary>
    ///     Parses a manifest.
    /// </summary>
    LegacyPackageManifest ParseManifest(string text);

    /// <summary>
    ///     Returns all package individual manifests
    /// </summary>
    /// <returns></returns>
    IEnumerable<LegacyPackageManifest> GetManifests();
}
