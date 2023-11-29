namespace Umbraco.Cms.Core.Models.DeliveryApi;

public interface IApiMedia
{
    Guid Id { get; }

    string Name { get; }

    string MediaType { get; }

    string Url { get; }

    string? Extension { get; }

    int? Width { get; }

    int? Height { get; }

    int? Bytes { get; }

    IDictionary<string, object?> Properties { get; }
}
