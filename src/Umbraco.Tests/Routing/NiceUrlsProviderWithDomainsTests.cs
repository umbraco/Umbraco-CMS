using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.language;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class NiceUrlsProviderWithDomainsTests : BaseRoutingTest
	{
		public override void TearDown()
		{
			base.TearDown();

			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", "");
			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "");

			ClearLanguagesAndDomains();
		}

		internal override IRoutesCache GetRoutesCache()
		{
			return new DefaultRoutesCache(false);
		}

		void ClearLanguagesAndDomains()
		{
			var domains = Domain.GetDomains();
			foreach (var d in domains)
				d.Delete();

			var langs = Language.GetAllAsList();
			foreach (var l in langs.Skip(1))
				l.Delete();
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

		void SetDomains1()
		{
			var langEn = Language.GetByCultureCode("en-US");
			var langFr = Language.GetByCultureCode("fr-FR");

			Domain.MakeNew("domain1.com", 1001, langFr.id);
		}

		void SetDomains2()
		{
			var langEn = Language.GetByCultureCode("en-US");
			var langFr = Language.GetByCultureCode("fr-FR");

			Domain.MakeNew("http://domain1.com/foo", 1001, langFr.id);
		}

		void SetDomains3()
		{
			var langEn = Language.GetByCultureCode("en-US");
			var langFr = Language.GetByCultureCode("fr-FR");

			Domain.MakeNew("http://domain1.com/", 10011, langFr.id);
		}

		void SetDomains4()
		{
			var langEn = Language.GetByCultureCode("en-US");
			var langFr = Language.GetByCultureCode("fr-FR");

			Domain.MakeNew("http://domain1.com/", 1001, langFr.id);
			Domain.MakeNew("http://domain1.com/en", 10011, langEn.id);
			Domain.MakeNew("http://domain1.com/fr", 10012, langEn.id);
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
</root>";
		}

		// with one simple domain "domain1.com"
		// basic tests
		[TestCase(1001, "http://domain1.com", false, "/")]
		[TestCase(10011, "http://domain1.com", false, "/1001-1")]
		[TestCase(1002, "http://domain1.com", false, "/1002")]
		// absolute tests
		[TestCase(1001, "http://domain1.com", true, "http://domain1.com/")]
		[TestCase(10011, "http://domain1.com", true, "http://domain1.com/1001-1")]
		// different current tests
		[TestCase(1001, "http://domain2.com", false, "http://domain1.com/")]
		[TestCase(10011, "http://domain2.com", false, "http://domain1.com/1001-1")]
		[TestCase(1001, "https://domain1.com", false, "/")]
		[TestCase(10011, "https://domain1.com", false, "/1001-1")]

		public void Get_Nice_Url_SimpleDomain(int nodeId, string currentUrl, bool absolute, string expected)
		{
			var routingContext = GetRoutingContext("/test", 1111);

			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", "true");
			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "false"); // ignored w/domains

			InitializeLanguagesAndDomains();
			SetDomains1();

			var currentUri = new Uri(currentUrl);
			var result = routingContext.NiceUrlProvider.GetNiceUrl(nodeId, currentUri, absolute);
			Assert.AreEqual(expected, result);
		}

		// with one complete domain "http://domain1.com/foo"
		// basic tests
		[TestCase(1001, "http://domain1.com", false, "/foo")]
		[TestCase(10011, "http://domain1.com", false, "/foo/1001-1")]
		[TestCase(1002, "http://domain1.com", false, "/1002")]
		// absolute tests
		[TestCase(1001, "http://domain1.com", true, "http://domain1.com/foo")]
		[TestCase(10011, "http://domain1.com", true, "http://domain1.com/foo/1001-1")]
		// different current tests
		[TestCase(1001, "http://domain2.com", false, "http://domain1.com/foo")]
		[TestCase(10011, "http://domain2.com", false, "http://domain1.com/foo/1001-1")]
		[TestCase(1001, "https://domain1.com", false, "http://domain1.com/foo")]
		[TestCase(10011, "https://domain1.com", false, "http://domain1.com/foo/1001-1")]

		public void Get_Nice_Url_SimpleWithSchemeAndPath(int nodeId, string currentUrl, bool absolute, string expected)
		{
			var routingContext = GetRoutingContext("/test", 1111);

			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", "true");
			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "false"); // ignored w/domains

			InitializeLanguagesAndDomains();
			SetDomains2();

			var currentUri = new Uri(currentUrl);
			var result = routingContext.NiceUrlProvider.GetNiceUrl(nodeId, currentUri, absolute);
			Assert.AreEqual(expected, result);
		}

		// with one domain, not at root
		[TestCase(1001, "http://domain1.com", false, "/1001")]
		[TestCase(10011, "http://domain1.com", false, "/")]
		[TestCase(100111, "http://domain1.com", false, "/1001-1-1")]
		[TestCase(1002, "http://domain1.com", false, "/1002")]

		public void Get_Nice_Url_DeepDomain(int nodeId, string currentUrl, bool absolute, string expected)
		{
			var routingContext = GetRoutingContext("/test", 1111);

			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", "true");
			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "false"); // ignored w/domains

			InitializeLanguagesAndDomains();
			SetDomains3();

			var currentUri = new Uri(currentUrl);
			var result = routingContext.NiceUrlProvider.GetNiceUrl(nodeId, currentUri, absolute);
			Assert.AreEqual(expected, result);
		}

		// with nested domains
		[TestCase(1001, "http://domain1.com", false, "/")]
		[TestCase(10011, "http://domain1.com", false, "/en")]
		[TestCase(100111, "http://domain1.com", false, "/en/1001-1-1")]
		[TestCase(10012, "http://domain1.com", false, "/fr")]
		[TestCase(100121, "http://domain1.com", false, "/fr/1001-2-1")]
		[TestCase(10013, "http://domain1.com", false, "/1001-3")]
		[TestCase(1002, "http://domain1.com", false, "/1002")]

		public void Get_Nice_Url_NestedDomains(int nodeId, string currentUrl, bool absolute, string expected)
		{
			var routingContext = GetRoutingContext("/test", 1111);

			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", "true");
			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "false"); // ignored w/domains

			InitializeLanguagesAndDomains();
			SetDomains4();

			var currentUri = new Uri(currentUrl);
			var result = routingContext.NiceUrlProvider.GetNiceUrl(nodeId, currentUri, absolute);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void Get_Nice_Url_DomainsAndCache()
		{
			var routingContext = GetRoutingContext("/test", 1111);

			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", "true");
			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "false"); // ignored w/domains

			InitializeLanguagesAndDomains();
			SetDomains4();

			string ignore;
			ignore = routingContext.NiceUrlProvider.GetNiceUrl(1001, new Uri("http://domain1.com"), false);
			ignore = routingContext.NiceUrlProvider.GetNiceUrl(10011, new Uri("http://domain1.com"), false);
			ignore = routingContext.NiceUrlProvider.GetNiceUrl(100111, new Uri("http://domain1.com"), false);
			ignore = routingContext.NiceUrlProvider.GetNiceUrl(10012, new Uri("http://domain1.com"), false);
			ignore = routingContext.NiceUrlProvider.GetNiceUrl(100121, new Uri("http://domain1.com"), false);
			ignore = routingContext.NiceUrlProvider.GetNiceUrl(10013, new Uri("http://domain1.com"), false);
			ignore = routingContext.NiceUrlProvider.GetNiceUrl(1002, new Uri("http://domain1.com"), false);
			ignore = routingContext.NiceUrlProvider.GetNiceUrl(1001, new Uri("http://domain2.com"), false);
			ignore = routingContext.NiceUrlProvider.GetNiceUrl(10011, new Uri("http://domain2.com"), false);
			ignore = routingContext.NiceUrlProvider.GetNiceUrl(100111, new Uri("http://domain2.com"), false);
			ignore = routingContext.NiceUrlProvider.GetNiceUrl(1002, new Uri("http://domain2.com"), false);

			var cachedRoutes = ((DefaultRoutesCache)routingContext.UmbracoContext.RoutesCache).GetCachedRoutes();
			Assert.AreEqual(7, cachedRoutes.Count);

			var cachedIds = ((DefaultRoutesCache)routingContext.UmbracoContext.RoutesCache).GetCachedIds();
			Assert.AreEqual(7, cachedIds.Count);

			CheckRoute(cachedRoutes, cachedIds, 1001, "1001/");
			CheckRoute(cachedRoutes, cachedIds, 10011, "10011/");
			CheckRoute(cachedRoutes, cachedIds, 100111, "10011/1001-1-1");
			CheckRoute(cachedRoutes, cachedIds, 10012, "10012/");
			CheckRoute(cachedRoutes, cachedIds, 100121, "10012/1001-2-1");
			CheckRoute(cachedRoutes, cachedIds, 10013, "1001/1001-3");
			CheckRoute(cachedRoutes, cachedIds, 1002, "/1002");

			// use the cache
			Assert.AreEqual("/", routingContext.NiceUrlProvider.GetNiceUrl(1001, new Uri("http://domain1.com"), false));
			Assert.AreEqual("/en", routingContext.NiceUrlProvider.GetNiceUrl(10011, new Uri("http://domain1.com"), false));
			Assert.AreEqual("/en/1001-1-1", routingContext.NiceUrlProvider.GetNiceUrl(100111, new Uri("http://domain1.com"), false));
			Assert.AreEqual("/fr", routingContext.NiceUrlProvider.GetNiceUrl(10012, new Uri("http://domain1.com"), false));
			Assert.AreEqual("/fr/1001-2-1", routingContext.NiceUrlProvider.GetNiceUrl(100121, new Uri("http://domain1.com"), false));
			Assert.AreEqual("/1001-3", routingContext.NiceUrlProvider.GetNiceUrl(10013, new Uri("http://domain1.com"), false));
			Assert.AreEqual("/1002", routingContext.NiceUrlProvider.GetNiceUrl(1002, new Uri("http://domain1.com"), false));

			Assert.AreEqual("http://domain1.com/fr/1001-2-1", routingContext.NiceUrlProvider.GetNiceUrl(100121, new Uri("http://domain2.com"), false));
		}

		void CheckRoute(IDictionary<int, string> routes, IDictionary<string, int> ids, int id, string route)
		{
			Assert.IsTrue(routes.ContainsKey(id));
			Assert.AreEqual(route, routes[id]);
			Assert.IsTrue(ids.ContainsKey(route));
			Assert.AreEqual(id, ids[route]);
		}
	}
}
