namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Represents a context cache used by Deploy operations.
/// </summary>
public interface IContextCache
{
    /// <summary>
    /// Creates the item on the context cache using the specified <paramref name="key" />.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="key">The key of the cached item.</param>
    /// <param name="item">The item.</param>
    void Create<T>(string key, T item);

    /// <summary>
    /// Gets an item from the context cache or creates and stores it using the specified <paramref name="key" />.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="key">The key of the cached item.</param>
    /// <param name="factory">The factory method to create the item (if it doesn't exist yet).</param>
    /// <returns>
    /// The item.
    /// </returns>
    T? GetOrCreate<T>(string key, Func<T?> factory);

    /// <summary>
    /// Gets an item from the context cache or creates and stores it using the specified <paramref name="key" />.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="key">The key of the cached item.</param>
    /// <param name="factory">The factory method to create the item (if it doesn't exist yet).</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the item.
    /// </returns>
    async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory)
    {
        // TODO: Remove default implementation in v15
        bool shouldCreate = false;
        T? value = GetOrCreate<T>(key, () =>
        {
            shouldCreate = true;
            return default;
        });

        if (shouldCreate)
        {
            // Only invoke and await if we need to create the value
            value = await factory().ConfigureAwait(false);
            Create(key, value);
        }

        return value;
    }

    /// <summary>
    /// Clears all cached items on this context.
    /// </summary>
    void Clear();
}
