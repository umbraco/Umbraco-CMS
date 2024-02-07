using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class CurrentDynamicRootOriginFinder : ByKeyDynamicRootOriginFinder
{
    public CurrentDynamicRootOriginFinder(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override string SupportedOriginType { get; set; } = "Current";

    public override Guid? FindOriginKey(DynamicRootNodeQuery query)
    {
        query.OriginKey = query.Context.CurrentKey;
        var baseResult = base.FindOriginKey(query);

        return baseResult;
    }
}
