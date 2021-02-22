using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Routing;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    public class ContentFinderByIdTests : BaseWebTest
    {
        [TestCase("/1046", 1046)]
        [TestCase("/1046.aspx", 1046)]
        public async Task Lookup_By_Id(string urlAsString, int nodeMatch)
        {
            var umbracoContext = GetUmbracoContext(urlAsString);
            var publishedRouter = CreatePublishedRouter(GetUmbracoContextAccessor(umbracoContext));
            var frequest = await publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);
            var webRoutingSettings = new WebRoutingSettings();
            var lookup = new ContentFinderByIdPath(Microsoft.Extensions.Options.Options.Create(webRoutingSettings), LoggerFactory.CreateLogger<ContentFinderByIdPath>(), Factory.GetRequiredService<IRequestAccessor>(), GetUmbracoContextAccessor(umbracoContext));


            var result = lookup.TryFindContent(frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(frequest.PublishedContent.Id, nodeMatch);
        }
    }
}
