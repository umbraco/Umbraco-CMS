namespace Umbraco.Cms.Core.Models.DeliveryApi;

public class ApiElement : IApiElement
{
    public ApiElement(Guid id, string contentType, IDictionary<string, object?> properties)
    {
        Id = id;
        ContentType = contentType;
        Properties = properties;
    }

    public Guid Id { get; }

    public string ContentType { get; }

    public IDictionary<string, object?> Properties { get; }
}
