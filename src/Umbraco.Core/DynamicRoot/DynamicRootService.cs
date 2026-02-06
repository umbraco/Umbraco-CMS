using Umbraco.Cms.Core.DynamicRoot.QuerySteps;
using Umbraco.Cms.Core.DynamicRoot.Origin;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DynamicRoot;

/// <summary>
///     Default implementation of <see cref="IDynamicRootService"/> that resolves dynamic roots for content pickers
///     using a collection of origin finders and query steps.
/// </summary>
public class DynamicRootService : IDynamicRootService
{
    private readonly DynamicRootOriginFinderCollection _originFinderCollection;
    private readonly DynamicRootQueryStepCollection _queryStepCollection;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DynamicRootService"/> class.
    /// </summary>
    /// <param name="originFinderCollection">The collection of origin finders used to locate the starting point for dynamic root resolution.</param>
    /// <param name="queryStepCollection">The collection of query steps used to filter or traverse the content tree from the origin.</param>
    public DynamicRootService(DynamicRootOriginFinderCollection originFinderCollection, DynamicRootQueryStepCollection queryStepCollection)
    {
        _originFinderCollection = originFinderCollection;
        _queryStepCollection = queryStepCollection;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Guid>> GetDynamicRootsAsync(DynamicRootNodeQuery dynamicRootNodeQuery)
    {
        var originKey = FindOriginKey(dynamicRootNodeQuery);

        if (originKey is null)
        {
            return Array.Empty<Guid>();
        }

        // no steps means the origin is the root
        if (dynamicRootNodeQuery.QuerySteps.Any() is false)
        {
            return originKey.Value.Yield();
        }

        // start with the origin
        ICollection<Guid> filtered = new []{originKey.Value};

        // resolved each Query Step using the result of the previous step (or origin)
        foreach (DynamicRootQueryStep startNodeSelectorFilter in dynamicRootNodeQuery.QuerySteps)
        {
            filtered = await ExcuteFiltersAsync(filtered, startNodeSelectorFilter);
        }

        return filtered;
    }

    /// <summary>
    ///     Executes the query steps to filter the origin keys based on the specified query step.
    /// </summary>
    /// <param name="origin">The collection of origin keys to filter.</param>
    /// <param name="dynamicRootQueryStep">The query step defining the filter operation to apply.</param>
    /// <returns>A collection of filtered content keys.</returns>
    /// <exception cref="NotSupportedException">Thrown when no query step handler supports the specified alias.</exception>
    internal async Task<ICollection<Guid>> ExcuteFiltersAsync(ICollection<Guid> origin, DynamicRootQueryStep dynamicRootQueryStep)
    {
        foreach (IDynamicRootQueryStep queryStep in _queryStepCollection)
        {
            var queryStepAttempt = await queryStep.ExecuteAsync(origin, dynamicRootQueryStep);
            if (queryStepAttempt is { Success: true, Result: not null })
            {
                return queryStepAttempt.Result;
            }
        }

        throw new NotSupportedException($"Did not find any filteres that could handle {dynamicRootQueryStep.Alias}");
    }

    /// <summary>
    ///     Finds the origin key using the configured origin finders.
    /// </summary>
    /// <param name="dynamicRootNodeQuery">The query containing the origin alias and context.</param>
    /// <returns>The unique identifier of the origin content, or <c>null</c> if no origin finder could resolve it.</returns>
    internal Guid? FindOriginKey(DynamicRootNodeQuery dynamicRootNodeQuery)
    {
        foreach (IDynamicRootOriginFinder originFinder in _originFinderCollection)
        {
            Guid? originKey = originFinder.FindOriginKey(dynamicRootNodeQuery);

            if (originKey is not null)
            {
                return originKey;
            }
        }

        return null;
    }
}
