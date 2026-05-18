using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders;

public abstract class BreadthFirstKeyProvider
{
    protected readonly INavigationQueryService NavigationQueryService;
    private readonly int _seedCount;

    protected BreadthFirstKeyProvider(INavigationQueryService navigationQueryService, int seedCount)
    {
        NavigationQueryService = navigationQueryService;
        _seedCount = seedCount;
    }

    public virtual ISet<Guid> GetSeedKeys()
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

        foreach (Guid key in rootKeys)
        {
            if (ShouldSeed(key))
            {
                keys.Add(key);
                keyCount++;
                if (keyCount == _seedCount)
                {
                    return keys;
                }
            }

            if (ShouldTraverseChildren(key))
            {
                keyQueue.Enqueue(key);
            }
        }

        while (keyQueue.Count > 0 && keyCount < _seedCount)
        {
            Guid key = keyQueue.Dequeue();

            if (NavigationQueryService.TryGetChildrenKeys(key, out IEnumerable<Guid> childKeys) is false)
            {
                continue;
            }

            foreach (Guid childKey in childKeys)
            {
                if (ShouldSeed(childKey))
                {
                    keys.Add(childKey);
                    keyCount++;
                    if (keyCount == _seedCount)
                    {
                        return keys;
                    }
                }

                if (ShouldTraverseChildren(childKey))
                {
                    keyQueue.Enqueue(childKey);
                }
            }
        }

        return keys;
    }

    /// <summary>
    /// Determines whether a node should be included in the seed set and counted toward the seed limit.
    /// </summary>
    /// <param name="key">The node key.</param>
    /// <returns><c>true</c> if the node should be seeded; otherwise, <c>false</c>.</returns>
    protected virtual bool ShouldSeed(Guid key) => true;

    /// <summary>
    /// Determines whether a node's children should be traversed during the breadth-first search.
    /// </summary>
    /// <param name="key">The node key.</param>
    /// <returns><c>true</c> if the node's children should be traversed; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// By default, only nodes that pass <see cref="ShouldSeed"/> are traversed.
    /// Override this to traverse nodes that aren't themselves seeded (e.g. containers).
    /// </remarks>
    protected virtual bool ShouldTraverseChildren(Guid key) => ShouldSeed(key);
}
