using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class ContentFinderByNiceUrlTests : BaseWebTest
    {
		[TestCase("/", 1046)]
		[TestCase("/default.aspx", 1046)] //this one is actually rather important since this is the path that comes through when we are running in pre-IIS 7 for the root document '/' !
		[TestCase("/Sub1", 1173)]
		[TestCase("/sub1", 1173)]
		[TestCase("/sub1.aspx", 1173)]
		[TestCase("/home/sub1", -1)] // should fail

		// these two are special. getNiceUrl(1046) returns "/" but getNiceUrl(1172) cannot also return "/" so
		// we've made it return "/test-page" => we have to support that url back in the lookup...
		[TestCase("/home", 1046)]
		[TestCase("/test-page", 1172)]
		public void Match_Document_By_Url_Hide_Top_Level(string urlString, int expectedId)
		{
			var umbracoContext = GetUmbracoContext(urlString);
		    var facadeRouter = CreateFacadeRouter();
			var frequest = facadeRouter.CreateRequest(umbracoContext);
            var lookup = new ContentFinderByNiceUrl(Logger);
		    SettingsForTests.HideTopLevelNodeFromPath = true;

            Assert.IsTrue(Core.Configuration.GlobalSettings.HideTopLevelNodeFromPath);

            // fixme debugging - going further down, the routes cache is NOT empty?!
		    if (urlString == "/home/sub1")
		        System.Diagnostics.Debugger.Break();

			var result = lookup.TryFindContent(frequest);

			if (expectedId > 0)
			{
				Assert.IsTrue(result);
				Assert.AreEqual(expectedId, frequest.PublishedContent.Id);
			}
			else
			{
				Assert.IsFalse(result);
			}
		}

		[TestCase("/", 1046)]
		[TestCase("/default.aspx", 1046)] //this one is actually rather important since this is the path that comes through when we are running in pre-IIS 7 for the root document '/' !
		[TestCase("/home", 1046)]
		[TestCase("/home/Sub1", 1173)]
		[TestCase("/Home/Sub1", 1173)] //different cases
		[TestCase("/home/Sub1.aspx", 1173)]
		public void Match_Document_By_Url(string urlString, int expectedId)
		{
			var umbracoContext = GetUmbracoContext(urlString);
		    var facadeRouter = CreateFacadeRouter();
			var frequest = facadeRouter.CreateRequest(umbracoContext);
            var lookup = new ContentFinderByNiceUrl(Logger);
            SettingsForTests.HideTopLevelNodeFromPath = false;

            Assert.IsFalse(Core.Configuration.GlobalSettings.HideTopLevelNodeFromPath);

            var result = lookup.TryFindContent(frequest);

			Assert.IsTrue(result);
			Assert.AreEqual(expectedId, frequest.PublishedContent.Id);
		}
	}
}