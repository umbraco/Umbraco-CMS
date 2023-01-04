using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi
{
    public class PublishedContentNameProvider : IPublishedContentNameProvider
    {
        public string GetName(IPublishedContent content) => content.Name;
    }
}
