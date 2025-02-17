using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Document;

internal sealed class DocumentBreadthFirstKeyProvider : BreadthFirstKeyProvider, IDocumentSeedKeyProvider
{
    private readonly IPublishStatusQueryService _publishStatusService;
    private readonly int _seedCount;

    public DocumentBreadthFirstKeyProvider(
        IDocumentNavigationQueryService documentNavigationQueryService,
        IOptions<CacheSettings> cacheSettings,
        IPublishStatusQueryService publishStatusService)
        : base(documentNavigationQueryService, cacheSettings.Value.DocumentBreadthFirstSeedCount)
    {
        _publishStatusService = publishStatusService;
        _seedCount = cacheSettings.Value.DocumentBreadthFirstSeedCount;
    }


    // TODO: V16 - Move this method back to the base class
    // The main need for this is because we now need to filter the keys, based on if they have published ancestor path or not
    // We should add `FilterKeys` virtual method on the base class that does nothing, and then override it here instead
    // Note that it's important that we do this filtering as we're doing the search, since we want to make sure we hit the seed count
    // For instance if you have 500 content nodes, request 100 seeded, we need to return 100 keys, even if we need to filter out 20 of them
    public new ISet<Guid> GetSeedKeys()
    {
        if (_seedCount == 0)
        {
            return new HashSet<Guid>();
        }

        Queue<Guid> keyQueue = new();
        HashSet<Guid> keys = [];
        int keyCount = 0;

        if (NavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys) is false)
        {
            return new HashSet<Guid>();
        }

        rootKeys = rootKeys.Where(x => _publishStatusService.IsDocumentPublishedInAnyCulture(x));

        foreach (Guid key in rootKeys)
        {
            keyCount++;
            keys.Add(key);
            keyQueue.Enqueue(key);
            if (keyCount == _seedCount)
            {
                return keys;
            }
        }

        while (keyQueue.Count > 0 && keyCount < _seedCount)
        {
            Guid key = keyQueue.Dequeue();

            if (NavigationQueryService.TryGetChildrenKeys(key, out IEnumerable<Guid> childKeys) is false)
            {
                continue;
            }

            childKeys = childKeys.Where(x => _publishStatusService.IsDocumentPublishedInAnyCulture(x));

            foreach (Guid childKey in childKeys)
            {
                keys.Add(childKey);
                keyCount++;
                if (keyCount == _seedCount)
                {
                    return keys;
                }

                keyQueue.Enqueue(childKey);
            }
        }

        return keys;
    }
}
