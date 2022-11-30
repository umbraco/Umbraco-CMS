using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Headless;

public interface IHeadlessContentNameProvider
{
    string? GetName(IPublishedContent content);
}
