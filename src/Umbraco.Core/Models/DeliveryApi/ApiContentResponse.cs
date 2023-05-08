using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

public class ApiContentResponse : ApiContent, IApiContentResponse
{
    public ApiContentResponse(Guid id, string name, string contentType, IApiContentRoute route, IDictionary<string, object?> properties, IDictionary<string, IApiContentRoute> cultures)
        : base(id, name, contentType, route, properties)
        => Cultures = cultures;

    // a little DX; by default this dictionary will be serialized as the first part of the response due to the inner workings of the serializer.
    // that's rather confusing to see as the very first thing when you get a response from the API, so let's move it downwards.
    // hopefully some day System.Text.Json will be able to order the properties differently in a centralized way. for now we have to live
    // with this annotation :(
    [JsonPropertyOrder(100)]
    public IDictionary<string, IApiContentRoute> Cultures { get; }
}
