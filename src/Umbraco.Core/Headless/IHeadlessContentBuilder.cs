using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Headless;

public interface IHeadlessContentBuilder
{
    IHeadlessContent Build(IPublishedContent content);
}
