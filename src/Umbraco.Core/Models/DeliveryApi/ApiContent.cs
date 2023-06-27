namespace Umbraco.Cms.Core.Models.DeliveryApi;

public class ApiContent : ApiElement, IApiContent
{
    public ApiContent(Guid id, string name, string contentType, DateTime createDate, DateTime updateDate, IApiContentRoute route, IDictionary<string, object?> properties)
        : base(id, contentType, properties)
    {
        Name = name;
        CreateDate = createDate;
        UpdateDate = updateDate;
        Route = route;
    }

    public string Name { get; }

    public DateTime CreateDate { get; }

    public DateTime UpdateDate { get; }

    public IApiContentRoute Route { get; }
}
