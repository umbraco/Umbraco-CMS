using System.Linq;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Tests.Common;
using Umbraco.Tests.Common.Builders;
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
            Composition.Register<ISiteDomainHelper, SiteDomainHelper>();
        }

        protected override void ComposeSettings()
        {
            var contentSettings = new ContentSettingsBuilder().Build();
            var userPasswordConfigurationSettings = new UserPasswordConfigurationSettingsBuilder().Build();

            Composition.Register(x => Microsoft.Extensions.Options.Options.Create(contentSettings));
            Composition.Register(x => Microsoft.Extensions.Options.Options.Create(userPasswordConfigurationSettings));

            // TODO: remove this once legacy config is fully extracted.
            Composition.Configs.Add(TestHelpers.SettingsForTests.GenerateMockContentSettings);
            Composition.Configs.Add(TestHelpers.SettingsForTests.GenerateMockGlobalSettings);
        }

        protected IPublishedUrlProvider GetPublishedUrlProvider(IUmbracoContext umbracoContext, DefaultUrlProvider urlProvider)
        {
            var webRoutingSettings = new WebRoutingSettingsBuilder().Build();
            return new UrlProvider(
                new TestUmbracoContextAccessor(umbracoContext),
                Microsoft.Extensions.Options.Options.Create(webRoutingSettings),
                new UrlProviderCollection(new[] { urlProvider }),
                new MediaUrlProviderCollection(Enumerable.Empty<IMediaUrlProvider>()),
                Mock.Of<IVariationContextAccessor>());
        }
    }
}
