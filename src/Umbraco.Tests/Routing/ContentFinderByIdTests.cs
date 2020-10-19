using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    public class ContentFinderByIdTests : BaseWebTest
    {

        [TestCase("/1046", 1046)]
        [TestCase("/1046.aspx", 1046)]
        public void Lookup_By_Id(string urlAsString, int nodeMatch)
        {
            var umbracoContext = GetUmbracoContext(urlAsString);
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            var webRoutingSettings = new WebRoutingSettings();
            var lookup = new ContentFinderByIdPath(Microsoft.Extensions.Options.Options.Create(webRoutingSettings), LoggerFactory.CreateLogger<ContentFinderByIdPath>(), Factory.GetInstance<IRequestAccessor>());


            var result = lookup.TryFindContent(frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(frequest.PublishedContent.Id, nodeMatch);
        }
    }
}
