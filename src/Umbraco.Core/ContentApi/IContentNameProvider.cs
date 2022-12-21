using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public interface IContentNameProvider
{
    string? GetName(IPublishedContent content);
}
