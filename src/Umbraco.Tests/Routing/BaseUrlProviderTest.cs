using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Routing
{
    public abstract class BaseUrlProviderTest : BaseWebTest
    {
        protected IUmbracoContextAccessor UmbracoContextAccessor { get; } = new TestUmbracoContextAccessor();

        protected abstract bool HideTopLevelNodeFromPath { get; }

        protected override void Compose()
        {
            base.Compose();
            Builder.Services.AddTransient<ISiteDomainMapper, SiteDomainMapper>();
        }

        protected override void ComposeSettings()
        {
            var contentSettings = new ContentSettings();
            var userPasswordConfigurationSettings = new UserPasswordConfigurationSettings();

            Builder.Services.AddTransient(x => Microsoft.Extensions.Options.Options.Create(contentSettings));
            Builder.Services.AddTransient(x => Microsoft.Extensions.Options.Options.Create(userPasswordConfigurationSettings));
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
