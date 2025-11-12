using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

[JsonDerivedType(typeof(ApiContentResponse))]
public interface IApiContentResponse : IApiContent
{
    IDictionary<string, IApiContentRoute> Cultures { get; }
}
