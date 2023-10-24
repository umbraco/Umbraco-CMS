namespace Umbraco.Cms.Core.StartNodeFinder.Filters;

public interface IStartNodeFilterRepository
{
    IEnumerable<Guid> NearestAncestorOrSelf(IEnumerable<Guid> origins, StartNodeFilter filter);
}
