using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing
{
    [TestFixture]
    public class GetContentUrlsTests : PublishedSnapshotServiceTestBase
    {
        private WebRoutingSettings _webRoutingSettings;
        private RequestHandlerSettings _requestHandlerSettings;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _webRoutingSettings = new WebRoutingSettings();
            _requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };

            GlobalSettings.HideTopLevelNodeFromPath = false;

            string xml = PublishedContentXml.BaseWebTestXml(1234);

            IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
                xml,
                TestHelper.ShortStringHelper,
                out ContentType[] contentTypes,
                out DataType[] dataTypes).ToList();

            InitializedCache(kits, contentTypes, dataTypes: dataTypes);
        }

        private ILocalizedTextService GetTextService()
        {
            var textService = new Mock<ILocalizedTextService>();
            textService.Setup(x => x.Localize(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                         It.IsAny<CultureInfo>(),
                         It.IsAny<IDictionary<string, string>>()
                         ))
                .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args)
                => $"{key}/{alias}");

            return textService.Object;
        }

        private ILocalizationService GetLangService(params string[] isoCodes)
        {
            var allLangs = isoCodes
                .Select(CultureInfo.GetCultureInfo)
                .Select(culture => new Language(GlobalSettings, culture.Name)
                {
                    CultureName = culture.DisplayName,
                    IsDefault = true,
                    IsMandatory = true
                }).ToArray();

            var langService = Mock.Of<ILocalizationService>(x => x.GetAllLanguages() == allLangs);
            return langService;
        }

        [Test]
        public async Task Content_Not_Published()
        {
            var contentType = ContentTypeBuilder.CreateBasicContentType();
            var content = ContentBuilder.CreateBasicContent(contentType);
            content.Id = 1046; // FIXME: we are using this ID only because it's built into the test XML published cache
            content.Path = "-1,1046";

            var umbracoContextAccessor = GetUmbracoContextAccessor("http://localhost:8000");
            var publishedRouter = CreatePublishedRouter(
                umbracoContextAccessor,
                new[] { new ContentFinderByUrl(Mock.Of<ILogger<ContentFinderByUrl>>(), umbracoContextAccessor) });
            var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();

            UrlProvider urlProvider = GetUrlProvider(umbracoContextAccessor, _requestHandlerSettings, _webRoutingSettings, out UriUtility uriUtility);

            var urls = (await content.GetContentUrlsAsync(
                publishedRouter,
                umbracoContext,
                GetLangService("en-US", "fr-FR"),
                GetTextService(),
                Mock.Of<IContentService>(),
                VariationContextAccessor,
                Mock.Of<ILogger<IContent>>(),
                uriUtility,
                urlProvider)).ToList();

            Assert.AreEqual(1, urls.Count);
            Assert.AreEqual("content/itemNotPublished", urls[0].Text);
            Assert.IsFalse(urls[0].IsUrl);
        }

        [Test]
        public async Task Invariant_Root_Content_Published_No_Domains()
        {
            var contentType = ContentTypeBuilder.CreateBasicContentType();
            var content = ContentBuilder.CreateBasicContent(contentType);
            content.Id = 1046; // FIXME: we are using this ID only because it's built into the test XML published cache
            content.Path = "-1,1046";
            content.Published = true;

            var umbracoContextAccessor = GetUmbracoContextAccessor("http://localhost:8000");
            var publishedRouter = CreatePublishedRouter(
                umbracoContextAccessor,
                new[] { new ContentFinderByUrl(Mock.Of<ILogger<ContentFinderByUrl>>(), umbracoContextAccessor) });
            var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();

            UrlProvider urlProvider = GetUrlProvider(umbracoContextAccessor, _requestHandlerSettings, _webRoutingSettings, out UriUtility uriUtility);

            var urls = (await content.GetContentUrlsAsync(
                publishedRouter,
                umbracoContext,
                GetLangService("en-US", "fr-FR"),
                GetTextService(),
                Mock.Of<IContentService>(),
                VariationContextAccessor,
                Mock.Of<ILogger<IContent>>(),
                uriUtility,
                urlProvider)).ToList();

            Assert.AreEqual(1, urls.Count);
            Assert.AreEqual("/home/", urls[0].Text);
            Assert.AreEqual("en-US", urls[0].Culture);
            Assert.IsTrue(urls[0].IsUrl);
        }

        [Test]
        public async Task Invariant_Child_Content_Published_No_Domains()
        {
            var contentType = ContentTypeBuilder.CreateBasicContentType();
            var parent = ContentBuilder.CreateBasicContent(contentType);
            parent.Id = 1046; // FIXME: we are using this ID only because it's built into the test XML published cache
            parent.Name = "home";
            parent.Path = "-1,1046";
            parent.Published = true;
            var child = ContentBuilder.CreateBasicContent(contentType);
            child.Name = "sub1";
            child.Id = 1173; // FIXME: we are using this ID only because it's built into the test XML published cache
            child.Path = "-1,1046,1173";
            child.Published = true;


            var umbracoContextAccessor = GetUmbracoContextAccessor("http://localhost:8000");
            var publishedRouter = CreatePublishedRouter(
                umbracoContextAccessor,
                new[] { new ContentFinderByUrl(Mock.Of<ILogger<ContentFinderByUrl>>(), umbracoContextAccessor) });
            var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();


            UrlProvider urlProvider = GetUrlProvider(umbracoContextAccessor, _requestHandlerSettings, _webRoutingSettings, out UriUtility uriUtility);

            var urls = (await child.GetContentUrlsAsync(
                publishedRouter,
                umbracoContext,
                GetLangService("en-US", "fr-FR"),
                GetTextService(),
                Mock.Of<IContentService>(),
                VariationContextAccessor,
                Mock.Of<ILogger<IContent>>(),
                uriUtility,
                urlProvider)).ToList();

            Assert.AreEqual(1, urls.Count);
            Assert.AreEqual("/home/sub1/", urls[0].Text);
            Assert.AreEqual("en-US", urls[0].Culture);
            Assert.IsTrue(urls[0].IsUrl);
        }

        // TODO: We need a lot of tests here, the above was just to get started with being able to unit test this method
        // * variant URLs without domains assigned, what happens?
        // * variant URLs with domains assigned, but also having more languages installed than there are domains/cultures assigned
        // * variant URLs with an ancestor culture unpublished
        // * invariant URLs with ancestors as variants
        // * ... probably a lot more

    }
}
