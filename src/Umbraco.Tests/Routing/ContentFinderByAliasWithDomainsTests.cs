using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    public class ContentFinderByAliasWithDomainsTests : ContentFinderByAliasTests
    {
        private PublishedContentType _publishedContentType;

        protected override void Initialize()
        {
            base.Initialize();

            var properties = new[]
            {
                new PublishedPropertyType("umbracoUrlAlias", Constants.DataTypes.Textbox, false, ContentVariation.Nothing,
                    new PropertyValueConverterCollection(Enumerable.Empty<IPropertyValueConverter>()),
                    Mock.Of<IPublishedModelFactory>(),
                    Mock.Of<IPublishedContentTypeFactory>()),
            };
            _publishedContentType = new PublishedContentType(Guid.NewGuid(), 0, "Doc", PublishedItemType.Content, Enumerable.Empty<string>(), properties, ContentVariation.Nothing);
        }

        protected override PublishedContentType GetPublishedContentTypeByAlias(string alias)
        {
            if (alias == "Doc") return _publishedContentType;
            return null;
        }

        void SetDomains1()
        {
            SetupDomainServiceMock(new[]
            {
                // No culture
                new UmbracoDomain("http://domain1.com/") {Id = 1, LanguageId = LangDeId, RootContentId = 1001, LanguageIsoCode = "de-DE"},

                // Cultures organized by domains with one level paths
                new UmbracoDomain("http://domain2.com/de") {Id = 1, LanguageId = LangDeId, RootContentId = 1002, LanguageIsoCode = "de-DE"},
                new UmbracoDomain("http://domain2.com/en") {Id = 1, LanguageId = LangEngId, RootContentId = 1002, LanguageIsoCode = "en-US"},

                // Cultures organized by sub-domains
                new UmbracoDomain("http://de.domain3.com") {Id = 1, LanguageId = LangDeId, RootContentId = 1003, LanguageIsoCode = "de-DE"},
                new UmbracoDomain("http://en.domain3.com") {Id = 1, LanguageId = LangEngId, RootContentId = 1003, LanguageIsoCode = "en-US"},

                // Domain with port
                new UmbracoDomain("http://domain4.com:8080") {Id = 1, LanguageId = LangDeId, RootContentId = 1004, LanguageIsoCode = "de-DE"},
            });
        }

        [TestCase("http://domain1.com/this/is/my/wrong/alias", "de-DE", -1001)] // Alias does not exist
        [TestCase("http://domain1.com/this/is/my/alias", "de-DE", 1001)] // Alias exists
        [TestCase("http://domain1.com/myotheralias", "de-DE", 1001)] // Alias exists
        [TestCase("http://domain1.com/page2/alias", "de-DE", 10011)] // alias to sub-page works
        [TestCase("http://domain1.com/endanger", "de-DE", 10011)] // alias to sub-page works, even with "en..."
        [TestCase("http://domain1.com/en/flux", "en-US", -10011)] // alias to domain's page fails - no /en alias on domain's home

        [TestCase("http://domain2.com/test212", "de-DE", -1002)] // Alias does not exist
        [TestCase("http://domain2.com/de/test1", "de-DE", 1002)] // Alias exists
        [TestCase("http://domain2.com/de/foo/bar", "de-DE", 1002)] // Alias exists
        [TestCase("http://domain2.com/de/page2/alias", "de-DE", 10021)] // alias to sub-page works
        [TestCase("http://domain2.com/en/test1", "en-US", 1002)] // Alias exists
        [TestCase("http://domain2.com/en/foo/bar", "en-US", 1002)] // Alias exists

        [TestCase("http://de.domain3.com/test1", "de-DE", 1003)] // Alias exists
        [TestCase("http://de.domain3.com/page2/alias", "de-DE", 10031)] // alias to sub-page works
        [TestCase("http://en.domain3.com/test1", "en-US", 1003)] // Alias exists
        [TestCase("http://en.domain3.com/test4", "en-US", -1003)] // Alias does not exist

        [TestCase("http://domain4.com:8080/test5", "de-DE", 1004)] // Alias exists
        public void Lookup_By_Url_Alias_And_Domain(string inputUrl, string expectedCulture, int expectedNode)
        {
            SetDomains1();

            var globalSettings = Mock.Get(Factory.GetInstance<IGlobalSettings>()); //this will modify the IGlobalSettings instance stored in the container
            globalSettings.Setup(x => x.HideTopLevelNodeFromPath).Returns(false);

            var umbracoSettings = Current.Configs.Settings();

            var umbracoContext = GetUmbracoContext(inputUrl, urlProviders: new[]
            {
                new DefaultUrlProvider(umbracoSettings.RequestHandler, Logger, globalSettings.Object, new SiteDomainHelper())
            }, globalSettings: globalSettings.Object);

            var publishedRouter = CreatePublishedRouter();
            var request = publishedRouter.CreateRequest(umbracoContext);
            // must lookup domain
            publishedRouter.FindDomain(request);

            if (expectedNode > 0)
                Assert.AreEqual(expectedCulture, request.Culture.Name);

            var finder = new ContentFinderByUrlAlias(Logger);
            var result = finder.TryFindContent(request);

            if (expectedNode > 0)
            {
                Assert.IsTrue(result);
                Assert.AreEqual(request.PublishedContent.Id, expectedNode);
            }
            else
            {
                Assert.IsFalse(result);
            }
        }

        protected override string GetXmlContent(int templateId)
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[
<!ELEMENT Doc ANY>
<!ATTLIST Doc id ID #REQUIRED>
]>
<root id=""-1"">

    <Doc id=""1001"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""1001"" writerName=""admin"" creatorName=""admin"" path=""-1,1001"" isDoc="""">
        <content><![CDATA[]]></content>
        <umbracoUrlAlias><![CDATA[this/is/my/alias, myotheralias]]></umbracoUrlAlias>

        <Doc id=""10011"" parentID=""1001"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""1001-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011"" isDoc="""">
            <content><![CDATA[<div>This is some content</div>]]></content>
            <umbracoUrlAlias><![CDATA[page2/alias, 2ndpagealias, flux, endanger]]></umbracoUrlAlias>

            <Doc id=""100111"" parentID=""10011"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1001-1-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100111"" isDoc="""">
                <content><![CDATA[]]></content>
                <umbracoUrlAlias><![CDATA[only/one/alias, entropy, bar/foo, bar/nil]]></umbracoUrlAlias>
            </Doc>
        </Doc>
    </Doc>


    <Doc id=""1002"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""1002"" writerName=""admin"" creatorName=""admin"" path=""-1,1002"" isDoc="""">
        <content><![CDATA[]]></content>
        <umbracoUrlAlias><![CDATA[test1, foo/bar]]></umbracoUrlAlias>

        <Doc id=""10021"" parentID=""1002"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""1002-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1002,10021"" isDoc="""">
            <content><![CDATA[<div>This is some content</div>]]></content>
            <umbracoUrlAlias><![CDATA[page2/alias]]></umbracoUrlAlias>
        </Doc>
    </Doc>


    <Doc id=""1003"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""1003"" writerName=""admin"" creatorName=""admin"" path=""-1,1003"" isDoc="""">
        <content><![CDATA[]]></content>
        <umbracoUrlAlias><![CDATA[test1, bar/foo]]></umbracoUrlAlias>

        <Doc id=""10031"" parentID=""1003"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""1003-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031"" isDoc="""">
            <content><![CDATA[<div>This is some content</div>]]></content>
            <umbracoUrlAlias><![CDATA[page2/alias]]></umbracoUrlAlias>

            <Doc id=""100311"" parentID=""10031"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1003-1-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031,100311"" isDoc="""">
                <content><![CDATA[]]></content>
            </Doc>
        </Doc>
    </Doc>


    <Doc id=""1004"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""1004"" writerName=""admin"" creatorName=""admin"" path=""-1,1004"" isDoc="""">
        <content><![CDATA[]]></content>
        <umbracoUrlAlias><![CDATA[test5]]></umbracoUrlAlias>

        <Doc id=""10041"" parentID=""1004"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""1004-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1004,10041"" isDoc="""">
            <content><![CDATA[<div>This is some content</div>]]></content>
            <umbracoUrlAlias><![CDATA[page2/alias]]></umbracoUrlAlias>
        </Doc>
    </Doc>


</root>";
        }
    }
}
