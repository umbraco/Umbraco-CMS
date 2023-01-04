using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public interface IPublishedContentNameProvider
{
    string GetName(IPublishedContent content);
}
