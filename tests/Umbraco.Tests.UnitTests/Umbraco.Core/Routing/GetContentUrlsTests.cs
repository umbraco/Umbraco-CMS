using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class GetContentUrlsTests : PublishedSnapshotServiceTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();

        _webRoutingSettings = new WebRoutingSettings();
        _requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };

        GlobalSettings.HideTopLevelNodeFromPath = false;

        var xml = PublishedContentXml.BaseWebTestXml(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);
    }

    private WebRoutingSettings _webRoutingSettings;
    private RequestHandlerSettings _requestHandlerSettings;

    private ILocalizedTextService GetTextService()
    {
        var textService = new Mock<ILocalizedTextService>();
        textService.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args)
                => $"{key}/{alias}");

        return textService.Object;
    }

    private ILocalizationService GetLangService(params string[] isoCodes)
    {
        var allLangs = isoCodes
            .Select(CultureInfo.GetCultureInfo)
            .Select(culture => new Language(culture.Name, culture.EnglishName) { IsDefault = true, IsMandatory = true })
            .ToArray();

        var langServiceMock = new Mock<ILocalizationService>();
        langServiceMock.Setup(x => x.GetAllLanguages()).Returns(allLangs);
        langServiceMock.Setup(x => x.GetDefaultLanguageIsoCode()).Returns(allLangs.First(x => x.IsDefault).IsoCode);

        return langServiceMock.Object;
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

        var urlProvider = GetUrlProvider(umbracoContextAccessor, _requestHandlerSettings, _webRoutingSettings, out var uriUtility);

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

        var urlProvider = GetUrlProvider(umbracoContextAccessor, _requestHandlerSettings, _webRoutingSettings, out var uriUtility);

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

        Assert.AreEqual(2, urls.Count);

        var enUrl = urls.First(x => x.Culture == "en-US");

        Assert.AreEqual("/home/", enUrl.Text);
        Assert.AreEqual("en-US", enUrl.Culture);
        Assert.IsTrue(enUrl.IsUrl);

        var frUrl = urls.First(x => x.Culture == "fr-FR");

        Assert.IsFalse(frUrl.IsUrl);
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

        var localizationService = GetLangService("en-US", "fr-FR");
        var urlProvider = GetUrlProvider(umbracoContextAccessor, _requestHandlerSettings, _webRoutingSettings, out var uriUtility);

        var urls = (await child.GetContentUrlsAsync(
            publishedRouter,
            umbracoContext,
            localizationService,
            GetTextService(),
            Mock.Of<IContentService>(),
            VariationContextAccessor,
            Mock.Of<ILogger<IContent>>(),
            uriUtility,
            urlProvider)).ToList();

        Assert.AreEqual(2, urls.Count);

        var enUrl = urls.First(x => x.Culture == "en-US");

        Assert.AreEqual("/home/sub1/", enUrl.Text);
        Assert.AreEqual("en-US", enUrl.Culture);
        Assert.IsTrue(enUrl.IsUrl);

        var frUrl = urls.First(x => x.Culture == "fr-FR");

        Assert.IsFalse(frUrl.IsUrl);
    }

    // TODO: We need a lot of tests here, the above was just to get started with being able to unit test this method
    // * variant URLs without domains assigned, what happens?
    // * variant URLs with domains assigned, but also having more languages installed than there are domains/cultures assigned
    // * variant URLs with an ancestor culture unpublished
    // * invariant URLs with ancestors as variants
    // * ... probably a lot more
}
