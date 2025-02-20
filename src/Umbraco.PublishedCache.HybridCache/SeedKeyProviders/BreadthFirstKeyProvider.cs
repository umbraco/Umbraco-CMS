using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders;

public abstract class BreadthFirstKeyProvider
{
    protected readonly INavigationQueryService NavigationQueryService;
    private readonly int _seedCount;

    public BreadthFirstKeyProvider(INavigationQueryService navigationQueryService, int seedCount)
    {
        NavigationQueryService = navigationQueryService;
        _seedCount = seedCount;
    }

    public ISet<Guid> GetSeedKeys()
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
