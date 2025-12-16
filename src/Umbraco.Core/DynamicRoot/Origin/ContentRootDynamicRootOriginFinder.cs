namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class ContentRootDynamicRootOriginFinder : IDynamicRootOriginFinder
{
    protected virtual string SupportedOriginType { get; set; } = "ContentRoot";

    public virtual Guid? FindOriginKey(DynamicRootNodeQuery query)
    {
        if (query.OriginAlias != SupportedOriginType)
        {
            return null;
        }

        return Constants.System.RootSystemKey;
    }
}
