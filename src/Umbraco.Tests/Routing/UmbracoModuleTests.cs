using System;
using System.Configuration;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using umbraco.IO;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.Routing
{
	[TestFixture, RequiresSTA]
	public class UmbracoModuleTests : BaseRoutingTest
	{
		private UmbracoModule _module;

		public override void Initialize()
		{
			base.Initialize();
			
			//the module requires that the singleton is setup
			ApplicationContext.Current = ApplicationContext;

			//create the module
			_module = new UmbracoModule();

			ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", Umbraco.Core.Configuration.GlobalSettings.CurrentVersion);
			ConfigurationManager.AppSettings.Set("umbracoReservedPaths", "~/umbraco,~/install/");
			ConfigurationManager.AppSettings.Set("umbracoReservedUrls", "~/config/splashes/booting.aspx,~/install/default.aspx,~/config/splashes/noNodes.aspx,~/VSEnterpriseHelper.axd");

			//create the not found handlers config
			using (var sw = File.CreateText(Umbraco.Core.IO.IOHelper.MapPath(Umbraco.Core.IO.SystemFiles.NotFoundhandlersConfig, false)))
			{
				sw.Write(@"<NotFoundHandlers>
	<notFound assembly='umbraco' type='SearchForAlias' />
	<notFound assembly='umbraco' type='SearchForTemplate'/>
	<notFound assembly='umbraco' type='SearchForProfile'/>
	<notFound assembly='umbraco' type='handle404'/>
</NotFoundHandlers>");
			}
		}

		public override void TearDown()
		{
			base.TearDown();

			_module.DisposeIfDisposable();
			
			//reset the app config
			ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", "");
			ConfigurationManager.AppSettings.Set("umbracoReservedPaths", "");
			ConfigurationManager.AppSettings.Set("umbracoReservedUrls", "");		
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
		[TestCase("/umbraco-test", true)]
		[TestCase("/install-test", true)]
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




		//NOTE: This test shows how we can test most of the HttpModule, it however is testing a method that no longer exists and is testing too much, 
		// we need to write unit tests for each of the components: NiceUrlProvider, all of the Lookup classes, etc...
		// to ensure that each one is individually tested. 

		//[TestCase("/", 1046)]
		//[TestCase("/home.aspx", 1046)]
		//[TestCase("/home/sub1.aspx", 1173)]
		//[TestCase("/home.aspx?altTemplate=blah", 1046)]
		//public void Process_Front_End_Document_Request_Match_Node(string url, int nodeId)
		//{
		//    var httpContextFactory = new FakeHttpContextFactory(url);
		//    var httpContext = httpContextFactory.HttpContext;
		//    var umbracoContext = new UmbracoContext(httpContext, ApplicationContext.Current, new NullRoutesCache());
		//    var contentStore = new ContentStore(umbracoContext);
		//    var niceUrls = new NiceUrlProvider(contentStore, umbracoContext);
		//    umbracoContext.RoutingContext = new RoutingContext(
		//        new IDocumentLookup[] {new LookupByNiceUrl()},
		//        new DefaultLastChanceLookup(),
		//        contentStore,
		//        niceUrls); 

		//    StateHelper.HttpContext = httpContext;

		//    //because of so much dependency on the db, we need to create som stuff here, i originally abstracted out stuff but 
		//    //was turning out to be quite a deep hole because ultimately we'd have to abstract the old 'Domain' and 'Language' classes
		//    Domain.MakeNew("Test.com", 1000, Language.GetByCultureCode("en-US").id);

		//    //need to create a template with id 1045
		//    var template = Template.MakeNew("test", new User(0));

		//    SetupUmbracoContextForTest(umbracoContext, template);

		//    _module.AssignDocumentRequest(httpContext, umbracoContext, httpContext.Request.Url);

		//    Assert.IsNotNull(umbracoContext.DocumentRequest);
		//    Assert.IsNotNull(umbracoContext.DocumentRequest.XmlNode);	
		//    Assert.IsFalse(umbracoContext.DocumentRequest.IsRedirect);
		//    Assert.IsFalse(umbracoContext.DocumentRequest.Is404);
		//    Assert.AreEqual(umbracoContext.DocumentRequest.Culture, Thread.CurrentThread.CurrentCulture);
		//    Assert.AreEqual(umbracoContext.DocumentRequest.Culture, Thread.CurrentThread.CurrentUICulture);
		//    Assert.AreEqual(nodeId, umbracoContext.DocumentRequest.NodeId);
			
		//}

	}
}