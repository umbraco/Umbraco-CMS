namespace Umbraco.Cms.Core.Routing;

public enum UmbracoRouteResult
{
    /// <summary>
    ///     Routing was successful and a content item was matched
    /// </summary>
    Success,

    /// <summary>
    ///     A redirection took place
    /// </summary>
    Redirect,

    /// <summary>
    ///     Nothing matched
    /// </summary>
    NotFound,
}
