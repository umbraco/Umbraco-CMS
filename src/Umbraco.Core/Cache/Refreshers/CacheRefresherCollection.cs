using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Represents a collection of <see cref="ICacheRefresher" /> instances.
/// </summary>
/// <remarks>
///     This collection provides access to all registered cache refreshers, allowing lookup by unique identifier.
/// </remarks>
public class CacheRefresherCollection : BuilderCollectionBase<ICacheRefresher>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CacheRefresherCollection" /> class.
    /// </summary>
    /// <param name="items">A factory function that returns the cache refresher instances.</param>
    public CacheRefresherCollection(Func<IEnumerable<ICacheRefresher>> items)
        : base(items)
    {
    }

    /// <summary>
    ///     Gets the cache refresher with the specified unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the cache refresher.</param>
    /// <returns>The cache refresher with the specified identifier, or <c>null</c> if not found.</returns>
    public ICacheRefresher? this[Guid id]
        => this.FirstOrDefault(x => x.RefresherUniqueId == id);
}
