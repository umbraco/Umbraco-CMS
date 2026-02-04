namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Defines a request-level cache that stores items for the duration of a single HTTP request.
/// </summary>
/// <remarks>
///     The request cache is designed to store transient data that is only valid within the context
///     of a single HTTP request. Outside a web environment, the behavior of this cache is unspecified.
/// </remarks>
public interface IRequestCache : IAppCache, IEnumerable<KeyValuePair<string, object?>>
{
    /// <summary>
    ///     Gets a value indicating whether the request cache is available.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the request cache is available; otherwise, <c>false</c>.
    /// </value>
    bool IsAvailable { get; }

    /// <summary>
    ///     Sets a value in the request cache.
    /// </summary>
    /// <param name="key">The key of the item to set.</param>
    /// <param name="value">The value to store in the cache.</param>
    /// <returns><c>true</c> if the value was set successfully; otherwise, <c>false</c>.</returns>
    bool Set(string key, object? value);

    /// <summary>
    ///     Removes a value from the request cache.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    /// <returns><c>true</c> if the item was removed successfully; otherwise, <c>false</c>.</returns>
    bool Remove(string key);
}
