using System.Configuration;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Tests.Stubs;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.DocumentLookups
{
	[TestFixture, RequiresSTA]
	public abstract class BaseTest
	{
		[SetUp]
		public virtual void Initialize()
		{
			TestHelper.SetupLog4NetForTests();
			TestHelper.InitializeDatabase();
			Resolution.Freeze();
		}

		[TearDown]
		public virtual void TearDown()
		{
			//reset the context on global settings
			Umbraco.Core.Configuration.GlobalSettings.HttpContext = null;
			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "");
			Resolution.IsFrozen = false;
			TestHelper.ClearDatabase();
			Cache.ClearAllCache();
		}

		protected FakeHttpContextFactory GetHttpContextFactory(string url)
		{
			var factory = new FakeHttpContextFactory(url);

			//set the state helper
			StateHelper.HttpContext = factory.HttpContext;
			
			return factory;
		}

		private UmbracoContext GetUmbracoContext(string url, Template template)
		{
			var ctx = new UmbracoContext(
				GetHttpContextFactory(url).HttpContext,
				new ApplicationContext(),
				new FakeRoutesCache());
			SetupUmbracoContextForTest(ctx, template);
			return ctx;
		}

		protected RoutingContext GetRoutingContext(string url, Template template)
		{
			var umbracoContext = GetUmbracoContext(url, template);
			var contentStore = new XmlContentStore();
			var niceUrls = new NiceUrlProvider(contentStore, umbracoContext);
			var routingRequest = new RoutingContext(
				umbracoContext,
				Enumerable.Empty<IDocumentLookup>(),
				new FakeLastChanceLookup(),
				contentStore,
				niceUrls);
			return routingRequest;
		}

		/// <summary>
		/// Initlializes the UmbracoContext with specific XML
		/// </summary>
		/// <param name="umbracoContext"></param>
		/// <param name="template"></param>
		protected void SetupUmbracoContextForTest(UmbracoContext umbracoContext, Template template)
		{
			umbracoContext.GetXmlDelegate = () =>
				{
					var xDoc = new XmlDocument();

					//create a custom xml structure to return

					xDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?><!DOCTYPE root[ 
<!ELEMENT Home ANY>
<!ATTLIST Home id ID #REQUIRED>

]>
<root id=""-1"">
	<Home id=""1046"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + template.Id + @""" sortOrder=""2"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""home"" writerName=""admin"" creatorName=""admin"" path=""-1,1046"" isDoc=""""><content><![CDATA[]]></content>
		<Home id=""1173"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + template.Id + @""" sortOrder=""1"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""sub1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173"" isDoc=""""><content><![CDATA[]]></content>
			<Home id=""1174"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + template.Id + @""" sortOrder=""1"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""sub2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1174"" isDoc=""""><content><![CDATA[]]></content>
			</Home>
			<Home id=""1176"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + template.Id + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc=""""><content><![CDATA[]]></content>
			</Home>
		</Home>
		<Home id=""1175"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + template.Id + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1175"" isDoc=""""><content><![CDATA[]]></content>
		</Home>
	</Home>
	<Home id=""1172"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + template.Id + @""" sortOrder=""3"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test"" writerName=""admin"" creatorName=""admin"" path=""-1,1172"" isDoc="""" />
</root>");
					//return the custom x doc
					return xDoc;
				};
		}
	}
}