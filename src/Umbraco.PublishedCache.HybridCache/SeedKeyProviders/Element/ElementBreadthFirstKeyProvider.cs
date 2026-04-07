using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Element;

/// <summary>
/// Provides seed keys for the element cache by traversing the element tree breadth-first
/// and filtering out unpublished elements.
/// </summary>
internal sealed class ElementBreadthFirstKeyProvider : BreadthFirstKeyProvider, IElementSeedKeyProvider
{
    private readonly IElementPublishStatusQueryService _publishStatusService;
    private readonly int _seedCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementBreadthFirstKeyProvider"/> class.
    /// </summary>
    /// <param name="elementNavigationQueryService">The element navigation query service for traversing the element tree.</param>
    /// <param name="cacheSettings">The cache settings containing the element seed count.</param>
    /// <param name="publishStatusService">The publish status service for filtering unpublished elements.</param>
    public ElementBreadthFirstKeyProvider(
        IElementNavigationQueryService elementNavigationQueryService,
        IOptions<CacheSettings> cacheSettings,
        IElementPublishStatusQueryService publishStatusService)
        : base(elementNavigationQueryService, cacheSettings.Value.ElementBreadthFirstSeedCount)
    {
        _publishStatusService = publishStatusService;
        _seedCount = cacheSettings.Value.ElementBreadthFirstSeedCount;
    }

    /// <inheritdoc/>
    // Override to filter out unpublished elements during traversal, ensuring the seed count
    // is reached even if some elements are filtered out (same approach as DocumentBreadthFirstKeyProvider).
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

        rootKeys = rootKeys.Where(x => _publishStatusService.IsPublishedInAnyCulture(x));

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

            childKeys = childKeys.Where(x => _publishStatusService.IsPublishedInAnyCulture(x));

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
