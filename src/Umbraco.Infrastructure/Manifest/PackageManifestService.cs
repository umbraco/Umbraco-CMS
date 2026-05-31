using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Manifest;

internal sealed class PackageManifestService : IPackageManifestService
{
    private readonly IEnumerable<IPackageManifestReader> _packageManifestReaders;
    private readonly IAppPolicyCache _cache;
    private RuntimeSettings _runtimeSettings;


    /// <summary>
    /// Initializes a new instance of the <see cref="PackageManifestService"/> class.
    /// </summary>
    /// <param name="packageManifestReaders">A collection of <see cref="IPackageManifestReader"/> instances used to read package manifests.</param>
    /// <param name="appCaches">The <see cref="AppCaches"/> instance used for caching manifest data.</param>
    /// <param name="runtimeSettingsOptionsMonitor">An <see cref="IOptionsMonitor{RuntimeSettings}"/> used to monitor runtime configuration settings.</param>
    public PackageManifestService(
        IEnumerable<IPackageManifestReader> packageManifestReaders,
        AppCaches appCaches,
        IOptionsMonitor<RuntimeSettings> runtimeSettingsOptionsMonitor)
    {
        _packageManifestReaders = packageManifestReaders;
        _cache = appCaches.RuntimeCache;
        _runtimeSettings = runtimeSettingsOptionsMonitor.CurrentValue;
        runtimeSettingsOptionsMonitor.OnChange(runtimeSettings => _runtimeSettings = runtimeSettings);
    }

    /// <summary>
    /// Asynchronously retrieves all package manifests from the registered package manifest readers, caching the results for improved performance.
    /// The cache duration depends on the runtime mode: 30 days in production, 10 seconds otherwise.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="IEnumerable{T}"/> of <see cref="Umbraco.Cms.Infrastructure.Manifest.PackageManifest"/> instances.
    /// </returns>
    public async Task<IEnumerable<PackageManifest>> GetAllPackageManifestsAsync()
        => await _cache.GetCacheItemAsync(
               $"{nameof(PackageManifestService)}-PackageManifests",
               async () =>
               {
                   var packageManifests = new List<PackageManifest>();
                   foreach (IPackageManifestReader packageManifestReader in _packageManifestReaders)
                   {
                       packageManifests.AddRange(await packageManifestReader.ReadPackageManifestsAsync());
                   }

                   return packageManifests;
               },
               _runtimeSettings.Mode == RuntimeMode.Production
                   ? TimeSpan.FromDays(30)
                   : TimeSpan.FromSeconds(10))
           ?? Enumerable.Empty<PackageManifest>();

    /// <summary>
    /// Asynchronously retrieves all package manifests that have public access enabled.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains an enumerable collection of <see cref="PackageManifest"/> instances where public access is allowed.</returns>
    public async Task<IEnumerable<PackageManifest>> GetPublicPackageManifestsAsync()
        => (await GetAllPackageManifestsAsync()).Where(manifest => manifest.AllowPublicAccess);

    /// <summary>
    /// Asynchronously retrieves all private package manifests, i.e., manifests that do not allow public access.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with a result of an enumerable collection of private <see cref="Umbraco.Cms.Infrastructure.Manifest.PackageManifest"/> instances.</returns>
    public async Task<IEnumerable<PackageManifest>> GetPrivatePackageManifestsAsync()
        => (await GetAllPackageManifestsAsync()).Where(manifest => manifest.AllowPublicAccess == false);

    /// <summary>
    /// Asynchronously retrieves and combines the import maps from all available package manifests.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a <see cref="Umbraco.Cms.Infrastructure.Manifest.PackageManifestImportmap"/> instance
    /// that merges the <c>imports</c> and <c>scopes</c> from all non-null import maps found in the loaded package manifests.
    /// </returns>
    public async Task<PackageManifestImportmap> GetPackageManifestImportmapAsync()
    {
        IEnumerable<PackageManifest> packageManifests = await GetAllPackageManifestsAsync();
        var manifests = packageManifests.Select(x => x.Importmap).WhereNotNull().ToList();

        var importDict = manifests
            .SelectMany(x => x.Imports)
            .ToDictionary(x => x.Key, x => x.Value);
        var scopesDict = manifests
            .SelectMany(x => x.Scopes ?? new Dictionary<string, Dictionary<string, string>>())
            .ToDictionary(x => x.Key, x => x.Value);

        return new PackageManifestImportmap
        {
            Imports = importDict,
            Scopes = scopesDict,
        };
    }
}
