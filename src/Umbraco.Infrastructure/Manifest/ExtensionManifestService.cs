using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

internal sealed class ExtensionManifestService : IExtensionManifestService
{
    private readonly IExtensionManifestReader _extensionManifestReader;
    private readonly IAppPolicyCache _cache;

    public ExtensionManifestService(IExtensionManifestReader extensionManifestReader, AppCaches appCaches)
    {
        _extensionManifestReader = extensionManifestReader;
        _cache = appCaches.RuntimeCache;
    }

    public async Task<IEnumerable<ExtensionManifest>> GetManifestsAsync()
        => await _cache.GetCacheItemAsync(
               $"{nameof(ExtensionManifestService)}-Manifests",
               async () => await _extensionManifestReader.ReadManifestsAsync(),
               TimeSpan.FromMinutes(10))
           ?? Array.Empty<ExtensionManifest>();
}
