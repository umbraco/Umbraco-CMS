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
    private readonly PackageManifestSettings _packageManifestSettings;

    public PackageManifestService(
        IEnumerable<IPackageManifestReader> packageManifestReaders,
        AppCaches appCaches,
        IOptions<PackageManifestSettings> packageManifestSettings)
    {
        _packageManifestReaders = packageManifestReaders;
        _packageManifestSettings = packageManifestSettings.Value;
        _cache = appCaches.RuntimeCache;
    }

    public async Task<IEnumerable<PackageManifest>> GetPackageManifestsAsync()
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
               _packageManifestSettings.CacheTimeout)
           ?? Array.Empty<PackageManifest>();
}
