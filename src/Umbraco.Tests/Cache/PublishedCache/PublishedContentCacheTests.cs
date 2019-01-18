using System.Linq;
using System.Xml;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Cache.PublishedCache
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class PublishContentCacheTests : BaseWebTest
    {
        private FakeHttpContextFactory _httpContextFactory;
        private UmbracoContext _umbracoContext;
        private IPublishedContentCache _cache;
        private XmlDocument _xml;

        private string GetXml()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?><!DOCTYPE root[
<!ELEMENT Home ANY>
<!ATTLIST Home id ID #REQUIRED>

]>
<root id=""-1"">
    <Home id=""1046"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""2"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""home"" writerName=""admin"" creatorName=""admin"" path=""-1,1046"" isDoc=""""><content><![CDATA[]]></content>
        <Home id=""1173"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""1"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""sub1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173"" isDoc=""""><content><![CDATA[]]></content>
            <Home id=""1174"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""1"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""sub2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1174"" isDoc=""""><content><![CDATA[]]></content>
            </Home>
            <Home id=""1176"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc=""""><content><![CDATA[]]></content>
            </Home>
        </Home>
        <Home id=""1175"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""2"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1175"" isDoc=""""><content><![CDATA[]]></content>
        </Home>
        <Home id=""1177"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""2"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub'Apostrophe"" urlName=""sub'apostrophe"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1177"" isDoc=""""><content><![CDATA[]]></content>
        </Home>
    </Home>
    <Home id=""1172"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""3"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test"" writerName=""admin"" creatorName=""admin"" path=""-1,1172"" isDoc="""" />
</root>";
        }

        protected override void Initialize()
        {
            base.Initialize();

            _httpContextFactory = new FakeHttpContextFactory("~/Home");

            var umbracoSettings = Factory.GetInstance<IUmbracoSettingsSection>();
            var globalSettings = Factory.GetInstance<IGlobalSettings>();

            _xml = new XmlDocument();
            _xml.LoadXml(GetXml());
            var xmlStore = new XmlStore(() => _xml, null, null, null);
            var cacheProvider = new StaticCacheProvider();
            var domainCache = new DomainCache(ServiceContext.DomainService, DefaultCultureAccessor);
            var publishedShapshot = new Umbraco.Web.PublishedCache.XmlPublishedCache.PublishedSnapshot(
                new PublishedContentCache(xmlStore, domainCache, cacheProvider, globalSettings, new SiteDomainHelper(), ContentTypesCache, null, null),
                new PublishedMediaCache(xmlStore, ServiceContext.MediaService, ServiceContext.UserService, cacheProvider, ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>()),
                new PublishedMemberCache(null, cacheProvider, Current.Services.MemberService, ContentTypesCache),
                domainCache);
            var publishedSnapshotService = new Mock<IPublishedSnapshotService>();
            publishedSnapshotService.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>())).Returns(publishedShapshot);

            _umbracoContext = new UmbracoContext(
                _httpContextFactory.HttpContext,
                publishedSnapshotService.Object,
                new WebSecurity(_httpContextFactory.HttpContext, Current.Services.UserService, globalSettings),
                umbracoSettings,
                Enumerable.Empty<IUrlProvider>(),
                globalSettings,
                new TestVariationContextAccessor());

            _cache = _umbracoContext.ContentCache;
        }

        [Test]
        public void Has_Content()
        {
            Assert.IsTrue(_cache.HasContent());
        }


        [Test]
        public void Get_Root_Docs()
        {
            var result = _cache.GetAtRoot();
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(1046, result.ElementAt(0).Id);
            Assert.AreEqual(1172, result.ElementAt(1).Id);
        }


        [TestCase("/", 1046)]
        [TestCase("/home", 1046)]
        [TestCase("/Home", 1046)] //test different cases
        [TestCase("/home/sub1", 1173)]
        [TestCase("/Home/sub1", 1173)]
        [TestCase("/home/Sub1", 1173)] //test different cases
        [TestCase("/home/Sub'Apostrophe", 1177)]
        public void Get_Node_By_Route(string route, int nodeId)
        {
            var result = _cache.GetByRoute(route, false);
            Assert.IsNotNull(result);
            Assert.AreEqual(nodeId, result.Id);
        }



        [TestCase("/", 1046)]
        [TestCase("/sub1", 1173)]
        [TestCase("/Sub1", 1173)]
        public void Get_Node_By_Route_Hiding_Top_Level_Nodes(string route, int nodeId)
        {
            var result = _cache.GetByRoute(route, true);
            Assert.IsNotNull(result);
            Assert.AreEqual(nodeId, result.Id);
        }
    }
}
