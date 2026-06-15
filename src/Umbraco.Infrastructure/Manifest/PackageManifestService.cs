using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Manifest;

internal sealed class PackageManifestService : IPackageManifestService
{
    private readonly IEnumerable<IPackageManifestReader> _packageManifestReaders;
    private readonly IAppPolicyCache _cache;
    private RuntimeSettings _runtimeSettings;

    // The global cache-bust hash only depends on the debug mode and Umbraco version, both fixed for the application's
    // lifetime, so it is computed once here rather than on every importmap request (this service is a singleton).
    private readonly string _globalCacheBustHash;


    /// <summary>
    /// Initializes a new instance of the <see cref="PackageManifestService"/> class.
    /// </summary>
    /// <param name="packageManifestReaders">A collection of <see cref="IPackageManifestReader"/> instances used to read package manifests.</param>
    /// <param name="appCaches">The <see cref="AppCaches"/> instance used for caching manifest data.</param>
    /// <param name="runtimeSettingsOptionsMonitor">An <see cref="IOptionsMonitor{RuntimeSettings}"/> used to monitor runtime configuration settings.</param>
    /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/> used to determine the current hosting environment (e.g. debug mode).</param>
    /// <param name="umbracoVersion">The <see cref="IUmbracoVersion"/> used as a fallback cache-bust hash source when a package has no version.</param>
    public PackageManifestService(
        IEnumerable<IPackageManifestReader> packageManifestReaders,
        AppCaches appCaches,
        IOptionsMonitor<RuntimeSettings> runtimeSettingsOptionsMonitor,
        IHostingEnvironment hostingEnvironment,
        IUmbracoVersion umbracoVersion)
    {
        _packageManifestReaders = packageManifestReaders;
        _cache = appCaches.RuntimeCache;
        _runtimeSettings = runtimeSettingsOptionsMonitor.CurrentValue;
        runtimeSettingsOptionsMonitor.OnChange(runtimeSettings => _runtimeSettings = runtimeSettings);
        _globalCacheBustHash = CacheBustHashGenerator.Generate(hostingEnvironment, umbracoVersion);
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

        // Last-wins on duplicate import/scope keys across packages (the old ToDictionary threw, letting one package break the whole importmap).
        var importDict = new Dictionary<string, string>();
        var scopesDict = new Dictionary<string, Dictionary<string, string>>();

        foreach (PackageManifest manifest in packageManifests)
        {
            AppendStampedImportmap(manifest, _globalCacheBustHash, importDict, scopesDict);
        }

        return new PackageManifestImportmap
        {
            Imports = importDict,
            Scopes = scopesDict,
        };
    }

    private static void AppendStampedImportmap(
        PackageManifest manifest,
        string globalHash,
        Dictionary<string, string> importDict,
        Dictionary<string, Dictionary<string, string>> scopesDict)
    {
        PackageManifestImportmap? importmap = manifest.Importmap;
        if (importmap is null)
        {
            return;
        }

        (var hash, var stamp) = PackageManifestCacheBuster.ResolvePackageCacheBust(manifest, globalHash);

        foreach ((var key, var value) in importmap.Imports)
        {
            importDict[key] = PackageManifestCacheBuster.ApplyCacheBust(value, hash, stamp);
        }

        foreach ((var scopeKey, Dictionary<string, string> scopeImports) in importmap.Scopes ?? new Dictionary<string, Dictionary<string, string>>())
        {
            scopesDict[scopeKey] = StampScope(scopeImports, hash, stamp);
        }
    }

    private static Dictionary<string, string> StampScope(Dictionary<string, string> scopeImports, string hash, bool stamp)
    {
        var stampedScope = new Dictionary<string, string>();
        foreach ((var key, var value) in scopeImports)
        {
            stampedScope[key] = PackageManifestCacheBuster.ApplyCacheBust(value, hash, stamp);
        }

        return stampedScope;
    }
}
