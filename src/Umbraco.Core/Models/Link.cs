namespace Umbraco.Cms.Core.Models;

public class Link
{
    public string? Name { get; set; }

    public string? Target { get; set; }

    public LinkType Type { get; set; }

    public Udi? Udi { get; set; }

    public string? Url { get; set; }
}
