using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class ParentOrSelfDynamicRootOriginFinder : ParentDynamicRootOriginFinder
{
    public ParentOrSelfDynamicRootOriginFinder(IEntityService entityService) : base(entityService)
    {
    }

    protected override string SupportedOriginType { get; set; } = "ParentOrSelf";

    public override Guid? FindOriginKey(DynamicRootNodeQuery query)
    {
        var baseResult = base.FindOriginKey(query);
        if(baseResult is not null || query.Context.CurrentKey is null)
        {
            return baseResult;
        }
        query.OriginKey = query.Context.CurrentKey;
        return base.FindOriginKey(query);
    }
}
