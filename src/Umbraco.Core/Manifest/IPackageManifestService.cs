namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Provides services for retrieving package manifests.
/// </summary>
public interface IPackageManifestService
{
    /// <summary>
    ///     Gets all package manifests asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation containing all package manifests.</returns>
    Task<IEnumerable<PackageManifest>> GetAllPackageManifestsAsync();

    /// <summary>
    ///     Gets all public package manifests asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation containing public package manifests that allow public access.</returns>
    Task<IEnumerable<PackageManifest>> GetPublicPackageManifestsAsync();

    /// <summary>
    ///     Gets all private package manifests asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation containing private package manifests that do not allow public access.</returns>
    Task<IEnumerable<PackageManifest>> GetPrivatePackageManifestsAsync();

    /// <summary>
    ///     Gets the combined importmap from all package manifests asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation containing the merged <see cref="PackageManifestImportmap"/>.</returns>
    Task<PackageManifestImportmap> GetPackageManifestImportmapAsync();
}
