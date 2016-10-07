using LightInject;
using NUnit.Framework;
using Umbraco.Core.DI;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.DependencyInjection
{
    [TestFixture]
    public class TempTests
    {
        //[Test]
        public void Test()
        {
            var container = new ServiceContainer();
            container.ConfigureUmbracoCore();
            container.RegisterCollectionBuilder<UrlProviderCollectionBuilder>()
                .Append<DefaultUrlProvider>();
            var col = container.GetInstance<UrlProviderCollection>();
        }
    }
}
