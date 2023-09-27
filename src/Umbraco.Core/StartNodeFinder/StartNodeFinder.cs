using Umbraco.Cms.Core.StartNodeFinder.Filters;
using Umbraco.Cms.Core.StartNodeFinder.Origin;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.StartNodeFinder;

public class StartNodeFinder : IStartNodeFinder
{
    private readonly StartNodeOriginFinderCollection _originFinderCollection;
    private readonly StartNodeSelectorFilterCollection _startNodeSelectorFilterCollection;

    public StartNodeFinder(StartNodeOriginFinderCollection originFinderCollection, StartNodeSelectorFilterCollection startNodeSelectorFilterCollection)
    {
        _originFinderCollection = originFinderCollection;
        _startNodeSelectorFilterCollection = startNodeSelectorFilterCollection;
    }

    public IEnumerable<Guid> GetDynamicStartNodes(StartNodeSelector startNodeSelector)
    {
        var originKey = CalculateOriginKey(startNodeSelector);

        if (originKey is null)
        {
            return Array.Empty<Guid>();
        }

        if (startNodeSelector.Filter.Any() is false)
        {
            return originKey.Value.Yield();
        }

        IEnumerable<Guid> filtered = originKey.Value.Yield();
        foreach (StartNodeFilter startNodeSelectorFilter in startNodeSelector.Filter)
        {
            filtered = ExcuteFilters(filtered, startNodeSelectorFilter);
        }

        return Array.Empty<Guid>();
    }

    internal IEnumerable<Guid> ExcuteFilters(IEnumerable<Guid> origin, StartNodeFilter startNodeFilter)
    {
        foreach (IStartNodeSelectorFilter startNodeSelectorFilter in _startNodeSelectorFilterCollection)
        {
            IEnumerable<Guid>? filtered = startNodeSelectorFilter.Filter(origin, startNodeFilter);

            if (filtered is not null)
            {
                return filtered;
            }
        }

        throw new NotSupportedException($"Did not find any filteres that could handle {startNodeFilter.DirectionAlias}");
    }

    internal Guid? CalculateOriginKey(StartNodeSelector startNodeSelector)
    {
        foreach (IStartNodeOriginFinder startNodeOriginFinder in _originFinderCollection)
        {
            Guid? originKey = startNodeOriginFinder.FindOriginKey(startNodeSelector);

            if (originKey is not null)
            {
                return originKey;
            }
        }

        return null;
    }
}

