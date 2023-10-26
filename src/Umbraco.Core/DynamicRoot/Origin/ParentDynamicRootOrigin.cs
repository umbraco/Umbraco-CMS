using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class ParentDynamicRootOrigin : ByKeyDynamicRootOrigin
{
    public ParentDynamicRootOrigin(IEntityService entityService) : base(entityService)
    {
    }

    protected override string SupportedOriginType { get; set; } = "Parent";
    public override Guid? FindOriginKey(DynamicRootNodeSelector selector)
    {
        selector.OriginKey = selector.Context.ParentKey;
        var baseResult = base.FindOriginKey(selector);

        if (baseResult is not null)
        {
            return baseResult;
        }

        return null;
    }
}
