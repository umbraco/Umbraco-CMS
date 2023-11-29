using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiMediaUrlProvider
{
    string GetUrl(IPublishedContent media);
}
