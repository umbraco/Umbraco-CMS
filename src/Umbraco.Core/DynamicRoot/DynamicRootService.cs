using Umbraco.Cms.Core.DynamicRoot.QuerySteps;
using Umbraco.Cms.Core.DynamicRoot.Origin;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DynamicRoot;

public class DynamicRootService : IDynamicRootService
{
    private readonly DynamicRootOriginFinderCollection _originFinderCollection;
    private readonly DynamicRootQueryStepCollection _queryStepCollection;

    public DynamicRootService(DynamicRootOriginFinderCollection originFinderCollection, DynamicRootQueryStepCollection queryStepCollection)
    {
        _originFinderCollection = originFinderCollection;
        _queryStepCollection = queryStepCollection;
    }

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

