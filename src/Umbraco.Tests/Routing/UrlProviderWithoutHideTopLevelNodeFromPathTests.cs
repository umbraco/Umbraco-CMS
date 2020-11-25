using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Tests.Common;
using Umbraco.Tests.LegacyXmlPublishedCache;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.Testing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class UrlProviderWithoutHideTopLevelNodeFromPathTests : BaseUrlProviderTest
    {
        private readonly GlobalSettings _globalSettings;

        public UrlProviderWithoutHideTopLevelNodeFromPathTests()
        {
            _globalSettings = new GlobalSettings { HideTopLevelNodeFromPath = HideTopLevelNodeFromPath };
        }

        protected override bool HideTopLevelNodeFromPath => false;

        protected override void ComposeSettings()
        {
            base.ComposeSettings();
            Builder.Services.AddTransient(x => Microsoft.Extensions.Options.Options.Create(_globalSettings));
        }

        /// <summary>
        /// This checks that when we retrieve a NiceUrl for multiple items that there are no issues with cache overlap
        /// and that they are all cached correctly.
        /// </summary>
        [Test]
        public void Ensure_Cache_Is_Correct()
        {
            var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = false };

            var umbracoContext = GetUmbracoContext("/test", 1111, globalSettings: _globalSettings);
            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);
            var urlProvider = new DefaultUrlProvider(
                Microsoft.Extensions.Options.Options.Create(requestHandlerSettings),
                LoggerFactory.CreateLogger<DefaultUrlProvider>(),
                Microsoft.Extensions.Options.Options.Create(_globalSettings),
                new SiteDomainHelper(), umbracoContextAccessor, UriUtility);

            var publishedUrlProvider = GetPublishedUrlProvider(umbracoContext, urlProvider);

            var samples = new Dictionary<int, string> {
                { 1046, "/home" },
                { 1173, "/home/sub1" },
                { 1174, "/home/sub1/sub2" },
                { 1176, "/home/sub1/sub-3" },
                { 1177, "/home/sub1/custom-sub-1" },
                { 1178, "/home/sub1/custom-sub-2" },
                { 1175, "/home/sub-2" },
                { 1172, "/test-page" }
            };

            foreach (var sample in samples)
            {
                var result = publishedUrlProvider.GetUrl(sample.Key);
                Assert.AreEqual(sample.Value, result);
            }

            var randomSample = new KeyValuePair<int, string>(1177, "/home/sub1/custom-sub-1");
            for (int i = 0; i < 5; i++)
            {
                var result = publishedUrlProvider.GetUrl(randomSample.Key);
                Assert.AreEqual(randomSample.Value, result);
            }

            var cache = umbracoContext.Content as PublishedContentCache;
            if (cache == null) throw new Exception("Unsupported IPublishedContentCache, only the Xml one is supported.");
            var cachedRoutes = cache.RoutesCache.GetCachedRoutes();
            Assert.AreEqual(8, cachedRoutes.Count);

            foreach (var sample in samples)
            {
                Assert.IsTrue(cachedRoutes.ContainsKey(sample.Key));
                Assert.AreEqual(sample.Value, cachedRoutes[sample.Key]);
            }

            var cachedIds = cache.RoutesCache.GetCachedIds();
            Assert.AreEqual(0, cachedIds.Count);
        }

        [TestCase(1046, "/home/")]
        [TestCase(1173, "/home/sub1/")]
        [TestCase(1174, "/home/sub1/sub2/")]
        [TestCase(1176, "/home/sub1/sub-3/")]
        [TestCase(1177, "/home/sub1/custom-sub-1/")]
        [TestCase(1178, "/home/sub1/custom-sub-2/")]
        [TestCase(1175, "/home/sub-2/")]
        [TestCase(1172, "/test-page/")]
        public void Get_Url_Not_Hiding_Top_Level(int nodeId, string niceUrlMatch)
        {
            var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };

            var umbracoContext = GetUmbracoContext("/test", 1111, globalSettings: _globalSettings);
            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);
            var urlProvider = new DefaultUrlProvider(
                Microsoft.Extensions.Options.Options.Create(requestHandlerSettings),
                LoggerFactory.CreateLogger<DefaultUrlProvider>(),
                Microsoft.Extensions.Options.Options.Create(_globalSettings),
                new SiteDomainHelper(), umbracoContextAccessor, UriUtility);
            var publishedUrlProvider = GetPublishedUrlProvider(umbracoContext, urlProvider);

            var result = publishedUrlProvider.GetUrl(nodeId);
            Assert.AreEqual(niceUrlMatch, result);
        }

        [Test]
        public void Get_Url_For_Culture_Variant_Without_Domains_Non_Current_Url()
        {
            const string currentUri = "http://example.us/test";

            var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };

            var contentType = new PublishedContentType(Guid.NewGuid(), 666, "alias", PublishedItemType.Content, Enumerable.Empty<string>(), Enumerable.Empty<PublishedPropertyType>(), ContentVariation.Culture);
            var publishedContent = new SolidPublishedContent(contentType) { Id = 1234 };

            var publishedContentCache = new Mock<IPublishedContentCache>();
            publishedContentCache.Setup(x => x.GetRouteById(1234, "fr-FR"))
                .Returns("9876/home/test-fr"); //prefix with the root id node with the domain assigned as per the umbraco standard
            publishedContentCache.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns<int>(id => id == 1234 ? publishedContent : null);

            var domainCache = new Mock<IDomainCache>();
            domainCache.Setup(x => x.GetAssigned(It.IsAny<int>(), false))
                .Returns((int contentId, bool includeWildcards) => Enumerable.Empty<Domain>());

            var snapshot = Mock.Of<IPublishedSnapshot>(x => x.Content == publishedContentCache.Object && x.Domains == domainCache.Object);

            var snapshotService = new Mock<IPublishedSnapshotService>();
            snapshotService.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>()))
                .Returns(snapshot);

            var umbracoContext = GetUmbracoContext(currentUri,
                globalSettings: _globalSettings,
                snapshotService: snapshotService.Object);
            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);
            var urlProvider = new DefaultUrlProvider(
                Microsoft.Extensions.Options.Options.Create(requestHandlerSettings),
                LoggerFactory.CreateLogger<DefaultUrlProvider>(),
                Microsoft.Extensions.Options.Options.Create(_globalSettings),
                new SiteDomainHelper(), umbracoContextAccessor, UriUtility);
            var publishedUrlProvider = GetPublishedUrlProvider(umbracoContext, urlProvider);

            //even though we are asking for a specific culture URL, there are no domains assigned so all that can be returned is a normal relative url.
            var url = publishedUrlProvider.GetUrl(1234, culture: "fr-FR");

            Assert.AreEqual("/home/test-fr/", url);
        }

        /// <summary>
        /// This tests DefaultUrlProvider.GetUrl with a specific culture when the current URL is the culture specific domain
        /// </summary>
        [Test]
        public void Get_Url_For_Culture_Variant_With_Current_Url()
        {
            const string currentUri = "http://example.fr/test";

            var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };

            var contentType = new PublishedContentType(Guid.NewGuid(), 666, "alias", PublishedItemType.Content, Enumerable.Empty<string>(), Enumerable.Empty<PublishedPropertyType>(), ContentVariation.Culture);
            var publishedContent = new SolidPublishedContent(contentType) { Id = 1234 };

            var publishedContentCache = new Mock<IPublishedContentCache>();
            publishedContentCache.Setup(x => x.GetRouteById(1234, "fr-FR"))
                .Returns("9876/home/test-fr"); //prefix with the root id node with the domain assigned as per the umbraco standard
            publishedContentCache.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns<int>(id => id == 1234 ? publishedContent : null);

            var domainCache = new Mock<IDomainCache>();
            domainCache.Setup(x => x.GetAssigned(It.IsAny<int>(), false))
                .Returns((int contentId, bool includeWildcards) =>
                {
                    if (contentId != 9876) return Enumerable.Empty<Domain>();
                    return new[]
                    {
                        new Domain(2, "example.us", 9876, CultureInfo.GetCultureInfo("en-US"), false), //default
                        new Domain(3, "example.fr", 9876, CultureInfo.GetCultureInfo("fr-FR"), false)
                    };
                });
            domainCache.Setup(x => x.DefaultCulture).Returns(CultureInfo.GetCultureInfo("en-US").Name);

            var snapshot = Mock.Of<IPublishedSnapshot>(x => x.Content == publishedContentCache.Object && x.Domains == domainCache.Object);

            var snapshotService = new Mock<IPublishedSnapshotService>();
            snapshotService.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>()))
                .Returns(snapshot);

            var umbracoContext = GetUmbracoContext(currentUri,
                globalSettings: _globalSettings,
                snapshotService: snapshotService.Object);
            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);
            var urlProvider = new DefaultUrlProvider(
                Microsoft.Extensions.Options.Options.Create(requestHandlerSettings),
                LoggerFactory.CreateLogger<DefaultUrlProvider>(),
                Microsoft.Extensions.Options.Options.Create(_globalSettings),
                new SiteDomainHelper(), umbracoContextAccessor, UriUtility);

            var publishedUrlProvider = GetPublishedUrlProvider(umbracoContext, urlProvider);

            var url = publishedUrlProvider.GetUrl(1234, culture: "fr-FR");

            Assert.AreEqual("/home/test-fr/", url);
        }

        /// <summary>
        /// This tests DefaultUrlProvider.GetUrl with a specific culture when the current URL is not the culture specific domain
        /// </summary>
        [Test]
        public void Get_Url_For_Culture_Variant_Non_Current_Url()
        {
            const string currentUri = "http://example.us/test";

            var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };

            var contentType = new PublishedContentType(Guid.NewGuid(), 666, "alias", PublishedItemType.Content, Enumerable.Empty<string>(), Enumerable.Empty<PublishedPropertyType>(), ContentVariation.Culture);
            var publishedContent = new SolidPublishedContent(contentType) { Id = 1234 };

            var publishedContentCache = new Mock<IPublishedContentCache>();
            publishedContentCache.Setup(x => x.GetRouteById(1234, "fr-FR"))
                .Returns("9876/home/test-fr"); //prefix with the root id node with the domain assigned as per the umbraco standard
            publishedContentCache.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns<int>(id => id == 1234 ? publishedContent : null);

            var domainCache = new Mock<IDomainCache>();
            domainCache.Setup(x => x.GetAssigned(It.IsAny<int>(), false))
                .Returns((int contentId, bool includeWildcards) =>
                {
                    if (contentId != 9876) return Enumerable.Empty<Domain>();
                    return new[]
                    {
                        new Domain(2, "example.us", 9876, CultureInfo.GetCultureInfo("en-US"), false), //default
                        new Domain(3, "example.fr", 9876, CultureInfo.GetCultureInfo("fr-FR"), false)
                    };
                });
            domainCache.Setup(x => x.DefaultCulture).Returns(CultureInfo.GetCultureInfo("en-US").Name);

            var snapshot = Mock.Of<IPublishedSnapshot>(x => x.Content == publishedContentCache.Object && x.Domains == domainCache.Object);

            var snapshotService = new Mock<IPublishedSnapshotService>();
            snapshotService.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>()))
                .Returns(snapshot);

            var umbracoContext = GetUmbracoContext(currentUri,
                globalSettings: _globalSettings,
                snapshotService: snapshotService.Object);
            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);
            var urlProvider = new DefaultUrlProvider(
                Microsoft.Extensions.Options.Options.Create(requestHandlerSettings),
                LoggerFactory.CreateLogger<DefaultUrlProvider>(),
                Microsoft.Extensions.Options.Options.Create(_globalSettings),
                new SiteDomainHelper(), umbracoContextAccessor, UriUtility);


            var publishedUrlProvider = GetPublishedUrlProvider(umbracoContext, urlProvider);
            var url = publishedUrlProvider.GetUrl(1234, culture: "fr-FR");

            //the current uri is not the culture specific domain we want, so the result is an absolute path to the culture specific domain
            Assert.AreEqual("http://example.fr/home/test-fr/", url);
        }

        [Test]
        public void Get_Url_Relative_Or_Absolute()
        {
            var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };

            var umbracoContext = GetUmbracoContext("http://example.com/test", 1111, globalSettings: _globalSettings);
            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);
            var urlProvider = new DefaultUrlProvider(
                Microsoft.Extensions.Options.Options.Create(requestHandlerSettings),
                LoggerFactory.CreateLogger<DefaultUrlProvider>(),
                Microsoft.Extensions.Options.Options.Create(_globalSettings),
                new SiteDomainHelper(), umbracoContextAccessor, UriUtility);
            var publishedUrlProvider = GetPublishedUrlProvider(umbracoContext, urlProvider);

            Assert.AreEqual("/home/sub1/custom-sub-1/", publishedUrlProvider.GetUrl(1177));

            publishedUrlProvider.Mode = UrlMode.Absolute;
            Assert.AreEqual("http://example.com/home/sub1/custom-sub-1/", publishedUrlProvider.GetUrl(1177));
        }

        [Test]
        public void Get_Url_Unpublished()
        {
            var requestHandlerSettings = new RequestHandlerSettings();

            var urlProvider = new DefaultUrlProvider(Microsoft.Extensions.Options.Options.Create(requestHandlerSettings),
                LoggerFactory.CreateLogger<DefaultUrlProvider>(),
                Microsoft.Extensions.Options.Options.Create(_globalSettings),
                new SiteDomainHelper(), UmbracoContextAccessor, UriUtility);
            var umbracoContext = GetUmbracoContext("http://example.com/test", 1111, globalSettings: _globalSettings);
            var publishedUrlProvider = GetPublishedUrlProvider(umbracoContext, urlProvider);

            //mock the Umbraco settings that we need

            Assert.AreEqual("#", publishedUrlProvider.GetUrl(999999));

            publishedUrlProvider.Mode = UrlMode.Absolute;

            Assert.AreEqual("#", publishedUrlProvider.GetUrl(999999));
        }
    }
}
