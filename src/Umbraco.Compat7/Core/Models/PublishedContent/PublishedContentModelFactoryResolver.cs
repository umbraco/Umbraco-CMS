using Umbraco.Core.Composing;
using CoreCurrent = Umbraco.Core.Composing.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Models.PublishedContent
{
    public class PublishedContentModelFactoryResolver
    {
        private PublishedContentModelFactoryResolver()
        { }

        public static PublishedContentModelFactoryResolver Current { get; } = new PublishedContentModelFactoryResolver();

        public static bool HasCurrent => true;

        public void SetFactory(IPublishedModelFactory factory)
        {
            CoreCurrent.Container.RegisterSingleton(_ => factory);
        }

        public IPublishedModelFactory Factory => CoreCurrent.PublishedModelFactory;
    }
}
