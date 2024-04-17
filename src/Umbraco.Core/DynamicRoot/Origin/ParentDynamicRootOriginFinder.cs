using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class ParentDynamicRootOriginFinder : ByKeyDynamicRootOriginFinder
{
    public ParentDynamicRootOriginFinder(IEntityService entityService) : base(entityService)
    {
    }

    protected override string SupportedOriginType { get; set; } = "Parent";

    public override Guid? FindOriginKey(DynamicRootNodeQuery query)
    {
        query.OriginKey = query.Context.ParentKey;
        var baseResult = base.FindOriginKey(query);

        return baseResult;
    }
}
