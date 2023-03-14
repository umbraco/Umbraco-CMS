namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     The direction of a route
/// </summary>
public enum RouteDirection
{
    /// <summary>
    ///     An inbound route used to map a URL to a content item
    /// </summary>
    Inbound = 1,

    /// <summary>
    ///     An outbound route used to generate a URL for a content item
    /// </summary>
    Outbound = 2,
}
