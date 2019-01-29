using Moq;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    public class ContentFinderByPageIdQueryTests : BaseWebTest
    {
        [TestCase("/?umbPageId=1046", 1046)]
        [TestCase("/?UMBPAGEID=1046", 1046)]
        [TestCase("/default.aspx?umbPageId=1046", 1046)] // TODO: Should this match??
        [TestCase("/some/other/page?umbPageId=1046", 1046)] // TODO: Should this match??
        [TestCase("/some/other/page.aspx?umbPageId=1046", 1046)] // TODO: Should this match??
        public void Lookup_By_Page_Id(string urlAsString, int nodeMatch)
        {
            var umbracoContext = GetUmbracoContext(urlAsString);
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            var lookup = new ContentFinderByPageIdQuery();

            //we need to manually stub the return output of HttpContext.Request["umbPageId"]
            var requestMock = Mock.Get(umbracoContext.HttpContext.Request);

            requestMock.Setup(x => x["umbPageID"])
                .Returns(umbracoContext.HttpContext.Request.QueryString["umbPageID"]);

            var result = lookup.TryFindContent(frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(frequest.PublishedContent.Id, nodeMatch);
        }
    }
}
