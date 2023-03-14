namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Defines an application cache.
/// </summary>
public interface IAppCache
{
    /// <summary>
    ///     Gets an item identified by its key.
    /// </summary>
    /// <param name="key">The key of the item.</param>
    /// <returns>The item, or null if the item was not found.</returns>
    object? Get(string key);

    /// <summary>
    ///     Gets or creates an item identified by its key.
    /// </summary>
    /// <param name="key">The key of the item.</param>
    /// <param name="factory">A factory function that can create the item.</param>
    /// <returns>The item.</returns>
    object? Get(string key, Func<object?> factory);

    /// <summary>
    ///     Gets items with a key starting with the specified value.
    /// </summary>
    /// <param name="keyStartsWith">The StartsWith value to use in the search.</param>
    /// <returns>Items matching the search.</returns>
    IEnumerable<object?> SearchByKey(string keyStartsWith);

    /// <summary>
    ///     Gets items with a key matching a regular expression.
    /// </summary>
    /// <param name="regex">The regular expression.</param>
    /// <returns>Items matching the search.</returns>
    IEnumerable<object?> SearchByRegex(string regex);

    /// <summary>
    ///     Removes all items from the cache.
    /// </summary>
    void Clear();

    /// <summary>
    ///     Removes an item identified by its key from the cache.
    /// </summary>
    /// <param name="key">The key of the item.</param>
    void Clear(string key);

    /// <summary>
    ///     Removes items of a specified type from the cache.
    /// </summary>
    /// <param name="type">The type to remove.</param>
    /// <remarks>
    ///     <para>
    ///         If the type is an interface, then all items of a type implementing that interface are
    ///         removed. Otherwise, only items of that exact type are removed (items of type inheriting from
    ///         the specified type are not removed).
    ///     </para>
    ///     <para>Performs a case-sensitive search.</para>
    /// </remarks>
    void ClearOfType(Type type);

    /// <summary>
    ///     Removes items of a specified type from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the items to remove.</typeparam>
    /// <remarks>
    ///     If the type is an interface, then all items of a type implementing that interface are
    ///     removed. Otherwise, only items of that exact type are removed (items of type inheriting from
    ///     the specified type are not removed).
    /// </remarks>
    void ClearOfType<T>();

    /// <summary>
    ///     Removes items of a specified type from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the items to remove.</typeparam>
    /// <param name="predicate">The predicate to satisfy.</param>
    /// <remarks>
    ///     If the type is an interface, then all items of a type implementing that interface are
    ///     removed. Otherwise, only items of that exact type are removed (items of type inheriting from
    ///     the specified type are not removed).
    /// </remarks>
    void ClearOfType<T>(Func<string, T, bool> predicate);

    /// <summary>
    ///     Clears items with a key starting with the specified value.
    /// </summary>
    /// <param name="keyStartsWith">The StartsWith value to use in the search.</param>
    void ClearByKey(string keyStartsWith);

    /// <summary>
    ///     Clears items with a key matching a regular expression.
    /// </summary>
    /// <param name="regex">The regular expression.</param>
    void ClearByRegex(string regex);
}
