using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using SqlCE4Umbraco;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Media.ThumbnailProviders;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.IO;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;
using GlobalSettings = umbraco.GlobalSettings;

namespace Umbraco.Tests
{
	[TestFixture, RequiresSTA]
	public class UmbracoModuleTests
	{
		private UmbracoModule _module;

		[SetUp]
		public void Initialize()
		{
			TestHelper.SetupLog4NetForTests();
			ApplicationContext.Current = new ApplicationContext()
				{
					IsReady = true
				};
			_module = new UmbracoModule();
			ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", Umbraco.Core.Configuration.GlobalSettings.CurrentVersion);
			ConfigurationManager.AppSettings.Set("umbracoReservedPaths", "~/umbraco,~/install/");
			ConfigurationManager.AppSettings.Set("umbracoReservedUrls", "~/config/splashes/booting.aspx,~/install/default.aspx,~/config/splashes/noNodes.aspx,~/VSEnterpriseHelper.axd");
			Cache.ClearAllCache();
			InitializeDatabase();

			//create the not found handlers config
			using(var sw = File.CreateText(IOHelper.MapPath(SystemFiles.NotFoundhandlersConfig, false)))
			{
				sw.Write(@"<NotFoundHandlers>
	<notFound assembly='umbraco' type='SearchForAlias' />
	<notFound assembly='umbraco' type='SearchForTemplate'/>
	<notFound assembly='umbraco' type='SearchForProfile'/>
	<notFound assembly='umbraco' type='handle404'/>
</NotFoundHandlers>");
			}
		}

		[TearDown]
		public void TearDown()
		{
			_module.Dispose();
			//reset the context on global settings
			Umbraco.Core.Configuration.GlobalSettings.HttpContext = null;
			//reset the app context
			ApplicationContext.Current = null;
			//reset the app config
			ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", "");
			ConfigurationManager.AppSettings.Set("umbracoReservedPaths", "");
			ConfigurationManager.AppSettings.Set("umbracoReservedUrls", "");
			ClearDatabase();
			Cache.ClearAllCache();
		}

		private void ClearDatabase()
		{
			var dataHelper = DataLayerHelper.CreateSqlHelper(GlobalSettings.DbDSN) as SqlCEHelper;
			if (dataHelper == null)
				throw new InvalidOperationException("The sql helper for unit tests must be of type SqlCEHelper, check the ensure the connection string used for this test is set to use SQLCE");
			dataHelper.ClearDatabase();
		}

		private void InitializeDatabase()
		{
			ConfigurationManager.AppSettings.Set("umbracoDbDSN", @"datalayer=SQLCE4Umbraco.SqlCEHelper,SQLCE4Umbraco;data source=|DataDirectory|\Umbraco.sdf");

			ClearDatabase();

			var dataHelper = DataLayerHelper.CreateSqlHelper(GlobalSettings.DbDSN);
			var installer = dataHelper.Utility.CreateInstaller();
			if (installer.CanConnect)
			{
				installer.Install();
			}
		}

		[TestCase("/umbraco_client/Tree/treeIcons.css", false)]
		[TestCase("/umbraco_client/Tree/Themes/umbraco/style.css?cdv=37", false)]
		[TestCase("/umbraco_client/scrollingmenu/style.css?cdv=37", false)]
		[TestCase("/umbraco/umbraco.aspx", false)]
		[TestCase("/umbraco/editContent.aspx", false)]
		[TestCase("/install/default.aspx", false)]
		[TestCase("/install/test.aspx", false)]
		[TestCase("/base/somebasehandler", false)]
		[TestCase("/", true)]
		[TestCase("/home.aspx", true)]
		public void Ensure_Request_Routable(string url, bool assert)
		{
			var httpContextFactory = new FakeHttpContextFactory(url);
			var httpContext = httpContextFactory.HttpContext;
			//set the context on global settings
			Umbraco.Core.Configuration.GlobalSettings.HttpContext = httpContext;
			var uri = httpContext.Request.Url;
			var lpath = uri.AbsolutePath.ToLower();

			var result = _module.EnsureUmbracoRoutablePage(uri, lpath, httpContext);

			Assert.AreEqual(assert, result);
		}

		[TestCase("/favicon.ico", true)]
		[TestCase("/umbraco_client/Tree/treeIcons.css", true)]
		[TestCase("/umbraco_client/Tree/Themes/umbraco/style.css?cdv=37", true)]
		[TestCase("/umbraco_client/scrollingmenu/style.css?cdv=37", true)]
		[TestCase("/base/somebasehandler", false)]
		[TestCase("/", false)]
		[TestCase("/home.aspx", false)]
		public void Is_Client_Side_Request(string url, bool assert)
		{
			var uri = new Uri("http://test.com" + url);			
			var result = _module.IsClientSideRequest(uri);
			Assert.AreEqual(assert, result);
		}

		[TestCase("/default.aspx?path=/", true)]
		[TestCase("/default.aspx?path=/home.aspx", true)]
		[TestCase("/default.aspx?path=/home.aspx?altTemplate=blah", true)]
		[TestCase("/default.aspx?p=/home.aspx", false)] //missing path
		[TestCase("/defaul.aspx?path=/home.aspx", false)] //not default path
		public void Process_Front_End_Document_Request(string url, bool assert)
		{
			var httpContextFactory = new FakeHttpContextFactory(url);
			var httpContext = httpContextFactory.HttpContext;
			var umbracoContext = new UmbracoContext(httpContext, ApplicationContext.Current, new DefaultRoutesCache(false));
			var contentStore = new ContentStore(umbracoContext);
			var niceUrls = new NiceUrlProvider(contentStore, umbracoContext);
			umbracoContext.RoutingContext = new RoutingContext(
				new IDocumentLookup[] {new LookupByNiceUrl()},
				new DefaultLastChanceLookup(),
				contentStore,
				niceUrls); 

			StateHelper.HttpContext = httpContext;

			//because of so much dependency on the db, we need to create som stuff here, i originally abstracted out stuff but 
			//was turning out to be quite a deep hole because ultimately we'd have to abstract the old 'Domain' and 'Language' classes
			Domain.MakeNew("Test.com", 1000, Language.GetByCultureCode("en-US").id);

			//need to create a template with id 1045
			var template = Template.MakeNew("test", new User(0));

			SetupUmbracoContextForTest(umbracoContext, template);

			var result = _module.ProcessFrontEndDocumentRequest(
				httpContext,
				umbracoContext);

			Assert.AreEqual(assert, result);
		}


		private void SetupUmbracoContextForTest(UmbracoContext umbracoContext, Template template)
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