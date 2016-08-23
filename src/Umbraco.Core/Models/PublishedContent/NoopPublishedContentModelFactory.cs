namespace Umbraco.Core.Models.PublishedContent
{
    public class NoopPublishedContentModelFactory : IPublishedContentModelFactory
    {
        public IPublishedContent CreateModel(IPublishedContent content)
        {
            return content;
        }
    }
}
