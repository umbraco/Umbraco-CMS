namespace Umbraco.Cms.Core.Models.ContentApi;

public class ApiContent : ApiElement, IApiContent
{
    public ApiContent(Guid key, string? name, string contentType, string url, IDictionary<string, object?> properties)
        : base(key, contentType, properties)
    {
        Name = name;
        Url = url;
    }

    public string? Name { get; }

    public string Url { get; }
}
