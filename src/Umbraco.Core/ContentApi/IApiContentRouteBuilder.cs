using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public interface IApiContentRouteBuilder
{
    IApiContentRoute Build(IPublishedContent content, string? culture = null);
}
