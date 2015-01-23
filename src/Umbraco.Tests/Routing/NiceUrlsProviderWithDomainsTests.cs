using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic.web;
using System.Configuration;
namespace Umbraco.Tests.Routing
{
    [TestFixture]
    public class NiceUrlsProviderWithDomainsTests : UrlRoutingTestBase
    {
        protected override void FreezeResolution()
        {
            SiteDomainHelperResolver.Current = new SiteDomainHelperResolver(new SiteDomainHelper());
            base.FreezeResolution();
        }


        void SetDomains1()
        {

            SetupDomainServiceMock(new[]
		    {
		        new UmbracoDomain("domain1.com") {Id = 1, Language = new Language("fr-FR"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 1001}}
		    });
        }

        void SetDomains2()
        {
            SetupDomainServiceMock(new[]
            {
                new UmbracoDomain("http://domain1.com/foo") {Id = 1, Language = new Language("fr-FR"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 1001}}
            });
        }

        void SetDomains3()
        {
            SetupDomainServiceMock(new[]
            {
                new UmbracoDomain("http://domain1.com/") {Id = 1, Language = new Language("fr-FR"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10011}}
            });
        }

        void SetDomains4()
        {
            SetupDomainServiceMock(new[]
            {
                new UmbracoDomain("http://domain1.com/") {Id = 1, Language = new Language("en-US"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 1001}},
                new UmbracoDomain("http://domain1.com/en") {Id = 1, Language = new Language("en-US"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10011}},
                new UmbracoDomain("http://domain1.com/fr") {Id = 1, Language = new Language("fr-FR"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10012}},
                new UmbracoDomain("http://domain3.com/") {Id = 1, Language = new Language("en-US"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 1003}},
                new UmbracoDomain("http://domain3.com/en") {Id = 1, Language = new Language("en-US"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10031}},
                new UmbracoDomain("http://domain3.com/fr") {Id = 1, Language = new Language("fr-FR"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10032}}
            });
        }

        void SetDomains5()
        {
            SetupDomainServiceMock(new[]
            {
                new UmbracoDomain("http://domain1.com/en") {Id = 1, Language = new Language("en-US"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10011}},
                new UmbracoDomain("http://domain1a.com/en") {Id = 1, Language = new Language("en-US"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10011}},
                new UmbracoDomain("http://domain1b.com/en") {Id = 1, Language = new Language("en-US"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10011}},
                new UmbracoDomain("http://domain1.com/fr") {Id = 1, Language = new Language("fr-FR"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10012}},
                new UmbracoDomain("http://domain1a.com/fr") {Id = 1, Language = new Language("fr-FR"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10012}},
                new UmbracoDomain("http://domain1b.com/fr") {Id = 1, Language = new Language("fr-FR"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10012}},
                new UmbracoDomain("http://domain3.com/en") {Id = 1, Language = new Language("en-US"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10031}},
                new UmbracoDomain("http://domain3.com/fr") {Id = 1, Language = new Language("fr-FR"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10032}}
            });
        }

