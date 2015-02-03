using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic.web;
using System.Configuration;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    class DomainsAndCulturesTests : UrlRoutingTestBase
    {
        void SetDomains1()
        {
            SetupDomainServiceMock(new[]
            {
                new UmbracoDomain("domain1.com/") {Id = 1, Language = new Language("de-DE"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 1001}},
                new UmbracoDomain("domain1.com/en") {Id = 1, Language = new Language("en-US"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10011}},
                new UmbracoDomain("domain1.com/fr") {Id = 1, Language = new Language("fr-FR"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10012}}
            });
        }

        void SetDomains2()
        {
            SetupDomainServiceMock(new[]
            {
                new UmbracoDomain("domain1.com/") {Id = 1, Language = new Language("de-DE"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 1001}},
                new UmbracoDomain("domain1.com/en") {Id = 1, Language = new Language("en-US"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10011}},
                new UmbracoDomain("domain1.com/fr") {Id = 1, Language = new Language("fr-FR"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10012}},
                new UmbracoDomain("*1001") {Id = 1, Language = new Language("de-DE"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 1001}},
                new UmbracoDomain("*10011") {Id = 1, Language = new Language("cs-CZ"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10011}},
                new UmbracoDomain("*100112") {Id = 1, Language = new Language("nl-NL"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 100112}},
                new UmbracoDomain("*1001122") {Id = 1, Language = new Language("da-DK"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 1001122}},
                new UmbracoDomain("*10012") {Id = 1, Language = new Language("nl-NL"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10012}},
                new UmbracoDomain("*10031") {Id = 1, Language = new Language("nl-NL"), RootContent = new Content("test1", -1, new ContentType(-1)) {Id = 10031}}
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
	<Doc id=""1001"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""1001"" writerName=""admin"" creatorName=""admin"" path=""-1,1001"" isDoc="""">
		<content><![CDATA[]]></content>
        <umbracoUrlAlias><![CDATA[this/is/my/alias, anotheralias]]></umbracoUrlAlias>
		<Doc id=""10011"" parentID=""1001"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""1001-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011"" isDoc="""">
			<content><![CDATA[<div>This is some content</div>]]></content>
            <umbracoUrlAlias><![CDATA[page2/alias, 2ndpagealias, en/flux, endanger]]></umbracoUrlAlias>
			<Doc id=""100111"" parentID=""10011"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1001-1-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100111"" isDoc="""">
				<content><![CDATA[]]></content>
                <umbracoUrlAlias><![CDATA[only/one/alias, entropy, bar/foo, en/bar/nil]]></umbracoUrlAlias>
			</Doc>
			<Doc id=""100112"" parentID=""10011"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-1-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100112"" isDoc="""">
				<content><![CDATA[]]></content>
				<Doc id=""1001121"" parentID=""100112"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-1-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100112,1001121"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
				<Doc id=""1001122"" parentID=""100112"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-1-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100112,1001122"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
			</Doc>
		</Doc>
		<Doc id=""10012"" parentID=""1001"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""1001-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012"" isDoc="""">
			<content><![CDATA[<div>This is some content</div>]]></content>
            <umbracoUrlAlias><![CDATA[alias42]]></umbracoUrlAlias>
			<Doc id=""100121"" parentID=""10012"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1001-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012,100121"" isDoc="""">
				<content><![CDATA[]]></content>
                <umbracoUrlAlias><![CDATA[alias43]]></umbracoUrlAlias>
			</Doc>
			<Doc id=""100122"" parentID=""10012"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012,100122"" isDoc="""">
				<content><![CDATA[]]></content>
				<Doc id=""1001221"" parentID=""100122"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-2-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012,100122,1001221"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
				<Doc id=""1001222"" parentID=""100122"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1001-2-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012,100122,1001222"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
			</Doc>
		</Doc>
		<Doc id=""10013"" parentID=""1001"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""1001-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10013"" isDoc="""">
            <umbracoUrlAlias><![CDATA[alias42]]></umbracoUrlAlias>
		</Doc>
	</Doc>
	<Doc id=""1002"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""1002"" writerName=""admin"" creatorName=""admin"" path=""-1,1002"" isDoc="""">
	</Doc>
	<Doc id=""1003"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""1003"" writerName=""admin"" creatorName=""admin"" path=""-1,1003"" isDoc="""">
		<content><![CDATA[]]></content>
		<Doc id=""10031"" parentID=""1003"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""1003-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031"" isDoc="""">
			<content><![CDATA[<div>This is some content</div>]]></content>
			<Doc id=""100311"" parentID=""10031"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1003-1-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031,100311"" isDoc="""">
				<content><![CDATA[]]></content>
			</Doc>
			<Doc id=""100312"" parentID=""10031"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-1-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031,100312"" isDoc="""">
				<content><![CDATA[]]></content>
				<Doc id=""1003121"" parentID=""100312"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-1-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031,100312,1003121"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
				<Doc id=""1003122"" parentID=""100312"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-1-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10031,100312,1003122"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
			</Doc>
		</Doc>
		<Doc id=""10032"" parentID=""1003"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""1003-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032"" isDoc="""">
			<content><![CDATA[<div>This is some content</div>]]></content>
			<Doc id=""100321"" parentID=""10032"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1003-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032,100321"" isDoc="""">
				<content><![CDATA[]]></content>
			</Doc>
			<Doc id=""100322"" parentID=""10032"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032,100322"" isDoc="""">
				<content><![CDATA[]]></content>
				<Doc id=""1003221"" parentID=""100322"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-2-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032,100322,1003221"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
				<Doc id=""1003222"" parentID=""100322"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""1003-2-2-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10032,100322,1003222"" isDoc="""">
					<content><![CDATA[]]></content>
				</Doc>
			</Doc>
		</Doc>
		<Doc id=""10033"" parentID=""1003"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""1003-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1003,10033"" isDoc="""">
		</Doc>
	</Doc>
</root>";
        }

        #region Cases
        [TestCase("http://domain1.com/", "de-DE", 1001)]
        [TestCase("http://domain1.com/1001-1", "de-DE", 10011)]
        [TestCase("http://domain1.com/1001-1/1001-1-1", "de-DE", 100111)]
        [TestCase("http://domain1.com/en", "en-US", 10011)]
        [TestCase("http://domain1.com/en/1001-1-1", "en-US", 100111)]
        [TestCase("http://domain1.com/fr", "fr-FR", 10012)]
        [TestCase("http://domain1.com/fr/1001-2-1", "fr-FR", 100121)]
        #endregion
        public void DomainAndCulture(string inputUrl, string expectedCulture, int expectedNode)
        {
            SetDomains1();

            var routingContext = GetRoutingContext(inputUrl);
            var url = routingContext.UmbracoContext.CleanedUmbracoUrl; //very important to use the cleaned up umbraco url
            var pcr = new PublishedContentRequest(url, routingContext);

            // lookup domain
            pcr.Engine.FindDomain();

            Assert.AreEqual(expectedCulture, pcr.Culture.Name);

            SettingsForTests.HideTopLevelNodeFromPath = false; 
            var finder = new ContentFinderByNiceUrl();
            var result = finder.TryFindContent(pcr);

            Assert.IsTrue(result);
            Assert.AreEqual(pcr.PublishedContent.Id, expectedNode);
        }

        #region Cases
        [TestCase("http://domain1.com/", "de-DE", 1001)] // domain takes over local wildcard at 1001
        [TestCase("http://domain1.com/1001-1", "cs-CZ", 10011)] // wildcard on 10011 applies
        [TestCase("http://domain1.com/1001-1/1001-1-1", "cs-CZ", 100111)] // wildcard on 10011 applies
        [TestCase("http://domain1.com/1001-1/1001-1-2", "nl-NL", 100112)] // wildcard on 100112 applies
        [TestCase("http://domain1.com/1001-1/1001-1-2/1001-1-2-1", "nl-NL", 1001121)] // wildcard on 100112 applies
        [TestCase("http://domain1.com/1001-1/1001-1-2/1001-1-2-2", "da-DK", 1001122)] // wildcard on 1001122 applies

        [TestCase("http://domain1.com/1001-2", "nl-NL", 10012)] // wildcard on 10012 applies
        [TestCase("http://domain1.com/1001-2/1001-2-1", "nl-NL", 100121)] // wildcard on 10012 applies
        [TestCase("http://domain1.com/en", "en-US", 10011)] // domain takes over local wildcard at 10011
        [TestCase("http://domain1.com/en/1001-1-1", "en-US", 100111)] // domain takes over local wildcard at 10011
        [TestCase("http://domain1.com/en/1001-1-2", "nl-NL", 100112)] // wildcard on 100112 applies
        [TestCase("http://domain1.com/en/1001-1-2/1001-1-2-1", "nl-NL", 1001121)] // wildcard on 100112 applies
        [TestCase("http://domain1.com/en/1001-1-2/1001-1-2-2", "da-DK", 1001122)] // wildcard on 1001122 applies

        [TestCase("http://domain1.com/fr", "fr-FR", 10012)] // domain takes over local wildcard at 10012
        [TestCase("http://domain1.com/fr/1001-2-1", "fr-FR", 100121)] // domain takes over local wildcard at 10012

        [TestCase("/1003", null, 1003)] // default culture (no domain)
        [TestCase("/1003/1003-1", "nl-NL", 10031)] // wildcard on 10031 applies
        [TestCase("/1003/1003-1/1003-1-1", "nl-NL", 100311)] // wildcard on 10031 applies
        #endregion
        public void DomainAndCultureWithWildcards(string inputUrl, string expectedCulture, int expectedNode)
        {
            SetDomains2();

            // defaults depend on test environment
            expectedCulture = expectedCulture ?? System.Threading.Thread.CurrentThread.CurrentUICulture.Name;

            var routingContext = GetRoutingContext(inputUrl);
            var url = routingContext.UmbracoContext.CleanedUmbracoUrl; //very important to use the cleaned up umbraco url
            var pcr = new PublishedContentRequest(url, routingContext);

            // lookup domain
            pcr.Engine.FindDomain();

            // find document
            SettingsForTests.HideTopLevelNodeFromPath = false;
            var finder = new ContentFinderByNiceUrl();
            var result = finder.TryFindContent(pcr);

            // apply wildcard domain
            pcr.Engine.HandleWildcardDomains();

            Assert.IsTrue(result);
            Assert.AreEqual(expectedCulture, pcr.Culture.Name);
            Assert.AreEqual(pcr.PublishedContent.Id, expectedNode);
        }
    }
}
