using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Headless
{
    public class HeadlessContentNameProvider : IHeadlessContentNameProvider
    {
        public string? GetName(IPublishedContent content) => content.Name;
    }
}
