namespace Umbraco.Cms.Core.Headless;

public class HeadlessContent : HeadlessElement, IHeadlessContent
{
    public HeadlessContent(Guid key, string? name, string contentType, string url, IDictionary<string, object?> properties)
        : base(key, contentType, properties)
    {
        Name = name;
        Url = url;
    }

    public string? Name { get; }

    public string Url { get; }
}
