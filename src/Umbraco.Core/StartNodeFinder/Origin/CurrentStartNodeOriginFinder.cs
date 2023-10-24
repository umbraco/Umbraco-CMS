using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.StartNodeFinder.Origin;

public class CurrentStartNodeOriginFinder : ByKeyStartNodeOriginFinder
{
    public CurrentStartNodeOriginFinder(IEntityService entityService) : base(entityService)
    {
    }

    protected override string SupportedOriginType { get; set; } = "Current";
    public override Guid? FindOriginKey(StartNodeSelector selector)
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
