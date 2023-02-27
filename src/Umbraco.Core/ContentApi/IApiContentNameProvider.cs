using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public interface IApiContentNameProvider
{
    string GetName(IPublishedContent content);
}
