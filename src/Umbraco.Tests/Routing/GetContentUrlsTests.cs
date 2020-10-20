using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Tests.Common;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Web.Routing;
using Microsoft.Extensions.Logging;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    public class GetContentUrlsTests : UrlRoutingTestBase
    {
        private GlobalSettings _globalSettings;
        private WebRoutingSettings _webRoutingSettings;
        private RequestHandlerSettings _requestHandlerSettings;

        public override void SetUp()
        {
            base.SetUp();

            _globalSettings = new GlobalSettings();
            _webRoutingSettings = new WebRoutingSettings();
            _requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };
        }

        private ILocalizedTextService GetTextService()
        {
            var textService = Mock.Of<ILocalizedTextService>(
                x => x.Localize("content/itemNotPublished",
                         It.IsAny<CultureInfo>(),
                         It.IsAny<IDictionary<string, string>>()) == "content/itemNotPublished");
            return textService;
        }

        private ILocalizationService GetLangService(params string[] isoCodes)
        {
            var allLangs = isoCodes
                .Select(CultureInfo.GetCultureInfo)
                .Select(culture => new Language(_globalSettings, culture.Name)
                {
                    CultureName = culture.DisplayName,
                    IsDefault = true,
                    IsMandatory = true
                }).ToArray();

            var langService = Mock.Of<ILocalizationService>(x => x.GetAllLanguages() == allLangs);
            return langService;
        }

        [Test]
        public void Content_Not_Published()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            var content = MockedContent.CreateBasicContent(contentType);
            content.Id = 1046; // FIXME: we are using this ID only because it's built into the test XML published cache
            content.Path = "-1,1046";

            var umbContext = GetUmbracoContext("http://localhost:8000");
            var publishedRouter = CreatePublishedRouter(Factory,
                contentFinders: new ContentFinderCollection(new[] { new ContentFinderByUrl(LoggerFactory.CreateLogger<ContentFinderByUrl>()) }));
            var urls = content.GetContentUrls(publishedRouter,
                umbContext,
                GetLangService("en-US", "fr-FR"), GetTextService(), ServiceContext.ContentService,
                VariationContextAccessor,
                LoggerFactory.CreateLogger<IContent>(),
                UriUtility,
                PublishedUrlProvider).ToList();

            Assert.AreEqual(1, urls.Count);
            Assert.AreEqual("content/itemNotPublished", urls[0].Text);
            Assert.IsFalse(urls[0].IsUrl);
        }

        [Test]
        public void Invariant_Root_Content_Published_No_Domains()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            var content = MockedContent.CreateBasicContent(contentType);
            content.Id = 1046; // FIXME: we are using this ID only because it's built into the test XML published cache
            content.Path = "-1,1046";
            content.Published = true;

            var umbContext = GetUmbracoContext("http://localhost:8000");
            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbContext);
            var urlProvider = new DefaultUrlProvider(
                Microsoft.Extensions.Options.Options.Create(_requestHandlerSettings),
                LoggerFactory.CreateLogger<DefaultUrlProvider>(),
                Microsoft.Extensions.Options.Options.Create(_globalSettings),
                new SiteDomainHelper(),
                umbracoContextAccessor, UriUtility);
            var publishedUrlProvider = new UrlProvider(
                umbracoContextAccessor,
                Microsoft.Extensions.Options.Options.Create(_webRoutingSettings),
                new UrlProviderCollection(new []{urlProvider}),
                new MediaUrlProviderCollection(Enumerable.Empty<IMediaUrlProvider>()),
                Mock.Of<IVariationContextAccessor>()
            );

            var publishedRouter = CreatePublishedRouter(Factory,
                contentFinders:new ContentFinderCollection(new[]{new ContentFinderByUrl(LoggerFactory.CreateLogger<ContentFinderByUrl>()) }));
            var urls = content.GetContentUrls(publishedRouter,
                umbContext,
                GetLangService("en-US", "fr-FR"), GetTextService(), ServiceContext.ContentService,
                VariationContextAccessor,
                LoggerFactory.CreateLogger<IContent>(),
                UriUtility,
                publishedUrlProvider).ToList();

            Assert.AreEqual(1, urls.Count);
            Assert.AreEqual("/home/", urls[0].Text);
            Assert.AreEqual("en-US", urls[0].Culture);
            Assert.IsTrue(urls[0].IsUrl);
        }

        [Test]
        public void Invariant_Child_Content_Published_No_Domains()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();
            var parent = MockedContent.CreateBasicContent(contentType);
            parent.Id = 1046; // FIXME: we are using this ID only because it's built into the test XML published cache
            parent.Name = "home";
            parent.Path = "-1,1046";
            parent.Published = true;
            var child = MockedContent.CreateBasicContent(contentType);
            child.Name = "sub1";
            child.Id = 1173; // FIXME: we are using this ID only because it's built into the test XML published cache
            child.Path = "-1,1046,1173";
            child.Published = true;

            var umbContext = GetUmbracoContext("http://localhost:8000");
            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbContext);
            var urlProvider = new DefaultUrlProvider(
                Microsoft.Extensions.Options.Options.Create(_requestHandlerSettings),
                LoggerFactory.CreateLogger<DefaultUrlProvider>(),
                Microsoft.Extensions.Options.Options.Create(_globalSettings),
                new SiteDomainHelper(), umbracoContextAccessor, UriUtility);
            var publishedUrlProvider = new UrlProvider(
                umbracoContextAccessor,
                Microsoft.Extensions.Options.Options.Create(_webRoutingSettings),
                new UrlProviderCollection(new []{urlProvider}),
                new MediaUrlProviderCollection(Enumerable.Empty<IMediaUrlProvider>()),
                Mock.Of<IVariationContextAccessor>()
            );

            var publishedRouter = CreatePublishedRouter(Factory,
                contentFinders: new ContentFinderCollection(new[] { new ContentFinderByUrl(LoggerFactory.CreateLogger<ContentFinderByUrl>()) }));
            var urls = child.GetContentUrls(publishedRouter,
                umbContext,
                GetLangService("en-US", "fr-FR"), GetTextService(), ServiceContext.ContentService,
                VariationContextAccessor,
                LoggerFactory.CreateLogger<IContent>(),
                UriUtility,
                publishedUrlProvider
                ).ToList();

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
