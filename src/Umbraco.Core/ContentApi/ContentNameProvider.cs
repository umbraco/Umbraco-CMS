using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi
{
    public class ContentNameProvider : IContentNameProvider
    {
        public string GetName(IPublishedContent content) => content.Name;
    }
}
