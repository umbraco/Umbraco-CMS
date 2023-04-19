namespace Umbraco.Cms.Core.Models.DeliveryApi;

public interface IApiContentResponse : IApiContent
{
    IDictionary<string, IApiContentRoute> Cultures { get; }
}
