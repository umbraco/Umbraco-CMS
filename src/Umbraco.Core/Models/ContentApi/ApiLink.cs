namespace Umbraco.Cms.Core.Models.ContentApi;

public class ApiLink
{
    public ApiLink(string url, string? title, string? target, Guid? contentId, string? destinationType, LinkType linkType)
    {
        Url = url;
        Title = title;
        Target = target;
        ContentId = contentId;
        DestinationType = destinationType;
        LinkType = linkType;
    }

    public string Url { get; }

    public string? Title { get; }

    public string? Target { get; }

    public Guid? ContentId { get; }

    public string? DestinationType { get; }

    public LinkType LinkType { get; }
}
