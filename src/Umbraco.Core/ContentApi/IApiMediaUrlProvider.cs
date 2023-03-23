using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public interface IApiMediaUrlProvider
{
    string GetUrl(IPublishedContent media);
}
