using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Extensions;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class UrlProviderWithHideTopLevelNodeFromPathTests : BaseUrlProviderTest
    {
        private GlobalSettings _globalSettings;

        public override void SetUp()
        {
            _globalSettings = new GlobalSettings { HideTopLevelNodeFromPath = HideTopLevelNodeFromPath };
            base.SetUp();
            PublishedSnapshotService = CreatePublishedSnapshotService(_globalSettings);


        }

        protected override bool HideTopLevelNodeFromPath => true;

        protected override void ComposeSettings()
        {
            base.ComposeSettings();
            Builder.Services.AddUnique(x => Microsoft.Extensions.Options.Options.Create(_globalSettings));
        }

        [TestCase(1046, "/")]
        [TestCase(1173, "/sub1/")]
        [TestCase(1174, "/sub1/sub2/")]
        [TestCase(1176, "/sub1/sub-3/")]
        [TestCase(1177, "/sub1/custom-sub-1/")]
        [TestCase(1178, "/sub1/custom-sub-2/")]
        [TestCase(1175, "/sub-2/")]
        [TestCase(1172, "/test-page/")] // not hidden because not first root
        public void Get_Url_Hiding_Top_Level(int nodeId, string niceUrlMatch)
        {
            var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };

            var umbracoContext = GetUmbracoContext("/test", 1111, globalSettings: _globalSettings, snapshotService:PublishedSnapshotService);
            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);
            var urlProvider = new DefaultUrlProvider(
                Microsoft.Extensions.Options.Options.Create(requestHandlerSettings),
                LoggerFactory.CreateLogger<DefaultUrlProvider>(),
                new SiteDomainHelper(), umbracoContextAccessor, UriUtility);
            var publishedUrlProvider = GetPublishedUrlProvider(umbracoContext, urlProvider);

            var result = publishedUrlProvider.GetUrl(nodeId);
            Assert.AreEqual(niceUrlMatch, result);
        }
    }
}
