namespace Umbraco.Core.Models.PublishedContent
{
    public class NoopPublishedContentModelFactory : IPublishedContentModelFactory
    {
        public IPublishedContent CreateModel(IPublishedContent content)
        {
            return content;
        }

        public T CreateModel<T>(IPropertySet content)
        {
            throw new System.NotImplementedException();
        }
    }
}
