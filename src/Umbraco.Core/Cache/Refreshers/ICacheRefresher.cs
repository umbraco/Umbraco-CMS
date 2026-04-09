using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Defines a cache refresher used for distributed cache invalidation in load-balanced environments.
/// </summary>
/// <remarks>
///     Cache refreshers are responsible for invalidating or refreshing cached data across all servers
///     in a load-balanced cluster when data changes occur.
/// </remarks>
public interface ICacheRefresher : IDiscoverable
{
    /// <summary>
    ///     Gets the unique identifier for this cache refresher.
    /// </summary>
    Guid RefresherUniqueId { get; }

    /// <summary>
    ///     Gets the name of this cache refresher.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Refreshes all cached items managed by this refresher.
    /// </summary>
    void RefreshAll();

    /// <summary>
    ///     Refreshes a specific cached item by its integer identifier.
    /// </summary>
    /// <param name="id">The identifier of the item to refresh.</param>
    void Refresh(int id);

    /// <summary>
    ///     Removes a specific cached item by its integer identifier.
    /// </summary>
    /// <param name="id">The identifier of the item to remove.</param>
    void Remove(int id);

    /// <summary>
    ///     Refreshes a specific cached item by its GUID identifier.
    /// </summary>
    /// <param name="id">The GUID of the item to refresh.</param>
    void Refresh(Guid id);
}

/// <summary>
///     Defines a strongly typed cache refresher that can refresh cache using object instances.
/// </summary>
/// <typeparam name="T">The type of entity being cached.</typeparam>
/// <remarks>
///     This interface provides better performance in non-load-balanced environments by allowing
///     cache refresh operations on already-resolved object instances, avoiding the need to
///     re-lookup objects by their identifiers.
/// </remarks>
public interface ICacheRefresher<T> : ICacheRefresher
{
    /// <summary>
    ///     Refreshes the cache for the specified instance.
    /// </summary>
    /// <param name="instance">The instance to refresh in the cache.</param>
    void Refresh(T instance);

    /// <summary>
    ///     Removes the specified instance from the cache.
    /// </summary>
    /// <param name="instance">The instance to remove from the cache.</param>
    void Remove(T instance);
}
