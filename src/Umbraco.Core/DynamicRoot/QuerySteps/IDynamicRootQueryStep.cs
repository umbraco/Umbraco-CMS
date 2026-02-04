using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

/// <summary>
///     Defines a query step that can filter or traverse the content tree during dynamic root resolution.
/// </summary>
public interface IDynamicRootQueryStep
{
    /// <summary>
    ///     Executes the query step on the specified origin content items.
    /// </summary>
    /// <param name="origins">The collection of content keys to use as starting points for this query step.</param>
    /// <param name="filter">The query step configuration containing the alias and document type filter criteria.</param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing an <see cref="Attempt{T}"/> with the filtered content keys.
    ///     Returns a failed attempt if this query step does not support the specified alias.
    /// </returns>
    Task<Attempt<ICollection<Guid>>> ExecuteAsync(ICollection<Guid> origins, DynamicRootQueryStep filter);

    /// <summary>
    ///     Gets the direction alias that this query step supports (e.g., "NearestAncestorOrSelf", "FurthestDescendantOrSelf").
    /// </summary>
    string SupportedDirectionAlias { get; }
}
