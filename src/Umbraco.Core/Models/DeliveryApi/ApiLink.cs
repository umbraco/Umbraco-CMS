namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class ApiLink
{
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public static ApiLink Content(string title, string? queryString, string? target, Guid destinationId, string destinationType, IApiContentRoute route)
        => new(LinkType.Content, url: null, queryString, title, target, destinationId, destinationType, route);
    public static ApiLink Content(string title, string? queryString, string? target, Guid destinationId, string destinationType, IApiContentRoute route, string? culture = null)
       => new(LinkType.Content, url: null, queryString, title, target, destinationId, destinationType, route, culture);

    public static ApiLink Media(string title, string url, string? queryString, string? target, Guid destinationId, string destinationType)
        => new(LinkType.Media, url, queryString, title, target, destinationId, destinationType, route: null);

    public static ApiLink External(string? title, string url, string? queryString, string? target)
        => new(LinkType.External, url, queryString, title, target, null, null, null);

    private ApiLink(LinkType linkType, string? url, string? queryString, string? title, string? target, Guid? destinationId, string? destinationType, IApiContentRoute? route, string? culture = null)
    {
        LinkType = linkType;
        Url = url;
        QueryString = queryString;
        Title = title;
        Target = target;
        DestinationId = destinationId;
        DestinationType = destinationType;
        Route = route;
        Culture = culture;
    }

    public string? Url { get; }

    public string? QueryString { get; }

    public string? Title { get; }

    public string? Target { get; }

    public Guid? DestinationId { get; }

    public string? DestinationType { get; }

    public IApiContentRoute? Route { get; }

    public LinkType LinkType { get; }

    public string? Culture { get; }
}
