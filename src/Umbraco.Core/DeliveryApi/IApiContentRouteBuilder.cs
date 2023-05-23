using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiContentRouteBuilder
{
    IApiContentRoute? Build(IPublishedContent content, string? culture = null);
}
