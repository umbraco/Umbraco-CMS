using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Headless;

public interface IHeadlessElementBuilder
{
    IHeadlessElement Build(IPublishedElement element);
}
