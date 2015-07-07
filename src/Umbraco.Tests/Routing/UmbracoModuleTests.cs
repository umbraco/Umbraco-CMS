using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Xml;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using Umbraco.Core.IO;
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
			
			//create the module
			_module = new UmbracoModule();

		    SettingsForTests.ConfigurationStatus = UmbracoVersion.GetSemanticVersion().ToSemanticString();
            //SettingsForTests.ReservedPaths = "~/umbraco,~/install/";
            //SettingsForTests.ReservedUrls = "~/config/splashes/booting.aspx,~/install/default.aspx,~/config/splashes/noNodes.aspx,~/VSEnterpriseHelper.axd";

		    Directory.CreateDirectory(Path.GetDirectoryName(IOHelper.MapPath(SystemFiles.NotFoundhandlersConfig, false)));

			//create the not found handlers config
			using (var sw = File.CreateText(IOHelper.MapPath(SystemFiles.NotFoundhandlersConfig, false)))
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
		}

		// do not test for /base here as it's handled before EnsureUmbracoRoutablePage is called
		[TestCase("/umbraco_client/Tree/treeIcons.css", false)]
		[TestCase("/umbraco_client/Tree/Themes/umbraco/style.css?cdv=37", false)]
		[TestCase("/umbraco_client/scrollingmenu/style.css?cdv=37", false)]
		[TestCase("/umbraco/umbraco.aspx", false)]
		[TestCase("/umbraco/editContent.aspx", false)]
		[TestCase("/install/default.aspx", false)]
		[TestCase("/install/?installStep=license", false)]
		[TestCase("/install?installStep=license", false)]
		[TestCase("/install/test.aspx", false)]
		[TestCase("/config/splashes/noNodes.aspx", false)]
		[TestCase("/", true)]
		[TestCase("/home.aspx", true)]
		[TestCase("/umbraco-test", true)]
		[TestCase("/install-test", true)]
		public void Ensure_Request_Routable(string url, bool assert)
		{
			var httpContextFactory = new FakeHttpContextFactory(url);
			var httpContext = httpContextFactory.HttpContext;
			var routingContext = GetRoutingContext(url);
			var umbracoContext = routingContext.UmbracoContext;
			
			var result = _module.EnsureUmbracoRoutablePage(umbracoContext, httpContext);

			Assert.AreEqual(assert, result.Success);
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
			var result = uri.IsClientSideRequest();
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
		//        new IPublishedContentLookup[] {new LookupByNiceUrl()},
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

		//    Assert.IsNotNull(umbracoContext.PublishedContentRequest);
		//    Assert.IsNotNull(umbracoContext.PublishedContentRequest.XmlNode);	
		//    Assert.IsFalse(umbracoContext.PublishedContentRequest.IsRedirect);
		//    Assert.IsFalse(umbracoContext.PublishedContentRequest.Is404);
		//    Assert.AreEqual(umbracoContext.PublishedContentRequest.Culture, Thread.CurrentThread.CurrentCulture);
		//    Assert.AreEqual(umbracoContext.PublishedContentRequest.Culture, Thread.CurrentThread.CurrentUICulture);
		//    Assert.AreEqual(nodeId, umbracoContext.PublishedContentRequest.NodeId);
			
		//}

	}
}