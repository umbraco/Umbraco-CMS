namespace Umbraco.Cms.Core.StartNodeFinder.Filters;

public class NearestAncestorOrSelfStartNodeSelectorFilter : IStartNodeSelectorFilter
{

    protected virtual string SupportedDirectionAlias { get; set; } = "NearestAncestorOrSelf";
    public IEnumerable<Guid>? Filter(IEnumerable<Guid> origins, StartNodeFilter filter)
    {
        if (filter.DirectionAlias != SupportedDirectionAlias || origins.Any() is false)
        {
            return null;
        }

        // TODO go find NearestAncestorOrSelf of the id's provided

        return null;
    }
}
