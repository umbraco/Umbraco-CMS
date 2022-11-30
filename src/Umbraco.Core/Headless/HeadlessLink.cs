using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Headless;

public class HeadlessLink
{
    public HeadlessLink(string url, string? title, string? target, Guid? key, string? destinationType, LinkType linkType)
    {
        Url = url;
        Title = title;
        Target = target;
        Key = key;
        DestinationType = destinationType;
        LinkType = linkType;
    }

    public string Url { get; }

    public string? Title { get; }

    public string? Target { get; }

    public Guid? Key { get; }

    public string? DestinationType { get; }

    public LinkType LinkType { get; }
}
