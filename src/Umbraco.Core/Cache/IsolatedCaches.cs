namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Represents a dictionary of <see cref="IAppPolicyCache" /> for types.
/// </summary>
/// <remarks>
///     <para>
///         Isolated caches are used by e.g. repositories, to ensure that each cached entity
///         type has its own cache, so that lookups are fast and the repository does not need to
///         search through all keys on a global scale.
///     </para>
/// </remarks>
public class IsolatedCaches : AppPolicedCacheDictionary<Type>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="IsolatedCaches" /> class.
    /// </summary>
    /// <param name="cacheFactory"></param>
    public IsolatedCaches(Func<Type, IAppPolicyCache> cacheFactory)
        : base(cacheFactory)
    {
    }

    /// <summary>
    ///     Gets a cache.
    /// </summary>
    public IAppPolicyCache GetOrCreate<T>()
        => GetOrCreate(typeof(T));

    /// <summary>
    ///     Tries to get a cache.
    /// </summary>
    public Attempt<IAppPolicyCache?> Get<T>()
        => Get(typeof(T));

    /// <summary>
    ///     Clears a cache.
    /// </summary>
    public void ClearCache<T>()
        => ClearCache(typeof(T));
}
