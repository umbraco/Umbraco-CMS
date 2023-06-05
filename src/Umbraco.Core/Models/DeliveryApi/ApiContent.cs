namespace Umbraco.Cms.Core.Models.DeliveryApi;

public class ApiContent : ApiElement, IApiContent
{
    public ApiContent(Guid id, string name, string contentType, IApiContentRoute route, IDictionary<string, object?> properties)
        : base(id, contentType, properties)
    {
        Name = name;
        Route = route;
    }

    public string Name { get; }

    public IApiContentRoute Route { get; }
}
