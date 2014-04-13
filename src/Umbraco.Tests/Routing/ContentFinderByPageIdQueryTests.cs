using Moq;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class ContentFinderByPageIdQueryTests : BaseRoutingTest
	{
		[TestCase("/?umbPageId=1046", 1046)]
		[TestCase("/?UMBPAGEID=1046", 1046)]
		[TestCase("/default.aspx?umbPageId=1046", 1046)] //TODO: Should this match??
		[TestCase("/some/other/page?umbPageId=1046", 1046)] //TODO: Should this match??
		[TestCase("/some/other/page.aspx?umbPageId=1046", 1046)] //TODO: Should this match??
		public void Lookup_By_Page_Id(string urlAsString, int nodeMatch)
		{		
			var routingContext = GetRoutingContext(urlAsString);
			var url = routingContext.UmbracoContext.CleanedUmbracoUrl; //very important to use the cleaned up umbraco url
			var docRequest = new PublishedContentRequest(url, routingContext);
			var lookup = new ContentFinderByPageIdQuery();			

			//we need to manually stub the return output of HttpContext.Request["umbPageId"]
		    var requestMock = Mock.Get(routingContext.UmbracoContext.HttpContext.Request);

            requestMock.Setup(x => x["umbPageID"])
				.Returns(routingContext.UmbracoContext.HttpContext.Request.QueryString["umbPageID"]);

			var result = lookup.TryFindContent(docRequest);

			Assert.IsTrue(result);
			Assert.AreEqual(docRequest.PublishedContent.Id, nodeMatch);
		}
	}
}