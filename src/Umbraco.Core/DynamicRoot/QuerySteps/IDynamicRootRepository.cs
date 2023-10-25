namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public interface IDynamicRootRepository
{
    Guid? NearestAncestorOrSelf(IEnumerable<Guid> origins, DynamicRootQueryStep queryStep);
    Guid? FarthestAncestorOrSelf(IEnumerable<Guid> origins, DynamicRootQueryStep queryStep);
    IEnumerable<Guid> NearestDescendantOrSelf(IEnumerable<Guid> origins, DynamicRootQueryStep queryStep);
    IEnumerable<Guid> FarthestDescendantOrSelf(IEnumerable<Guid> origins, DynamicRootQueryStep queryStep);
}
