using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class UrlsProviderWithDomainsTests : UrlRoutingTestBase
{
    private const string CacheKeyPrefix = "NuCache.ContentCache.RouteByContent";

    private void SetDomains1()
    {
        var domainService = Mock.Get(DomainService);

        domainService.Setup(service => service.GetAll(It.IsAny<bool>()))
            .Returns((bool incWildcards) => new[]
            {
                new UmbracoDomain("domain1.com")
                {
                    Id = 1, LanguageId = LangFrId, RootContentId = 1001, LanguageIsoCode = "fr-FR",
                },
            });
    }

    private void SetDomains2()
    {
        var domainService = Mock.Get(DomainService);

        domainService.Setup(service => service.GetAll(It.IsAny<bool>()))
            .Returns((bool incWildcards) => new[]
            {
                new UmbracoDomain("http://domain1.com/foo")
                {
                    Id = 1, LanguageId = LangFrId, RootContentId = 1001, LanguageIsoCode = "fr-FR",
                },
            });
    }

    private void SetDomains3()
    {
        var domainService = Mock.Get(DomainService);

        domainService.Setup(service => service.GetAll(It.IsAny<bool>()))
            .Returns((bool incWildcards) => new[]
            {
                new UmbracoDomain("http://domain1.com/")
                {
                    Id = 1, LanguageId = LangFrId, RootContentId = 10011, LanguageIsoCode = "fr-FR",
                },
            });
    }

    private void SetDomains4()
    {
        var domainService = Mock.Get(DomainService);

        domainService.Setup(service => service.GetAll(It.IsAny<bool>()))
            .Returns((bool incWildcards) => new[]
            {
                new UmbracoDomain("http://domain1.com/")
                {
                    Id = 1, LanguageId = LangEngId, RootContentId = 1001, LanguageIsoCode = "en-US",
                },
                new UmbracoDomain("http://domain1.com/en")
                {
                    Id = 2, LanguageId = LangEngId, RootContentId = 10011, LanguageIsoCode = "en-US",
                },
                new UmbracoDomain("http://domain1.com/fr")
                {
                    Id = 3, LanguageId = LangFrId, RootContentId = 10012, LanguageIsoCode = "fr-FR",
                },
                new UmbracoDomain("http://domain3.com/")
                {
                    Id = 4, LanguageId = LangEngId, RootContentId = 1003, LanguageIsoCode = "en-US",
                },
                new UmbracoDomain("http://domain3.com/en")
                {
                    Id = 5, LanguageId = LangEngId, RootContentId = 10031, LanguageIsoCode = "en-US",
                },
                new UmbracoDomain("http://domain3.com/fr")
                {
                    Id = 6, LanguageId = LangFrId, RootContentId = 10032, LanguageIsoCode = "fr-FR",
                },
            });
    }

    private void SetDomains5()
    {
        var domainService = Mock.Get(DomainService);

        domainService.Setup(service => service.GetAll(It.IsAny<bool>()))
            .Returns((bool incWildcards) => new[]
            {
                new UmbracoDomain("http://domain1.com/en")
                {
                    Id = 1, LanguageId = LangEngId, RootContentId = 10011, LanguageIsoCode = "en-US",
                },
                new UmbracoDomain("http://domain1a.com/en")
                {
                    Id = 2, LanguageId = LangEngId, RootContentId = 10011, LanguageIsoCode = "en-US",
                },
                new UmbracoDomain("http://domain1b.com/en")
                {
                    Id = 3, LanguageId = LangEngId, RootContentId = 10011, LanguageIsoCode = "en-US",
                },
                new UmbracoDomain("http://domain1.com/fr")
                {
                    Id = 4, LanguageId = LangFrId, RootContentId = 10012, LanguageIsoCode = "fr-FR",
                },
                new UmbracoDomain("http://domain1a.com/fr")
                {
                    Id = 5, LanguageId = LangFrId, RootContentId = 10012, LanguageIsoCode = "fr-FR",
                },
                new UmbracoDomain("http://domain1b.com/fr")
                {
                    Id = 6, LanguageId = LangFrId, RootContentId = 10012, LanguageIsoCode = "fr-FR",
                },
                new UmbracoDomain("http://domain3.com/en")
                {
                    Id = 7, LanguageId = LangEngId, RootContentId = 10031, LanguageIsoCode = "en-US",
                },
                new UmbracoDomain("http://domain3.com/fr")
                {
                    Id = 8, LanguageId = LangFrId, RootContentId = 10032, LanguageIsoCode = "fr-FR",
                },
            });
    }

    protected override string GetXmlContent(int templateId)
        => @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[
<!ELEMENT Doc ANY>
<!ATTLIST Doc id ID #REQUIRED>
]>
<root id=""-1"">
    <Doc id=""1001"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""1"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""1001"" writerName=""admin"" creatorName=""admin"" path=""-1,1001"" isDoc="""">
        <content><![CDATA[]]></content>
        <Doc id=""10011"" parentID=""1001"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""1"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""1001-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011"" isDoc="""">
            <content><![CDATA[<div>This is some content</div>]]></content>
            <Doc id=""100111"" parentID=""10011"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""1"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1001-1-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100111"" isDoc="""">
                <content><![CDATA[]]></content>
            </Doc>
            <Doc id=""100112"" parentID=""10011"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-1-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100112"" isDoc="""">
                <content><![CDATA[]]></content>
                <Doc id=""1001121"" parentID=""100112"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-1-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100112,1001121"" isDoc="""">
                    <content><![CDATA[]]></content>
                </Doc>
                <Doc id=""1001122"" parentID=""100112"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-1-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100112,1001122"" isDoc="""">
                    <content><![CDATA[]]></content>
                </Doc>
            </Doc>
        </Doc>
        <Doc id=""10012"" parentID=""1001"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""1001-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012"" isDoc="""">
            <content><![CDATA[<div>This is some content</div>]]></content>
            <Doc id=""100121"" parentID=""10012"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""1"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1001-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012,100121"" isDoc="""">
                <content><![CDATA[]]></content>
            </Doc>
            <Doc id=""100122"" parentID=""10012"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012,100122"" isDoc="""">
                <content><![CDATA[]]></content>
                <Doc id=""1001221"" parentID=""100122"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-2-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012,100122,1001221"" isDoc="""">
                    <content><![CDATA[]]></content>
                </Doc>
                <Doc id=""1001222"" parentID=""100122"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-2-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012,100122,1001222"" isDoc="""">
                    <content><![CDATA[]]></content>
                </Doc>
            </Doc>
        </Doc>
        <Doc id=""10013"" parentID=""1001"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""3"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""1001-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10013"" isDoc="""">
        </Doc>
    </Doc>
    <Doc id=""1002"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" +
           templateId +
           @""" sortOrder=""3"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""1002"" writerName=""admin"" creatorName=""admin"" path=""-1,1002"" isDoc="""">
    </Doc>
    <Doc id=""1003"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""1"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""1003"" writerName=""admin"" creatorName=""admin"" path=""-1,1003"" isDoc="""">
        <content><![CDATA[]]></content>
        <Doc id=""10031"" parentID=""1003"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""1"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""1003-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031"" isDoc="""">
            <content><![CDATA[<div>This is some content</div>]]></content>
            <Doc id=""100311"" parentID=""10031"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""1"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1003-1-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031,100311"" isDoc="""">
                <content><![CDATA[]]></content>
            </Doc>
            <Doc id=""100312"" parentID=""10031"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-1-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031,100312"" isDoc="""">
                <content><![CDATA[]]></content>
                <Doc id=""1003121"" parentID=""100312"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-1-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031,100312,1003121"" isDoc="""">
                    <content><![CDATA[]]></content>
                </Doc>
                <Doc id=""1003122"" parentID=""100312"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-1-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031,100312,1003122"" isDoc="""">
                    <content><![CDATA[]]></content>
                </Doc>
            </Doc>
        </Doc>
        <Doc id=""10032"" parentID=""1003"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""1003-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032"" isDoc="""">
            <content><![CDATA[<div>This is some content</div>]]></content>
            <Doc id=""100321"" parentID=""10032"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""1"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1003-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032,100321"" isDoc="""">
                <content><![CDATA[]]></content>
            </Doc>
            <Doc id=""100322"" parentID=""10032"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032,100322"" isDoc="""">
                <content><![CDATA[]]></content>
                <Doc id=""1003221"" parentID=""100322"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-2-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032,100322,1003221"" isDoc="""">
                    <content><![CDATA[]]></content>
                </Doc>
                <Doc id=""1003222"" parentID=""100322"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-2-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032,100322,1003222"" isDoc="""">
                    <content><![CDATA[]]></content>
                </Doc>
            </Doc>
        </Doc>
        <Doc id=""10033"" parentID=""1003"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" sortOrder=""3"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""1003-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10033"" isDoc="""">
        </Doc>
    </Doc>
</root>";

    // with one simple domain "domain1.com"
    // basic tests
    [TestCase(1001, "http://domain1.com", false, "/")]
    [TestCase(10011, "http://domain1.com", false, "/1001-1/")]
    [TestCase(1002, "http://domain1.com", false, "/1002/")]

    // absolute tests
    [TestCase(1001, "http://domain1.com", true, "http://domain1.com/")]
    [TestCase(10011, "http://domain1.com", true, "http://domain1.com/1001-1/")]

    // different current tests
    [TestCase(1001, "http://domain2.com", false, "http://domain1.com/")]
    [TestCase(10011, "http://domain2.com", false, "http://domain1.com/1001-1/")]
    [TestCase(1001, "https://domain1.com", false, "/")]
    [TestCase(10011, "https://domain1.com", false, "/1001-1/")]
    public void Get_Url_SimpleDomain(int nodeId, string currentUrl, bool absolute, string expected)
    {
        SetDomains1();

        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };
        GlobalSettings.HideTopLevelNodeFromPath = false;

        var umbracoContextAccessor = GetUmbracoContextAccessor("/test");

        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);

        var currentUri = new Uri(currentUrl);
        var mode = absolute ? UrlMode.Absolute : UrlMode.Auto;
        var result = urlProvider.GetUrl(nodeId, mode, current: currentUri);
        Assert.AreEqual(expected, result);
    }

    // with one complete domain "http://domain1.com/foo"
    // basic tests
    [TestCase(1001, "http://domain1.com", false, "/foo/")]
    [TestCase(10011, "http://domain1.com", false, "/foo/1001-1/")]
    [TestCase(1002, "http://domain1.com", false, "/1002/")]

    // absolute tests
    [TestCase(1001, "http://domain1.com", true, "http://domain1.com/foo/")]
    [TestCase(10011, "http://domain1.com", true, "http://domain1.com/foo/1001-1/")]

    // different current tests
    [TestCase(1001, "http://domain2.com", false, "http://domain1.com/foo/")]
    [TestCase(10011, "http://domain2.com", false, "http://domain1.com/foo/1001-1/")]
    [TestCase(1001, "https://domain1.com", false, "http://domain1.com/foo/")]
    [TestCase(10011, "https://domain1.com", false, "http://domain1.com/foo/1001-1/")]
    public void Get_Url_SimpleWithSchemeAndPath(int nodeId, string currentUrl, bool absolute, string expected)
    {
        SetDomains2();

        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };
        GlobalSettings.HideTopLevelNodeFromPath = false;

        var umbracoContextAccessor = GetUmbracoContextAccessor("/test");

        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);

        var currentUri = new Uri(currentUrl);
        var mode = absolute ? UrlMode.Absolute : UrlMode.Auto;
        var result = urlProvider.GetUrl(nodeId, mode, current: currentUri);
        Assert.AreEqual(expected, result);
    }

    // with one domain, not at root
    [TestCase(1001, "http://domain1.com", false, "/1001/")]
    [TestCase(10011, "http://domain1.com", false, "/")]
    [TestCase(100111, "http://domain1.com", false, "/1001-1-1/")]
    [TestCase(1002, "http://domain1.com", false, "/1002/")]
    public void Get_Url_DeepDomain(int nodeId, string currentUrl, bool absolute, string expected)
    {
        SetDomains3();

        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };
        GlobalSettings.HideTopLevelNodeFromPath = false;

        var umbracoContextAccessor = GetUmbracoContextAccessor("/test");

        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);

        var currentUri = new Uri(currentUrl);
        var mode = absolute ? UrlMode.Absolute : UrlMode.Auto;
        var result = urlProvider.GetUrl(nodeId, mode, current: currentUri);
        Assert.AreEqual(expected, result);
    }

    // with nested domains
    [TestCase(1001, "http://domain1.com", false, "/")]
    [TestCase(10011, "http://domain1.com", false, "/en/")]
    [TestCase(100111, "http://domain1.com", false, "/en/1001-1-1/")]
    [TestCase(10012, "http://domain1.com", false, "/fr/")]
    [TestCase(100121, "http://domain1.com", false, "/fr/1001-2-1/")]
    [TestCase(10013, "http://domain1.com", false, "/1001-3/")]
    [TestCase(1002, "http://domain1.com", false, "/1002/")]
    [TestCase(1003, "http://domain3.com", false, "/")]
    [TestCase(10031, "http://domain3.com", false, "/en/")]
    [TestCase(100321, "http://domain3.com", false, "/fr/1003-2-1/")]
    public void Get_Url_NestedDomains(int nodeId, string currentUrl, bool absolute, string expected)
    {
        SetDomains4();

        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };
        GlobalSettings.HideTopLevelNodeFromPath = false;

        var umbracoContextAccessor = GetUmbracoContextAccessor("/test");

        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);

        var currentUri = new Uri(currentUrl);
        var mode = absolute ? UrlMode.Absolute : UrlMode.Auto;
        var result = urlProvider.GetUrl(nodeId, mode, current: currentUri);
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void Get_Url_DomainsAndCache()
    {
        SetDomains4();

        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };
        GlobalSettings.HideTopLevelNodeFromPath = false;

        var umbracoContextAccessor = GetUmbracoContextAccessor("/test");
        var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);

        urlProvider.GetUrl(1001, UrlMode.Auto, current: new Uri("http://domain1.com"));
        urlProvider.GetUrl(10011, UrlMode.Auto, current: new Uri("http://domain1.com"));
        urlProvider.GetUrl(100111, UrlMode.Auto, current: new Uri("http://domain1.com"));
        urlProvider.GetUrl(10012, UrlMode.Auto, current: new Uri("http://domain1.com"));
        urlProvider.GetUrl(100121, UrlMode.Auto, current: new Uri("http://domain1.com"));
        urlProvider.GetUrl(10013, UrlMode.Auto, current: new Uri("http://domain1.com"));
        urlProvider.GetUrl(1002, UrlMode.Auto, current: new Uri("http://domain1.com"));
        urlProvider.GetUrl(1001, UrlMode.Auto, current: new Uri("http://domain2.com"));
        urlProvider.GetUrl(10011, UrlMode.Auto, current: new Uri("http://domain2.com"));
        urlProvider.GetUrl(100111, UrlMode.Auto, current: new Uri("http://domain2.com"));
        urlProvider.GetUrl(1002, UrlMode.Auto, current: new Uri("http://domain2.com"));

        var cache = (FastDictionaryAppCache)umbracoContext.PublishedSnapshot.ElementsCache;
        var cachedRoutes = cache.Keys.Where(x => x.StartsWith(CacheKeyPrefix)).ToList();
        Assert.AreEqual(7, cachedRoutes.Count);

        // var cachedIds = cache.RoutesCache.GetCachedIds();
        // Assert.AreEqual(0, cachedIds.Count);
        CheckRoute(cache, 1001, "1001/");
        CheckRoute(cache, 10011, "10011/");
        CheckRoute(cache, 100111, "10011/1001-1-1");
        CheckRoute(cache, 10012, "10012/");
        CheckRoute(cache, 100121, "10012/1001-2-1");
        CheckRoute(cache, 10013, "1001/1001-3");
        CheckRoute(cache, 1002, "/1002");

        // use the cache
        Assert.AreEqual("/", urlProvider.GetUrl(1001, UrlMode.Auto, current: new Uri("http://domain1.com")));
        Assert.AreEqual("/en/", urlProvider.GetUrl(10011, UrlMode.Auto, current: new Uri("http://domain1.com")));
        Assert.AreEqual("/en/1001-1-1/", urlProvider.GetUrl(100111, UrlMode.Auto, current: new Uri("http://domain1.com")));
        Assert.AreEqual("/fr/", urlProvider.GetUrl(10012, UrlMode.Auto, current: new Uri("http://domain1.com")));
        Assert.AreEqual("/fr/1001-2-1/", urlProvider.GetUrl(100121, UrlMode.Auto, current: new Uri("http://domain1.com")));
        Assert.AreEqual("/1001-3/", urlProvider.GetUrl(10013, UrlMode.Auto, current: new Uri("http://domain1.com")));
        Assert.AreEqual("/1002/", urlProvider.GetUrl(1002, UrlMode.Auto, current: new Uri("http://domain1.com")));

        Assert.AreEqual("http://domain1.com/fr/1001-2-1/", urlProvider.GetUrl(100121, UrlMode.Auto, current: new Uri("http://domain2.com")));
    }

    private static void CheckRoute(FastDictionaryAppCache routes, int id, string route)
    {
        var cacheKey = $"{CacheKeyPrefix}[P:{id}]";
        var found = (string)routes.Get(cacheKey);
        Assert.IsNotNull(found);
        Assert.AreEqual(route, found);
    }

    [Test]
    public void Get_Url_Relative_Or_Absolute()
    {
        SetDomains4();

        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };
        GlobalSettings.HideTopLevelNodeFromPath = false;

        var umbracoContextAccessor = GetUmbracoContextAccessor("http://domain1.com/test");
        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);

        Assert.AreEqual("/en/1001-1-1/", urlProvider.GetUrl(100111));
        Assert.AreEqual("http://domain3.com/en/1003-1-1/", urlProvider.GetUrl(100311));

        urlProvider.Mode = UrlMode.Absolute;

        Assert.AreEqual("http://domain1.com/en/1001-1-1/", urlProvider.GetUrl(100111));
        Assert.AreEqual("http://domain3.com/en/1003-1-1/", urlProvider.GetUrl(100311));
    }

    [Test]
    public void Get_Url_Alternate()
    {
        SetDomains5();

        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };
        GlobalSettings.HideTopLevelNodeFromPath = false;

        var umbracoContextAccessor = GetUmbracoContextAccessor("http://domain1.com/en/test");
        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out _);

        var url = urlProvider.GetUrl(100111, UrlMode.Absolute);
        Assert.AreEqual("http://domain1.com/en/1001-1-1/", url);

        var result = urlProvider.GetOtherUrls(100111).ToArray();

        foreach (var x in result)
        {
            Console.WriteLine(x);
        }

        Assert.AreEqual(2, result.Length);
        Assert.AreEqual(result[0].Text, "http://domain1b.com/en/1001-1-1/");
        Assert.AreEqual(result[1].Text, "http://domain1a.com/en/1001-1-1/");
    }
}
