namespace Umbraco.Cms.Core.DynamicRoot.Origin;

/// <summary>
///     An origin finder that returns the content root (system root) as the origin.
///     This finder handles the "ContentRoot" origin type.
/// </summary>
public class ContentRootDynamicRootOriginFinder : IDynamicRootOriginFinder
{
    /// <summary>
    ///     Gets or sets the origin type alias that this finder supports.
    /// </summary>
    protected virtual string SupportedOriginType { get; set; } = "ContentRoot";

    /// <inheritdoc/>
    public virtual Guid? FindOriginKey(DynamicRootNodeQuery query)
    {
        if (query.OriginAlias != SupportedOriginType)
        {
            return null;
        }

        return Constants.System.RootSystemKey;
    }
}
