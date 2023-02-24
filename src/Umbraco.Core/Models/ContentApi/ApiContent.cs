namespace Umbraco.Cms.Core.Models.ContentApi;

public class ApiContent : ApiElement, IApiContent
{
    public ApiContent(Guid id, string name, string contentType, string path, IDictionary<string, object?> properties)
        : base(id, contentType, properties)
    {
        Name = name;
        Path = path;
    }

    public string Name { get; }

    public string Path { get; }
}
