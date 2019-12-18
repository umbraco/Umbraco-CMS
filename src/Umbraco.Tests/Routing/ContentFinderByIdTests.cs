using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Tests.TestHelpers;
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
            var lookup = new ContentFinderByIdPath(Factory.GetInstance<IUmbracoSettingsSection>().WebRouting, Logger);


            var result = lookup.TryFindContent(frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(frequest.PublishedContent.Id, nodeMatch);
        }
    }
}
