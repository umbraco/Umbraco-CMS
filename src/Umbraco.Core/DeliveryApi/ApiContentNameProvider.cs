using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiContentNameProvider : IApiContentNameProvider
{
    public string GetName(IPublishedContent content) => content.Name;
}
