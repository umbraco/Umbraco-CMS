using Umbraco.Cms.Core.DynamicRoot.QuerySteps;
using Umbraco.Cms.Core.DynamicRoot.Origin;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DynamicRoot;

public class DynamicRootService : IDynamicRootService
{
    private readonly DynamicRootOriginCollection _originCollection;
    private readonly DynamicRootQueryStepCollection _dynamicRootQueryStepCollection;

    public DynamicRootService(DynamicRootOriginCollection originCollection, DynamicRootQueryStepCollection dynamicRootQueryStepCollection)
    {
        _originCollection = originCollection;
        _dynamicRootQueryStepCollection = dynamicRootQueryStepCollection;
    }

    public IEnumerable<Guid> GetDynamicRoots(DynamicRootNodeSelector dynamicRootNodeSelector)
    {
        var originKey = FindOriginKey(dynamicRootNodeSelector);

        if (originKey is null)
        {
            return Array.Empty<Guid>();
        }

        if (dynamicRootNodeSelector.QuerySteps.Any() is false)
        {
            return originKey.Value.Yield();
        }

        IEnumerable<Guid> filtered = originKey.Value.Yield();
        foreach (DynamicRootQueryStep startNodeSelectorFilter in dynamicRootNodeSelector.QuerySteps)
        {
            filtered = ExcuteFilters(filtered, startNodeSelectorFilter);
        }

        return filtered;
    }

    internal IEnumerable<Guid> ExcuteFilters(IEnumerable<Guid> origin, DynamicRootQueryStep dynamicRootQueryStep)
    {
        foreach (IDynamicRootQueryStep startNodeSelectorFilter in _dynamicRootQueryStepCollection)
        {
            if (startNodeSelectorFilter.Execute(origin, dynamicRootQueryStep, out var filtered))
            {
                return filtered;
            }
        }

        throw new NotSupportedException($"Did not find any filteres that could handle {dynamicRootQueryStep.Alias}");
    }

    internal Guid? FindOriginKey(DynamicRootNodeSelector dynamicRootNodeSelector)
    {
        foreach (IDynamicRootOrigin startNodeOriginFinder in _originCollection)
        {
            Guid? originKey = startNodeOriginFinder.FindOriginKey(dynamicRootNodeSelector);

            if (originKey is not null)
            {
                return originKey;
            }
        }

        return null;
    }
}

