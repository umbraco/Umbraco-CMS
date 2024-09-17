using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders;

public class BreadthFirstKeyProvider : IDocumentSeedKeyProvider
{
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;
    private readonly CacheSettings _cacheSettings;

    public BreadthFirstKeyProvider(
        IDocumentNavigationQueryService documentNavigationQueryService,
        IOptions<CacheSettings> cacheSettings)
    {
        _documentNavigationQueryService = documentNavigationQueryService;
        _cacheSettings = cacheSettings.Value;
    }

    public ISet<Guid> GetSeedKeys()
    {
        if (_cacheSettings.BreadthFirstSeedCount == 0)
        {
            return new HashSet<Guid>();
        }

        Queue<Guid> keyQueue = new();
        HashSet<Guid> keys = [];
        int keyCount = 0;

        if (_documentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys) is false)
        {
            return new HashSet<Guid>();
        }

        foreach (Guid key in rootKeys)
        {
            keyCount++;
            keys.Add(key);
            keyQueue.Enqueue(key);
            if (keyCount == _cacheSettings.BreadthFirstSeedCount)
            {
                return keys;
            }
        }

        while (keyQueue.Count > 0 && keyCount < _cacheSettings.BreadthFirstSeedCount)
        {
            Guid key = keyQueue.Dequeue();

            if (_documentNavigationQueryService.TryGetChildrenKeys(key, out IEnumerable<Guid> childKeys) is false)
            {
                continue;
            }

            foreach (Guid childKey in childKeys)
            {
                keys.Add(childKey);
                keyCount++;
                if (keyCount == _cacheSettings.BreadthFirstSeedCount)
                {
                    return keys;
                }

                keyQueue.Enqueue(childKey);
            }
        }

        return keys;
    }
}
