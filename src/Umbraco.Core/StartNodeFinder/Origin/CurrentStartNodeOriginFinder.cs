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
        if (selector.Context.CurrentKey.HasValue)
        {
            selector.OriginKey = selector.Context.CurrentKey;
            var baseResult = base.FindOriginKey(selector);

            if (baseResult is not null)
            {
                return baseResult;
            }
        }

        //If we cannot find the current Id, it should be okay to return null, as an unsaved item cannot have children anyway

        return null;
    }
}
