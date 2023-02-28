using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Manifest;

internal sealed class PackageManifestService : IPackageManifestService
{
    private readonly IEnumerable<IPackageManifestReader> _packageManifestReaders;
    private readonly IAppPolicyCache _cache;

    public PackageManifestService(IEnumerable<IPackageManifestReader> packageManifestReaders, AppCaches appCaches)
    {
        _packageManifestReaders = packageManifestReaders;
        _cache = appCaches.RuntimeCache;
    }

    public async Task<IEnumerable<PackageManifest>> GetPackageManifestsAsync()
        => await _cache.GetCacheItemAsync(
               $"{nameof(PackageManifestService)}-PackageManifests",
               async () =>
               {
                   var tasks = _packageManifestReaders
                       .Select(x => x.ReadPackageManifestsAsync())
                       .ToArray();
                   await Task.WhenAll(tasks);

                   return tasks.SelectMany(x => x.Result);
               },
               TimeSpan.FromMinutes(10))
           ?? Array.Empty<PackageManifest>();
}
