using System.Linq;
using System.Xml;
using Moq;
using NUnit.Framework;
using umbraco.BusinessLogic;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Cache.PublishedCache
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
	[TestFixture]
	public class PublishContentCacheTests : BaseWebTest
	{
		private FakeHttpContextFactory _httpContextFactory;
		private UmbracoContext _umbracoContext;
		private ContextualPublishedContentCache _cache;
	    private XmlDocument _xml;

		private string GetLegacyXml()
		{
			return @"<?xml version=""1.0"" encoding=""utf-8""?><!DOCTYPE root[ 
<!ELEMENT node ANY> <!ATTLIST node id ID #REQUIRED>  <!ELEMENT data ANY>

]>
<root id=""-1"">
	<node id=""1046"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""2"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""home"" writerName=""admin"" creatorName=""admin"" path=""-1,1046"" nodeTypeAlias=""Home""  ><content><![CDATA[]]></content>
		<node id=""1173"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""1"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""sub1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173"" nodeTypeAlias=""Home""><content><![CDATA[]]></content>
			<node id=""1174"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""1"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""sub2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1174"" nodeTypeAlias=""Home"" ><content><![CDATA[]]></content>
			</node>
			<node id=""1176"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""2"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" nodeTypeAlias=""Home"" ><content><![CDATA[]]></content>
			</node>
		</node>
		<node id=""1175"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""2"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1175"" nodeTypeAlias=""Home"" ><content><![CDATA[]]></content>
		</node>
	</node>
	<node id=""1172"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""3"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test"" writerName=""admin"" creatorName=""admin"" path=""-1,1172"" nodeTypeAlias=""Home"" />
</root>";
		}

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

		[SetUp]
        public override void Initialize()
        {
            base.Initialize();

            _httpContextFactory = new FakeHttpContextFactory("~/Home");
            //ensure the StateHelper is using our custom context
            StateHelper.HttpContext = _httpContextFactory.HttpContext;

		    var settings = SettingsForTests.GenerateMockSettings();
		    var contentMock = Mock.Get(settings.Content);
            contentMock.Setup(x => x.UseLegacyXmlSchema).Returns(false);
		    SettingsForTests.ConfigureSettings(settings);
            _xml = new XmlDocument();
            _xml.LoadXml(GetXml());
            var cache = new PublishedContentCache
                {
                    GetXmlDelegate = (context, preview) => _xml
                };

		    _umbracoContext = new UmbracoContext(
                _httpContextFactory.HttpContext,
                ApplicationContext,
                new PublishedCaches(cache, new PublishedMediaCache(ApplicationContext)),
                new WebSecurity(_httpContextFactory.HttpContext, ApplicationContext));

		    _cache = _umbracoContext.ContentCache;
        }

	    private void SetupForLegacy()
		{
            var settings = SettingsForTests.GenerateMockSettings();
		    var contentMock = Mock.Get(settings.Content);
            contentMock.Setup(x => x.UseLegacyXmlSchema).Returns(true);
            SettingsForTests.ConfigureSettings(settings);
            _xml = new XmlDocument();
            _xml.LoadXml(GetLegacyXml());
		}

	    protected override void FreezeResolution()
	    {
            PublishedContentModelFactoryResolver.Current = new PublishedContentModelFactoryResolver();
	        base.FreezeResolution();
	    }
		
	    [Test]
		public void Has_Content_LegacySchema()
		{
			SetupForLegacy();
			Has_Content();
		}

		[Test]
		public void Has_Content()
		{
			Assert.IsTrue(_cache.HasContent());
		}

		[Test]
		public void Get_Root_Docs_LegacySchema()
		{
			SetupForLegacy();
			Get_Root_Docs();
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
		public void Get_Node_By_Route_LegacySchema(string route, int nodeId)
		{
			SetupForLegacy();
			Get_Node_By_Route(route, nodeId);
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
		public void Get_Node_By_Route_Hiding_Top_Level_Nodes_LegacySchema(string route, int nodeId)
		{
			SetupForLegacy();
			Get_Node_By_Route_Hiding_Top_Level_Nodes(route, nodeId);
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