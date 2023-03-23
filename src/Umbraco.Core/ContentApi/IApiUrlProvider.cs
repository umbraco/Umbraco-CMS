using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public interface IApiUrlProvider
{
    string Url(IPublishedContent content);
}
