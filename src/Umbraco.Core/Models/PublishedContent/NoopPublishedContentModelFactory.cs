namespace Umbraco.Core.Models.PublishedContent
{
    public class NoopPublishedContentModelFactory : IPublishedContentModelFactory
    {
        public IPublishedContent CreateModel(IPublishedContent content)
        {
            return content;
        }

        public T CreateModel<T>(IPublishedFragment content)
        {
            throw new System.NotImplementedException();
        }
    }
}
