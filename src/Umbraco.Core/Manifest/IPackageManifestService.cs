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

    /// <summary>
    ///     Gets the merged list of importmap entries that should be preloaded
    ///     via <c>&lt;link rel="modulepreload"&gt;</c> at boot.
    /// </summary>
    /// <returns>
    ///     A task containing a de-duplicated <see cref="IReadOnlyList{T}"/> of resolved URLs (each preload alias
    ///     resolved against its source manifest's <see cref="PackageManifestImportmap.Imports"/>),
    ///     preserving the order packages were loaded in.
    /// </returns>
    /// <remarks>
    ///     The default implementation returns an empty list, so any existing
    ///     <see cref="IPackageManifestService" /> implementation continues to compile and run.
    ///     The shipped implementation is provided by the framework.
    ///     Scheduled to become a required member in Umbraco 19.
    /// </remarks>
    Task<IReadOnlyList<string>> GetPackageManifestPreloadAsync()
        => Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
}
