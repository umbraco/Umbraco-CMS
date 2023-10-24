namespace Umbraco.Cms.Core.StartNodeFinder.Filters;

public interface IStartNodeFilterRepository
{
    Guid? NearestAncestorOrSelf(IEnumerable<Guid> origins, StartNodeFilter filter);
    Guid? FarthestAncestorOrSelf(IEnumerable<Guid> origins, StartNodeFilter filter);
    IEnumerable<Guid> NearestDescendantOrSelf(IEnumerable<Guid> origins, StartNodeFilter filter);
    IEnumerable<Guid> FarthestDescendantOrSelf(IEnumerable<Guid> origins, StartNodeFilter filter);
}
