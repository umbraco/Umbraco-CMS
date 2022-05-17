namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Defines an application cache that support cache policies.
/// </summary>
/// <remarks>
///     A cache policy can be used to cache with timeouts,
///     or depending on files, and with a remove callback, etc.
/// </remarks>
public interface IAppPolicyCache : IAppCache
{
    /// <summary>
    ///     Gets an item identified by its key.
    /// </summary>
    /// <param name="key">The key of the item.</param>
    /// <param name="factory">A factory function that can create the item.</param>
    /// <param name="timeout">An optional cache timeout.</param>
    /// <param name="isSliding">An optional value indicating whether the cache timeout is sliding (default is false).</param>
    /// <param name="dependentFiles">Files the cache entry depends on.</param>
    /// <returns>The item.</returns>
    object? Get(
        string key,
        Func<object?> factory,
        TimeSpan? timeout,
        bool isSliding = false,
        string[]? dependentFiles = null);

    /// <summary>
    ///     Inserts an item.
    /// </summary>
    /// <param name="key">The key of the item.</param>
    /// <param name="factory">A factory function that can create the item.</param>
    /// <param name="timeout">An optional cache timeout.</param>
    /// <param name="isSliding">An optional value indicating whether the cache timeout is sliding (default is false).</param>
    /// <param name="dependentFiles">Files the cache entry depends on.</param>
    void Insert(
        string key,
        Func<object?> factory,
        TimeSpan? timeout = null,
        bool isSliding = false,
        string[]? dependentFiles = null);
}
