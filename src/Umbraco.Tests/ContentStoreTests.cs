using System.Xml;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;

namespace Umbraco.Tests
{
	[TestFixture]
	public class ContentStoreTests
	{
		private FakeHttpContextFactory _httpContextFactory;
		private UmbracoContext _umbracoContext;
		private ContentStore _contentStore;

		[SetUp]
		public void SetUp()
		{
			TestHelper.SetupLog4NetForTests();

			_httpContextFactory = new FakeHttpContextFactory("~/Home");
			//ensure the StateHelper is using our custom context
			StateHelper.HttpContext = _httpContextFactory.HttpContext;
			
			_umbracoContext = new UmbracoContext(_httpContextFactory.HttpContext, 
				new ApplicationContext(), 
				new DefaultRoutesCache(false));

			_umbracoContext.GetXmlDelegate = () =>
				{
					var xDoc = new XmlDocument();

					//create a custom xml structure to return

					xDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?><!DOCTYPE root[ 
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
	</Home>
	<Home id=""1172"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1045"" sortOrder=""3"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test"" writerName=""admin"" creatorName=""admin"" path=""-1,1172"" isDoc="""" />
</root>");
					//return the custom x doc
					return xDoc;
				};

			_contentStore = new ContentStore(_umbracoContext);
			
		}

		[TearDown]
		public void TearDown()
		{
			
		}

		[TestCase("/", 1046)]
		[TestCase("/home", 1046)]
		[TestCase("/Home", 1046)] //test different cases
		[TestCase("/home/sub1", 1173)]
		[TestCase("/Home/sub1", 1173)]
		[TestCase("/home/Sub1", 1173)] //test different cases
		public void Get_Node_By_Route(string route, int nodeId)
		{
			var result = _contentStore.GetDocumentByRoute(route, false);
			Assert.IsNotNull(result);
			Assert.AreEqual(nodeId, result.Id);
		}

		[TestCase("/", 1046)]		
		[TestCase("/sub1", 1173)]
		[TestCase("/Sub1", 1173)]
		public void Get_Node_By_Route_Hiding_Top_Level_Nodes(string route, int nodeId)
		{
			var result = _contentStore.GetDocumentByRoute(route, true);
			Assert.IsNotNull(result);
			Assert.AreEqual(nodeId, result.Id);
		}
	}
}