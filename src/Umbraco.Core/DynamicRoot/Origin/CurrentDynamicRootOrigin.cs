using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class CurrentDynamicRootOrigin : ByKeyDynamicRootOrigin
{
    public CurrentDynamicRootOrigin(IEntityService entityService) : base(entityService)
    {
    }

    protected override string SupportedOriginType { get; set; } = "Current";
    public override Guid? FindOriginKey(DynamicRootNodeSelector selector)
    {
        selector.OriginKey = selector.Context.CurrentKey;
        var baseResult = base.FindOriginKey(selector);

        if (baseResult is not null)
        {
            return baseResult;
        }

        return null;
    }
}
