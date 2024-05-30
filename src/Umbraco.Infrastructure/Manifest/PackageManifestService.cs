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

    public async Task<IEnumerable<PackageManifest>> GetAllPackageManifestsAsync()
        => await _cache.GetCacheItemAsync(
               $"{nameof(PackageManifestService)}-PackageManifests",
               async () =>
               {
                   Task<IEnumerable<PackageManifest>>[] tasks = _packageManifestReaders
                       .Select(x => x.ReadPackageManifestsAsync())
                       .ToArray();
                   await Task.WhenAll(tasks);

                   return tasks.SelectMany(x => x.Result);
               },
               _runtimeSettings.Mode == RuntimeMode.Production
                   ? TimeSpan.FromDays(30)
                   : TimeSpan.FromSeconds(10))
           ?? Array.Empty<PackageManifest>();

    public async Task<IEnumerable<PackageManifest>> GetPublicPackageManifestsAsync()
        => (await GetAllPackageManifestsAsync()).Where(manifest => manifest.AllowPublicAccess);

    public async Task<IEnumerable<PackageManifest>> GetPrivatePackageManifestsAsync()
        => (await GetAllPackageManifestsAsync()).Where(manifest => manifest.AllowPublicAccess == false);

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
