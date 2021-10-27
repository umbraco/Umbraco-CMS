using System;
using System.IO;
using System.Threading;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class UmbracoModuleTests : BaseWebTest
    {
        private UmbracoInjectedModule _module;

        public override void SetUp()
        {
            base.SetUp();

            // FIXME: be able to get the UmbracoModule from the container. any reason settings were from testobjects?
            //create the module
            var logger = Mock.Of<ILogger>();
            var globalSettings = TestObjects.GetGlobalSettings();
            var runtime = new RuntimeState(logger, Mock.Of<IUmbracoSettingsSection>(), globalSettings,
                new Lazy<IMainDom>(), new Lazy<IServerRegistrar>());

            _module = new UmbracoInjectedModule
            (
                globalSettings,
                runtime,
                logger,
                null, // FIXME: PublishedRouter complexities...
                Mock.Of<IUmbracoContextFactory>(),
                new RoutableDocumentFilter(globalSettings)
            );

            runtime.Level = RuntimeLevel.Run;


            //SettingsForTests.ReservedPaths = "~/umbraco,~/install/";
            //SettingsForTests.ReservedUrls = "~/config/splashes/booting.aspx,~/install/default.aspx,~/config/splashes/noNodes.aspx,~/VSEnterpriseHelper.axd";

        }

        public override void TearDown()
        {
            base.TearDown();

            _module.DisposeIfDisposable();
        }

        // do not test for /base here as it's handled before EnsureUmbracoRoutablePage is called
        [TestCase("/umbraco_client/Tree/treeIcons.css", false)]
        [TestCase("/umbraco_client/Tree/Themes/umbraco/style.css?cdv=37", false)]        
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
            var umbracoContext = GetUmbracoContext(url);

            var result = _module.EnsureUmbracoRoutablePage(umbracoContext, httpContext);

            Assert.AreEqual(assert, result.Success);
        }

        [TestCase("/favicon.ico", true)]
        [TestCase("/umbraco_client/Tree/treeIcons.css", true)]
        [TestCase("/umbraco_client/Tree/Themes/umbraco/style.css?cdv=37", true)]
        [TestCase("/base/somebasehandler", false)]
        [TestCase("/", false)]
        [TestCase("/home.aspx", false)]
        public void Is_Client_Side_Request(string url, bool assert)
        {
            var uri = new Uri("http://test.com" + url);
            var result = uri.IsClientSideRequest();
            Assert.AreEqual(assert, result);
        }

        [Test]
        public void Is_Client_Side_Request_InvalidPath_ReturnFalse()
        {
            //This URL is invalid. Default to false when the extension cannot be determined
            var uri = new Uri("http://test.com/installing-modules+foobar+\"yipee\"");
            var result = uri.IsClientSideRequest();
            Assert.AreEqual(false, result);
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
