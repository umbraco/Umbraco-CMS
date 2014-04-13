﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.language;
using System.Configuration;

namespace Umbraco.Tests.Routing
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
	[TestFixture]
	public class ContentFinderByNiceUrlWithDomainsTests : BaseRoutingTest
	{
		public override void Initialize()
		{
			base.Initialize();

            // ensure we can create them although the content is not in the database
            TestHelper.DropForeignKeys("umbracoDomains");
            
            InitializeLanguagesAndDomains();
		}

		void InitializeLanguagesAndDomains()
		{
			var domains = Domain.GetDomains();
			foreach (var d in domains)
				d.Delete();

			var langs = Language.GetAllAsList();
			foreach (var l in langs.Skip(1))
				l.Delete();

			Language.MakeNew("fr-FR");
		}

		void SetDomains3()
		{
			var langEn = Language.GetByCultureCode("en-US");
			var langFr = Language.GetByCultureCode("fr-FR");

			Domain.MakeNew("domain1.com/", 1001, langEn.id);
		}

		void SetDomains4()
		{
			var langEn = Language.GetByCultureCode("en-US");
			var langFr = Language.GetByCultureCode("fr-FR");

			Domain.MakeNew("domain1.com/", 1001, langEn.id);
			Domain.MakeNew("domain1.com/en", 10011, langEn.id);
			Domain.MakeNew("domain1.com/fr", 10012, langFr.id);

			Domain.MakeNew("http://domain3.com/", 1003, langEn.id);
			Domain.MakeNew("http://domain3.com/en", 10031, langEn.id);
			Domain.MakeNew("http://domain3.com/fr", 10032, langFr.id);
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
		<Doc id=""10011"" parentID=""1001"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""1001-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011"" isDoc="""">
			<content><![CDATA[<div>This is some content</div>]]></content>
			<Doc id=""100111"" parentID=""10011"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1001-1-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10011,100111"" isDoc="""">
				<content><![CDATA[]]></content>
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
			<Doc id=""100121"" parentID=""10012"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""1001-2-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1001,10012,100121"" isDoc="""">
				<content><![CDATA[]]></content>
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

		[TestCase("http://domain1.com/", 1001)]
		[TestCase("http://domain1.com/1001-1", 10011)]
		[TestCase("http://domain1.com/1001-2/1001-2-1", 100121)]

		public void Lookup_SingleDomain(string url, int expectedId)
		{
			SetDomains3();

		    SettingsForTests.HideTopLevelNodeFromPath = true;

			var routingContext = GetRoutingContext(url);
			var uri = routingContext.UmbracoContext.CleanedUmbracoUrl; //very important to use the cleaned up umbraco url
			var pcr = new PublishedContentRequest(uri, routingContext);

			// must lookup domain else lookup by url fails
			pcr.Engine.FindDomain();

			var lookup = new ContentFinderByNiceUrl();
			var result = lookup.TryFindContent(pcr);
			Assert.IsTrue(result);
			Assert.AreEqual(expectedId, pcr.PublishedContent.Id);
		}

		[TestCase("http://domain1.com/", 1001, "en-US")]
		[TestCase("http://domain1.com/en", 10011, "en-US")]
		[TestCase("http://domain1.com/en/1001-1-1", 100111, "en-US")]
		[TestCase("http://domain1.com/fr", 10012, "fr-FR")]
		[TestCase("http://domain1.com/fr/1001-2-1", 100121, "fr-FR")]
		[TestCase("http://domain1.com/1001-3", 10013, "en-US")]

		[TestCase("http://domain2.com/1002", 1002, "en-US")]

		[TestCase("http://domain3.com/", 1003, "en-US")]
		[TestCase("http://domain3.com/en", 10031, "en-US")]
		[TestCase("http://domain3.com/en/1003-1-1", 100311, "en-US")]
		[TestCase("http://domain3.com/fr", 10032, "fr-FR")]
		[TestCase("http://domain3.com/fr/1003-2-1", 100321, "fr-FR")]
		[TestCase("http://domain3.com/1003-3", 10033, "en-US")]

		[TestCase("https://domain1.com/", 1001, "en-US")]
		[TestCase("https://domain3.com/", 1001, "en-US")] // because domain3 is explicitely set on http

		public void Lookup_NestedDomains(string url, int expectedId, string expectedCulture)
		{
			SetDomains4();

            SettingsForTests.HideTopLevelNodeFromPath = true;

			var routingContext = GetRoutingContext(url);
			var uri = routingContext.UmbracoContext.CleanedUmbracoUrl; //very important to use the cleaned up umbraco url
			var pcr = new PublishedContentRequest(uri, routingContext);

			// must lookup domain else lookup by url fails
			pcr.Engine.FindDomain();
			Assert.AreEqual(expectedCulture, pcr.Culture.Name);

			var lookup = new ContentFinderByNiceUrl();
			var result = lookup.TryFindContent(pcr);
			Assert.IsTrue(result);
			Assert.AreEqual(expectedId, pcr.PublishedContent.Id);
		}
	}
}
