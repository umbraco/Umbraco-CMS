using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Cms.Infrastructure.Manifest;

/// <summary>
/// Defines the interface for reading Umbraco package manifests.
/// </summary>
public interface IPackageManifestReader
{
    /// <summary>
    /// Asynchronously reads and returns all available package manifests.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with a result of an enumerable collection of <see cref="Umbraco.Cms.Infrastructure.Manifest.PackageManifest"/>.</returns>
    Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync();
}
