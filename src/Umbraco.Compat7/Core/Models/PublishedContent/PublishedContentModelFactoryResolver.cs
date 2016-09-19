using Umbraco.Core.DependencyInjection;
using CoreCurrent = Umbraco.Core.DependencyInjection.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Models.PublishedContent
{
    public class PublishedContentModelFactoryResolver
    {
        public static PublishedContentModelFactoryResolver Current { get; set; } = new PublishedContentModelFactoryResolver();

        public void SetFactory(IPublishedContentModelFactory factory)
        {
            CoreCurrent.Container.RegisterSingleton(_ => factory);
        }

        public IPublishedContentModelFactory Factory => CoreCurrent.PublishedContentModelFactory;
    }
}
