namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

/// <summary>
///     Provides data access operations for dynamic root query steps, supporting ancestor and descendant traversal with document type filtering.
/// </summary>
public interface IDynamicRootRepository
{
    /// <summary>
    ///     Finds the nearest (closest to origin) ancestor or self that matches the query step criteria.
    /// </summary>
    /// <param name="origins">The collection of content keys to start the search from.</param>
    /// <param name="queryStep">The query step containing document type filter criteria.</param>
    /// <returns>A task representing the asynchronous operation, containing the key of the nearest matching ancestor or self, or <c>null</c> if none found.</returns>
    Task<Guid?> NearestAncestorOrSelfAsync(IEnumerable<Guid> origins, DynamicRootQueryStep queryStep);

    /// <summary>
    ///     Finds the furthest (topmost) ancestor or self that matches the query step criteria.
    /// </summary>
    /// <param name="origins">The collection of content keys to start the search from.</param>
    /// <param name="queryStep">The query step containing document type filter criteria.</param>
    /// <returns>A task representing the asynchronous operation, containing the key of the furthest matching ancestor or self, or <c>null</c> if none found.</returns>
    Task<Guid?> FurthestAncestorOrSelfAsync(IEnumerable<Guid> origins, DynamicRootQueryStep queryStep);

    /// <summary>
    ///     Finds the nearest (closest to origin) descendants or self that match the query step criteria.
    /// </summary>
    /// <param name="origins">The collection of content keys to start the search from.</param>
    /// <param name="queryStep">The query step containing document type filter criteria.</param>
    /// <returns>A task representing the asynchronous operation, containing a collection of keys for the nearest matching descendants or self.</returns>
    Task<ICollection<Guid>> NearestDescendantOrSelfAsync(ICollection<Guid> origins, DynamicRootQueryStep queryStep);

    /// <summary>
    ///     Finds the furthest (deepest) descendants or self that match the query step criteria.
    /// </summary>
    /// <param name="origins">The collection of content keys to start the search from.</param>
    /// <param name="queryStep">The query step containing document type filter criteria.</param>
    /// <returns>A task representing the asynchronous operation, containing a collection of keys for the furthest matching descendants or self.</returns>
    Task<ICollection<Guid>> FurthestDescendantOrSelfAsync(ICollection<Guid> origins, DynamicRootQueryStep queryStep);
}
