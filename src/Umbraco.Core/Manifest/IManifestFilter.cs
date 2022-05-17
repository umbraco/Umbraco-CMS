namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Provides filtering for package manifests.
/// </summary>
public interface IManifestFilter
{
    /// <summary>
    ///     Filters package manifests.
    /// </summary>
    /// <param name="manifests">The package manifests.</param>
    /// <remarks>
    ///     <para>It is possible to remove, change, or add manifests.</para>
    /// </remarks>
    void Filter(List<PackageManifest> manifests);
}
