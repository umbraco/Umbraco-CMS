using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.StartNodeFinder.Origin;

public class ParentStartNodeOriginFinder : ByKeyStartNodeOriginFinder
{
    public ParentStartNodeOriginFinder(IEntityService entityService) : base(entityService)
    {
    }

    protected override string SupportedOriginType { get; set; } = "Parent";
    public override Guid? FindOriginKey(StartNodeSelector selector)
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
