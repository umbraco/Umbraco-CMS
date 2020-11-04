using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Tests.Common;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    public abstract class BaseUrlProviderTest : BaseWebTest
    {
        protected IUmbracoContextAccessor UmbracoContextAccessor { get; } = new TestUmbracoContextAccessor();

        protected abstract bool HideTopLevelNodeFromPath { get; }

        protected override void Compose()
        {
            base.Compose();
            Composition.Services.AddTransient<ISiteDomainHelper, SiteDomainHelper>();
        }

        protected override void ComposeSettings()
        {
            var contentSettings = new ContentSettings();
            var userPasswordConfigurationSettings = new UserPasswordConfigurationSettings();

            Composition.Services.AddTransient(x => Microsoft.Extensions.Options.Options.Create(contentSettings));
            Composition.Services.AddTransient(x => Microsoft.Extensions.Options.Options.Create(userPasswordConfigurationSettings));
        }

        protected IPublishedUrlProvider GetPublishedUrlProvider(IUmbracoContext umbracoContext, DefaultUrlProvider urlProvider)
        {
            var webRoutingSettings = new WebRoutingSettings();
            return new UrlProvider(
                new TestUmbracoContextAccessor(umbracoContext),
                Microsoft.Extensions.Options.Options.Create(webRoutingSettings),
                new UrlProviderCollection(new[] { urlProvider }),
                new MediaUrlProviderCollection(Enumerable.Empty<IMediaUrlProvider>()),
                Mock.Of<IVariationContextAccessor>());
        }
    }
}
