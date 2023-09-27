namespace Umbraco.Cms.Core.StartNodeFinder.Filters;

public interface IStartNodeSelectorFilter
{
    IEnumerable<Guid>? Filter(IEnumerable<Guid> origins, StartNodeFilter filter);
}