        protected override string GetXmlContent(int templateId)
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[ 
<!ELEMENT Doc ANY>
<!ATTLIST Doc id ID #REQUIRED>
]>
<root id=""-1"">
	<Doc id=""1001"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""1001"" writerName=""admin"" creatorName=""admin"" path=""-1,1001"" isDoc="""">
		<content><![CDATA[]]></content>
		<Doc id=""10011"" parentID=""1001"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""1001-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011"" isDoc="""">
			<content><![CDATA[<div>This is some content</div>]]></content>
			<Doc id=""100111"" parentID=""10011"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1001-1-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100111"" isDoc="""">
				<content><![CDATA[]]></content>
			</Doc>
			<Doc id=""100112"" parentID=""10011"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-1-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100112"" isDoc="""">
				<content><![CDATA[]]></content>
				<Doc id=""1001121"" parentID=""100112"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-1-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100112,1001121"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
				<Doc id=""1001122"" parentID=""100112"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-1-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100112,1001122"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
			</Doc>
		</Doc>
		<Doc id=""10012"" parentID=""1001"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""1001-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012"" isDoc="""">
			<content><![CDATA[<div>This is some content</div>]]></content>
			<Doc id=""100121"" parentID=""10012"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1001-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012,100121"" isDoc="""">
				<content><![CDATA[]]></content>
			</Doc>
			<Doc id=""100122"" parentID=""10012"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012,100122"" isDoc="""">
				<content><![CDATA[]]></content>
				<Doc id=""1001221"" parentID=""100122"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-2-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012,100122,1001221"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
				<Doc id=""1001222"" parentID=""100122"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-2-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012,100122,1001222"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
			</Doc>
		</Doc>
		<Doc id=""10013"" parentID=""1001"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""3"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""1001-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10013"" isDoc="""">
		</Doc>
	</Doc>
	<Doc id=""1002"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""3"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""1002"" writerName=""admin"" creatorName=""admin"" path=""-1,1002"" isDoc="""">
	</Doc>
	<Doc id=""1003"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""1003"" writerName=""admin"" creatorName=""admin"" path=""-1,1003"" isDoc="""">
		<content><![CDATA[]]></content>
		<Doc id=""10031"" parentID=""1003"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""1003-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031"" isDoc="""">
			<content><![CDATA[<div>This is some content</div>]]></content>
			<Doc id=""100311"" parentID=""10031"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1003-1-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031,100311"" isDoc="""">
				<content><![CDATA[]]></content>
			</Doc>
			<Doc id=""100312"" parentID=""10031"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-1-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031,100312"" isDoc="""">
				<content><![CDATA[]]></content>
				<Doc id=""1003121"" parentID=""100312"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-1-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031,100312,1003121"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
				<Doc id=""1003122"" parentID=""100312"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-1-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031,100312,1003122"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
			</Doc>
		</Doc>
		<Doc id=""10032"" parentID=""1003"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""1003-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032"" isDoc="""">
			<content><![CDATA[<div>This is some content</div>]]></content>
			<Doc id=""100321"" parentID=""10032"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1003-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032,100321"" isDoc="""">
				<content><![CDATA[]]></content>
			</Doc>
			<Doc id=""100322"" parentID=""10032"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032,100322"" isDoc="""">
				<content><![CDATA[]]></content>
				<Doc id=""1003221"" parentID=""100322"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-2-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032,100322,1003221"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
				<Doc id=""1003222"" parentID=""100322"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-2-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032,100322,1003222"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
			</Doc>
		</Doc>
		<Doc id=""10033"" parentID=""1003"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""3"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""1003-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10033"" isDoc="""">
		</Doc>
	</Doc>
</root>";
        }

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
        public void Get_Nice_Url_SimpleDomain(int nodeId, string currentUrl, bool absolute, string expected)
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var request = Mock.Get(settings.RequestHandler);
            request.Setup(x => x.UseDomainPrefixes).Returns(false);

            var routingContext = GetRoutingContext("/test", 1111, umbracoSettings: settings);

            SettingsForTests.UseDirectoryUrls = true;
            SettingsForTests.HideTopLevelNodeFromPath = false; // ignored w/domains

            SetDomains1();

            var currentUri = new Uri(currentUrl);
            var result = routingContext.UrlProvider.GetUrl(nodeId, currentUri, absolute);
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
        public void Get_Nice_Url_SimpleWithSchemeAndPath(int nodeId, string currentUrl, bool absolute, string expected)
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var request = Mock.Get(settings.RequestHandler);
            request.Setup(x => x.UseDomainPrefixes).Returns(false);

            var routingContext = GetRoutingContext("/test", 1111, umbracoSettings: settings);

            SettingsForTests.UseDirectoryUrls = true;
            SettingsForTests.HideTopLevelNodeFromPath = false; // ignored w/domains

            SetDomains2();

            var currentUri = new Uri(currentUrl);
            var result = routingContext.UrlProvider.GetUrl(nodeId, currentUri, absolute);
            Assert.AreEqual(expected, result);
        }

        // with one domain, not at root
        [TestCase(1001, "http://domain1.com", false, "/1001/")]
        [TestCase(10011, "http://domain1.com", false, "/")]
        [TestCase(100111, "http://domain1.com", false, "/1001-1-1/")]
        [TestCase(1002, "http://domain1.com", false, "/1002/")]
        public void Get_Nice_Url_DeepDomain(int nodeId, string currentUrl, bool absolute, string expected)
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var request = Mock.Get(settings.RequestHandler);
            request.Setup(x => x.UseDomainPrefixes).Returns(false);

            var routingContext = GetRoutingContext("/test", 1111, umbracoSettings: settings);

            SettingsForTests.UseDirectoryUrls = true;
            SettingsForTests.HideTopLevelNodeFromPath = false; // ignored w/domains

            SetDomains3();

            var currentUri = new Uri(currentUrl);
            var result = routingContext.UrlProvider.GetUrl(nodeId, currentUri, absolute);
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
        public void Get_Nice_Url_NestedDomains(int nodeId, string currentUrl, bool absolute, string expected)
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var request = Mock.Get(settings.RequestHandler);
            request.Setup(x => x.UseDomainPrefixes).Returns(false);

            var routingContext = GetRoutingContext("/test", 1111, umbracoSettings: settings);

            SettingsForTests.UseDirectoryUrls = true;
            SettingsForTests.HideTopLevelNodeFromPath = false; // ignored w/domains

            SetDomains4();

            var currentUri = new Uri(currentUrl);
            var result = routingContext.UrlProvider.GetUrl(nodeId, currentUri, absolute);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Get_Nice_Url_DomainsAndCache()
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var request = Mock.Get(settings.RequestHandler);
            request.Setup(x => x.UseDomainPrefixes).Returns(false);

            var routingContext = GetRoutingContext("/test", 1111, umbracoSettings: settings);

            SettingsForTests.UseDirectoryUrls = true;
            SettingsForTests.HideTopLevelNodeFromPath = false; // ignored w/domains

            SetDomains4();

            string ignore;
            ignore = routingContext.UrlProvider.GetUrl(1001, new Uri("http://domain1.com"), false);
            ignore = routingContext.UrlProvider.GetUrl(10011, new Uri("http://domain1.com"), false);
            ignore = routingContext.UrlProvider.GetUrl(100111, new Uri("http://domain1.com"), false);
            ignore = routingContext.UrlProvider.GetUrl(10012, new Uri("http://domain1.com"), false);
            ignore = routingContext.UrlProvider.GetUrl(100121, new Uri("http://domain1.com"), false);
            ignore = routingContext.UrlProvider.GetUrl(10013, new Uri("http://domain1.com"), false);
            ignore = routingContext.UrlProvider.GetUrl(1002, new Uri("http://domain1.com"), false);
            ignore = routingContext.UrlProvider.GetUrl(1001, new Uri("http://domain2.com"), false);
            ignore = routingContext.UrlProvider.GetUrl(10011, new Uri("http://domain2.com"), false);
            ignore = routingContext.UrlProvider.GetUrl(100111, new Uri("http://domain2.com"), false);
            ignore = routingContext.UrlProvider.GetUrl(1002, new Uri("http://domain2.com"), false);

            var cache = routingContext.UmbracoContext.ContentCache.InnerCache as PublishedContentCache;
            if (cache == null) throw new Exception("Unsupported IPublishedContentCache, only the Xml one is supported.");
            var cachedRoutes = cache.RoutesCache.GetCachedRoutes();
            Assert.AreEqual(7, cachedRoutes.Count);

            var cachedIds = cache.RoutesCache.GetCachedIds();
            Assert.AreEqual(7, cachedIds.Count);

            CheckRoute(cachedRoutes, cachedIds, 1001, "1001/");
            CheckRoute(cachedRoutes, cachedIds, 10011, "10011/");
            CheckRoute(cachedRoutes, cachedIds, 100111, "10011/1001-1-1");
            CheckRoute(cachedRoutes, cachedIds, 10012, "10012/");
            CheckRoute(cachedRoutes, cachedIds, 100121, "10012/1001-2-1");
            CheckRoute(cachedRoutes, cachedIds, 10013, "1001/1001-3");
            CheckRoute(cachedRoutes, cachedIds, 1002, "/1002");

            // use the cache
            Assert.AreEqual("/", routingContext.UrlProvider.GetUrl(1001, new Uri("http://domain1.com"), false));
            Assert.AreEqual("/en/", routingContext.UrlProvider.GetUrl(10011, new Uri("http://domain1.com"), false));
            Assert.AreEqual("/en/1001-1-1/", routingContext.UrlProvider.GetUrl(100111, new Uri("http://domain1.com"), false));
            Assert.AreEqual("/fr/", routingContext.UrlProvider.GetUrl(10012, new Uri("http://domain1.com"), false));
            Assert.AreEqual("/fr/1001-2-1/", routingContext.UrlProvider.GetUrl(100121, new Uri("http://domain1.com"), false));
            Assert.AreEqual("/1001-3/", routingContext.UrlProvider.GetUrl(10013, new Uri("http://domain1.com"), false));
            Assert.AreEqual("/1002/", routingContext.UrlProvider.GetUrl(1002, new Uri("http://domain1.com"), false));

            Assert.AreEqual("http://domain1.com/fr/1001-2-1/", routingContext.UrlProvider.GetUrl(100121, new Uri("http://domain2.com"), false));
        }

        void CheckRoute(IDictionary<int, string> routes, IDictionary<string, int> ids, int id, string route)
        {
            Assert.IsTrue(routes.ContainsKey(id));
            Assert.AreEqual(route, routes[id]);
            Assert.IsTrue(ids.ContainsKey(route));
            Assert.AreEqual(id, ids[route]);
        }

        [Test]
        public void Get_Nice_Url_Relative_Or_Absolute()
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var requestMock = Mock.Get(settings.RequestHandler);
            requestMock.Setup(x => x.UseDomainPrefixes).Returns(false);

            var routingContext = GetRoutingContext("http://domain1.com/test", 1111, umbracoSettings: settings);

            SettingsForTests.UseDirectoryUrls = true;
            SettingsForTests.HideTopLevelNodeFromPath = false;

            SetDomains4();

            Assert.AreEqual("/en/1001-1-1/", routingContext.UrlProvider.GetUrl(100111));
            Assert.AreEqual("http://domain3.com/en/1003-1-1/", routingContext.UrlProvider.GetUrl(100311));

            requestMock.Setup(x => x.UseDomainPrefixes).Returns(true);

            Assert.AreEqual("http://domain1.com/en/1001-1-1/", routingContext.UrlProvider.GetUrl(100111));
            Assert.AreEqual("http://domain3.com/en/1003-1-1/", routingContext.UrlProvider.GetUrl(100311));

            requestMock.Setup(x => x.UseDomainPrefixes).Returns(false);
            routingContext.UrlProvider.Mode = UrlProviderMode.Absolute;

            Assert.AreEqual("http://domain1.com/en/1001-1-1/", routingContext.UrlProvider.GetUrl(100111));
            Assert.AreEqual("http://domain3.com/en/1003-1-1/", routingContext.UrlProvider.GetUrl(100311));
        }

        [Test]
        public void Get_Nice_Url_Alternate()
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var routingContext = GetRoutingContext("http://domain1.com/en/test", 1111, umbracoSettings: settings);

            SettingsForTests.UseDirectoryUrls = true;
            SettingsForTests.HideTopLevelNodeFromPath = false;

            SetDomains5();

            var url = routingContext.UrlProvider.GetUrl(100111, true);
            Assert.AreEqual("http://domain1.com/en/1001-1-1/", url);

            var result = routingContext.UrlProvider.GetOtherUrls(100111).ToArray();

            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Contains("http://domain1a.com/en/1001-1-1/"));
            Assert.IsTrue(result.Contains("http://domain1b.com/en/1001-1-1/"));
        }
    }
}
