using System;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using Umbraco.Core.Services;
using Umbraco.Tests.LegacyXmlPublishedCache;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    public class UrlsWithNestedDomains : UrlRoutingTestBase
    {
        // in the case of nested domains more than 1 URL may resolve to a document
        // but only one route can be cached - the 'canonical' route ie the route
        // using the closest domain to the node - here we test that if we request
        // a non-canonical route, it is not cached / the cache is not polluted

        protected override void Compose()
        {
            base.Compose();
            Composition.RegisterUnique(_ => Mock.Of<IDomainService>());
            Composition.Register<ISiteDomainHelper, SiteDomainHelper>();
        }

        [Test]
        public void DoNotPolluteCache()
        {
            var globalSettings = Mock.Get(Factory.GetInstance<IGlobalSettings>()); //this will modify the IGlobalSettings instance stored in the container
            globalSettings.Setup(x => x.HideTopLevelNodeFromPath).Returns(false);

            var settings = SettingsForTests.GenerateMockUmbracoSettings();

            SetDomains1();

            const string url = "http://domain1.com/1001-1/1001-1-1";

            // get the nice URL for 100111
            var umbracoContext = GetUmbracoContext(url, 9999, umbracoSettings: settings, urlProviders: new []
            {
                new DefaultUrlProvider(settings.RequestHandler, Logger, globalSettings.Object, new SiteDomainHelper())
            }, globalSettings:globalSettings.Object);
            Assert.AreEqual("http://domain2.com/1001-1-1/", umbracoContext.UrlProvider.GetUrl(100111, UrlMode.Absolute));

            // check that the proper route has been cached
            var cache = umbracoContext.Content as PublishedContentCache;
            if (cache == null) throw new Exception("Unsupported IPublishedContentCache, only the Xml one is supported.");
            var cachedRoutes = cache.RoutesCache.GetCachedRoutes();
            Assert.AreEqual("10011/1001-1-1", cachedRoutes[100111]);

            // route a rogue URL
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);

            publishedRouter.FindDomain(frequest);
            Assert.IsTrue(frequest.HasDomain);

            // check that it's been routed
            var lookup = new ContentFinderByUrl(Logger);
            var result = lookup.TryFindContent(frequest);
            Assert.IsTrue(result);
            Assert.AreEqual(100111, frequest.PublishedContent.Id);

            // has the cache been polluted?
            cachedRoutes = cache.RoutesCache.GetCachedRoutes();
            Assert.AreEqual("10011/1001-1-1", cachedRoutes[100111]); // no
            //Assert.AreEqual("1001/1001-1/1001-1-1", cachedRoutes[100111]); // yes

            // what's the nice URL now?
            Assert.AreEqual("http://domain2.com/1001-1-1/", umbracoContext.UrlProvider.GetUrl(100111)); // good
            //Assert.AreEqual("http://domain1.com/1001-1/1001-1-1", routingContext.NiceUrlProvider.GetNiceUrl(100111, true)); // bad
        }

        void SetDomains1()
        {
            SetupDomainServiceMock(new[]
            {
                new UmbracoDomain("http://domain1.com/") {Id = 1, LanguageId = LangEngId, RootContentId = 1001, LanguageIsoCode = "en-US"},
                new UmbracoDomain("http://domain2.com/") {Id = 1, LanguageId = LangEngId, RootContentId = 10011, LanguageIsoCode = "en-US"}
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
    }
}
