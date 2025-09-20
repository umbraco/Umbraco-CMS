namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class ApiLink
{
    [Obsolete("Please use the overload that accepts a query string. Will be removed in V14.")]
    public static ApiLink Content(string title, string? target, Guid destinationId, string destinationType, IApiContentRoute route)
        => Content(title, queryString: null, target, destinationId, destinationType, route);

    public static ApiLink Content(string title, string? queryString, string? target, Guid destinationId, string destinationType, IApiContentRoute route)
        => new(LinkType.Content, url: null, queryString, title, target, destinationId, destinationType, route);

    [Obsolete("Please use the overload that accepts a query string. Will be removed in V14.")]
    public static ApiLink Media(string title, string url, string? target, Guid destinationId, string destinationType)
        => Media(title, url, queryString: null, target, destinationId, destinationType);

    public static ApiLink Media(string title, string url, string? queryString, string? target, Guid destinationId, string destinationType)
        => new(LinkType.Media, url, queryString, title, target, destinationId, destinationType, route: null);

    [Obsolete("Please use the overload that accepts a query string. Will be removed in V14.")]
    public static ApiLink External(string? title, string url, string? target)
        => External(title, url, queryString: null, target);

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

    public string? Url { get; }

    public string? QueryString { get; }

    public string? Title { get; }

    public string? Target { get; }

    public Guid? DestinationId { get; }

    public string? DestinationType { get; }

    public IApiContentRoute? Route { get; }

    public LinkType LinkType { get; }
}
