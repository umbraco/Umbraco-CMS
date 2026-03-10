namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a link in the Delivery API.
/// </summary>
public sealed class ApiLink
{
    /// <summary>
    ///     Creates a link to content.
    /// </summary>
    /// <param name="title">The title of the link.</param>
    /// <param name="queryString">The query string of the link.</param>
    /// <param name="target">The target attribute of the link (e.g., "_blank").</param>
    /// <param name="destinationId">The unique identifier of the destination content.</param>
    /// <param name="destinationType">The content type alias of the destination.</param>
    /// <param name="route">The route information for the destination content.</param>
    /// <returns>A new <see cref="ApiLink" /> instance representing a content link.</returns>
    public static ApiLink Content(string title, string? queryString, string? target, Guid destinationId, string destinationType, IApiContentRoute route)
        => new(LinkType.Content, url: null, queryString, title, target, destinationId, destinationType, route);

    /// <summary>
    ///     Creates a link to media.
    /// </summary>
    /// <param name="title">The title of the link.</param>
    /// <param name="url">The URL of the media.</param>
    /// <param name="queryString">The query string of the link.</param>
    /// <param name="target">The target attribute of the link (e.g., "_blank").</param>
    /// <param name="destinationId">The unique identifier of the destination media.</param>
    /// <param name="destinationType">The media type alias of the destination.</param>
    /// <returns>A new <see cref="ApiLink" /> instance representing a media link.</returns>
    public static ApiLink Media(string title, string url, string? queryString, string? target, Guid destinationId, string destinationType)
        => new(LinkType.Media, url, queryString, title, target, destinationId, destinationType, route: null);

    /// <summary>
    ///     Creates an external link.
    /// </summary>
    /// <param name="title">The title of the link.</param>
    /// <param name="url">The external URL.</param>
    /// <param name="queryString">The query string of the link.</param>
    /// <param name="target">The target attribute of the link (e.g., "_blank").</param>
    /// <returns>A new <see cref="ApiLink" /> instance representing an external link.</returns>
    public static ApiLink External(string? title, string url, string? queryString, string? target)
        => new(LinkType.External, url, queryString, title, target, null, null, null);

    private ApiLink(LinkType linkType, string? url, string? queryString, string? title, string? target, Guid? destinationId, string? destinationType, IApiContentRoute? route)
    {
        LinkType = linkType;
        Url = url;
        QueryString = queryString;
        Title = title;
        Target = target;
        DestinationId = destinationId;
        DestinationType = destinationType;
        Route = route;
    }

    /// <summary>
    ///     Gets the URL of the link.
    /// </summary>
    public string? Url { get; }

    /// <summary>
    ///     Gets the query string of the link.
    /// </summary>
    public string? QueryString { get; }

    /// <summary>
    ///     Gets the title of the link.
    /// </summary>
    public string? Title { get; }

    /// <summary>
    ///     Gets the target attribute of the link (e.g., "_blank").
    /// </summary>
    public string? Target { get; }

    /// <summary>
    ///     Gets the unique identifier of the destination content or media.
    /// </summary>
    public Guid? DestinationId { get; }

    /// <summary>
    ///     Gets the content or media type alias of the destination.
    /// </summary>
    public string? DestinationType { get; }

    /// <summary>
    ///     Gets the route information for the destination content.
    /// </summary>
    public IApiContentRoute? Route { get; }

    /// <summary>
    ///     Gets the type of the link.
    /// </summary>
    public LinkType LinkType { get; }
}
