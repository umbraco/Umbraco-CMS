namespace Umbraco.Cms.Core.Models.ContentApi;

public class ApiContent : ApiElement, IApiContent
{
    public ApiContent(Guid id, string name, string contentType, string url, IDictionary<string, object?> properties)
        : base(id, contentType, properties)
    {
        Name = name;
        Url = url;
    }

    public string Name { get; }

    public string Url { get; }
}
