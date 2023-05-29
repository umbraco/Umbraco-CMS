namespace Umbraco.Cms.Core.Models.DeliveryApi;

public interface IApiElement
{
    Guid Id { get; }

    string ContentType { get; }

    IDictionary<string, object?> Properties { get; }
}
