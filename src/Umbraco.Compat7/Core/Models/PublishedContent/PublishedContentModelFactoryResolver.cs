using Umbraco.Core.DI;
using CoreCurrent = Umbraco.Core.DI.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Models.PublishedContent
{
    public class PublishedContentModelFactoryResolver
    {
        private PublishedContentModelFactoryResolver()
        { }

        public static PublishedContentModelFactoryResolver Current { get; } = new PublishedContentModelFactoryResolver();

        public static bool HasCurrent => true;

        public void SetFactory(IPublishedContentModelFactory factory)
        {
            CoreCurrent.Container.RegisterSingleton(_ => factory);
        }

        public IPublishedContentModelFactory Factory => CoreCurrent.PublishedContentModelFactory;
    }
}
