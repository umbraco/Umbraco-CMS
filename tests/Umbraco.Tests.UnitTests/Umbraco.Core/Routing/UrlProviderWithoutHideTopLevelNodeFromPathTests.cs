using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class UrlProviderWithoutHideTopLevelNodeFromPathTests : PublishedSnapshotServiceTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();

        GlobalSettings.HideTopLevelNodeFromPath = false;
    }

    private const string CacheKeyPrefix = "NuCache.ContentCache.RouteByContent";

    private void PopulateCache(string culture = "fr-FR")
    {
        var dataTypes = GetDefaultDataTypes();
        var propertyDataTypes = new Dictionary<string, IDataType>
        {
            // we only have one data type for this test which will be resolved with string empty.
            [string.Empty] = dataTypes[0],
        };
        var contentType1 = new ContentType(ShortStringHelper, -1);

        var rootData = new ContentDataBuilder()
            .WithName("Page" + Guid.NewGuid())
            .WithCultureInfos(new Dictionary<string, CultureVariation>
            {
                [culture] = new() { Name = "root", IsDraft = true, Date = DateTime.Now, UrlSegment = "root" },
            })
            .Build(ShortStringHelper, propertyDataTypes, contentType1, "alias");

        var root = ContentNodeKitBuilder.CreateWithContent(
            contentType1.Id,
            9876,
            "-1,9876",
            draftData: rootData,
            publishedData: rootData);

        var parentData = new ContentDataBuilder()
            .WithName("Page" + Guid.NewGuid())
            .WithCultureInfos(new Dictionary<string, CultureVariation>
            {
                [culture] = new() { Name = "home", IsDraft = true, Date = DateTime.Now, UrlSegment = "home" },
            })
            .Build();

        var parent = ContentNodeKitBuilder.CreateWithContent(
            contentType1.Id,
            5432,
            "-1,9876,5432",
            parentContentId: 9876,
            draftData: parentData,
            publishedData: parentData);

        var contentData = new ContentDataBuilder()
            .WithName("Page" + Guid.NewGuid())
            .WithCultureInfos(new Dictionary<string, CultureVariation>
            {
                [culture] = new() { Name = "name-fr2", IsDraft = true, Date = DateTime.Now, UrlSegment = "test-fr" },
            })
            .Build();

        var content = ContentNodeKitBuilder.CreateWithContent(
            contentType1.Id,
            1234,
            "-1,9876,5432,1234",
            parentContentId: 5432,
            draftData: contentData,
            publishedData: contentData);

        InitializedCache(new[] { root, parent, content }, new[] { contentType1 }, dataTypes);
    }

    private void SetDomains1()
    {
        var domainService = Mock.Get(DomainService);

        domainService.Setup(service => service.GetAll(It.IsAny<bool>()))
            .Returns((bool incWildcards) => new[]
            {
                new UmbracoDomain("http://example.us/") { Id = 1, RootContentId = 9876, LanguageIsoCode = "en-US" },
                new UmbracoDomain("http://example.fr/") { Id = 2, RootContentId = 9876, LanguageIsoCode = "fr-FR" },
            });
    }

    /// <summary>
    ///     This checks that when we retrieve a NiceUrl for multiple items that there are no issues with cache overlap
    ///     and that they are all cached correctly.
    /// </summary>
    [Test]
    public void Ensure_Cache_Is_Correct()
    {
        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = false };

        var xml = PublishedContentXml.BaseWebTestXml(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);

        var umbracoContextAccessor = GetUmbracoContextAccessor("/test");
        var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);

        var samples = new Dictionary<int, string>
        {
            { 1046, "/home" },
            { 1173, "/home/sub1" },
            { 1174, "/home/sub1/sub2" },
            { 1176, "/home/sub1/sub-3" },
            { 1177, "/home/sub1/custom-sub-1" },
            { 1178, "/home/sub1/custom-sub-2" },
            { 1175, "/home/sub-2" },
            { 1172, "/test-page" },
        };

        foreach (var sample in samples)
        {
            var result = urlProvider.GetUrl(sample.Key);
            Assert.AreEqual(sample.Value, result);
        }

        var randomSample = new KeyValuePair<int, string>(1177, "/home/sub1/custom-sub-1");
        for (var i = 0; i < 5; i++)
        {
            var result = urlProvider.GetUrl(randomSample.Key);
            Assert.AreEqual(randomSample.Value, result);
        }

        var cache = (FastDictionaryAppCache)umbracoContext.PublishedSnapshot.ElementsCache;
        var cachedRoutes = cache.Keys.Where(x => x.StartsWith(CacheKeyPrefix)).ToList();
        Assert.AreEqual(8, cachedRoutes.Count);

        foreach (var sample in samples)
        {
            var cacheKey = $"{CacheKeyPrefix}[P:{sample.Key}]";
            var found = (string)cache.Get(cacheKey);
            Assert.IsNotNull(found);
            Assert.AreEqual(sample.Value, found);
        }
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

        var xml = PublishedContentXml.BaseWebTestXml(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);

        var umbracoContextAccessor = GetUmbracoContextAccessor("/test");
        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);

        var result = urlProvider.GetUrl(nodeId);
        Assert.AreEqual(niceUrlMatch, result);
    }

    [Test]
    [TestCase("fr-FR", ExpectedResult = "#")] // Non default cultures cannot return urls
    [TestCase("en-US", ExpectedResult = "/root/home/test-fr/")] // Default culture can return urls
    public string Get_Url_For_Culture_Variant_Without_Domains_Non_Current_Url(string culture)
    {
        const string currentUri = "http://example.us/test";

        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };

        PopulateCache(culture);

        var umbracoContextAccessor = GetUmbracoContextAccessor(currentUri);
        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);

        // even though we are asking for a specific culture URL, there are no domains assigned so all that can be returned is a normal relative URL.
        var url = urlProvider.GetUrl(1234, culture: culture);

        return url;
    }

    /// <summary>
    ///     This tests DefaultUrlProvider.GetUrl with a specific culture when the current URL is the culture specific domain
    /// </summary>
    [Test]
    public void Get_Url_For_Culture_Variant_With_Current_Url()
    {
        const string currentUri = "http://example.fr/test";

        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };

        PopulateCache();

        SetDomains1();

        var umbracoContextAccessor = GetUmbracoContextAccessor(currentUri);
        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);

        var url = urlProvider.GetUrl(1234, culture: "fr-FR");

        Assert.AreEqual("/home/test-fr/", url);
    }

    /// <summary>
    ///     This tests DefaultUrlProvider.GetUrl with a specific culture when the current URL is not the culture specific
    ///     domain
    /// </summary>
    [Test]
    public void Get_Url_For_Culture_Variant_Non_Current_Url()
    {
        const string currentUri = "http://example.us/test";

        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };

        PopulateCache();

        SetDomains1();

        var umbracoContextAccessor = GetUmbracoContextAccessor(currentUri);
        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);
        var url = urlProvider.GetUrl(1234, culture: "fr-FR");

        // the current uri is not the culture specific domain we want, so the result is an absolute path to the culture specific domain
        Assert.AreEqual("http://example.fr/home/test-fr/", url);
    }

    [Test]
    public void Get_Url_Relative_Or_Absolute()
    {
        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };

        var xml = PublishedContentXml.BaseWebTestXml(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);

        var umbracoContextAccessor = GetUmbracoContextAccessor("http://example.com/test");

        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);

        Assert.AreEqual("/home/sub1/custom-sub-1/", urlProvider.GetUrl(1177));

        urlProvider.Mode = UrlMode.Absolute;
        Assert.AreEqual("http://example.com/home/sub1/custom-sub-1/", urlProvider.GetUrl(1177));
    }

    [Test]
    public void Get_Url_Unpublished()
    {
        var requestHandlerSettings = new RequestHandlerSettings();

        var xml = PublishedContentXml.BaseWebTestXml(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);

        var umbracoContextAccessor = GetUmbracoContextAccessor("http://example.com/test");

        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);

        // mock the Umbraco settings that we need
        Assert.AreEqual("#", urlProvider.GetUrl(999999));

        urlProvider.Mode = UrlMode.Absolute;

        Assert.AreEqual("#", urlProvider.GetUrl(999999));
    }
}
