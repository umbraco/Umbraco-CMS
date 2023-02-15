using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Manifest;

internal sealed class PackageManifestService : IPackageManifestService
{
    private readonly IPackageManifestReader _packageManifestReader;
    private readonly IAppPolicyCache _cache;

    public PackageManifestService(IPackageManifestReader packageManifestReader, AppCaches appCaches)
    {
        _packageManifestReader = packageManifestReader;
        _cache = appCaches.RuntimeCache;
    }

    public async Task<IEnumerable<PackageManifest>> GetPackageManifestsAsync()
        => await _cache.GetCacheItemAsync(
               $"{nameof(PackageManifestService)}-PackageManifests",
               async () => await _packageManifestReader.ReadPackageManifestsAsync(),
               TimeSpan.FromMinutes(10))
           ?? Array.Empty<PackageManifest>();
}
