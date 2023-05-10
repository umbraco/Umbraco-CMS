namespace Umbraco.Cms.Core.Models.DeliveryApi;

public interface IApiMedia
{
    public Guid Id { get; }

    public string Name { get; }

    public string MediaType { get; }

    public string Url { get; }

    public IDictionary<string, object?> Properties { get; }
}
