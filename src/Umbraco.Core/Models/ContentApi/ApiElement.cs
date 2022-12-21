namespace Umbraco.Cms.Core.Models.ContentApi;

public class ApiElement : IApiElement
{
    public ApiElement(Guid key, string contentType, IDictionary<string, object?> properties)
    {
        Key = key;
        ContentType = contentType;
        Properties = properties;
    }

    public Guid Key { get; }

    public string ContentType { get; }

    public IDictionary<string, object?> Properties { get; }
}
