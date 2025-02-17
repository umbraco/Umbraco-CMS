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
