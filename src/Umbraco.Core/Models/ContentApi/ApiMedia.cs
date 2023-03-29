namespace Umbraco.Cms.Core.Models.ContentApi;

public class ApiMedia : IApiMedia
{
    public ApiMedia(Guid id, string name, string mediaType, string url, IDictionary<string, object?> properties)
    {
        Id = id;
        Name = name;
        MediaType = mediaType;
        Url = url;
        Properties = properties;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string MediaType { get; }

    public string Url { get; }

    public IDictionary<string, object?> Properties { get; }
}
