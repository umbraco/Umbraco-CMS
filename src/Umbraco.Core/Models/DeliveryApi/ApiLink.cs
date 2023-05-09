namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class ApiLink
{
    public static ApiLink Content(string title, string? target, Guid destinationId, string destinationType, IApiContentRoute route)
        => new(LinkType.Content, null, title, target, destinationId, destinationType, route);

    public static ApiLink Media(string title, string url, string? target, Guid destinationId, string destinationType)
        => new(LinkType.Media, url, title, target, destinationId, destinationType, null);

    public static ApiLink External(string? title, string url, string? target)
        => new(LinkType.External, url, title, target, null, null, null);

    private ApiLink(LinkType linkType, string? url, string? title, string? target, Guid? destinationId, string? destinationType, IApiContentRoute? route)
    {
        LinkType = linkType;
        Url = url;
        Title = title;
        Target = target;
        DestinationId = destinationId;
        DestinationType = destinationType;
        Route = route;
    }

    public string? Url { get; }

    public string? Title { get; }

    public string? Target { get; }

    public Guid? DestinationId { get; }

    public string? DestinationType { get; }

    public IApiContentRoute? Route { get; }

    public LinkType LinkType { get; }
}
