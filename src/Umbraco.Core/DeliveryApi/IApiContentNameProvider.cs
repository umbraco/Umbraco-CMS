using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiContentNameProvider
{
    string GetName(IPublishedContent content);
}
