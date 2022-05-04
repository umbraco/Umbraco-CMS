using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing
{
    // TODO: We should be able to decouple this from the base db tests since we're just mocking the services now

    [TestFixture]
    public class ContentFinderByAliasTests : UrlRoutingTestBase
    {

        [TestCase("/this/is/my/alias", 1001)]
        [TestCase("/anotheralias", 1001)]
        [TestCase("/page2/alias", 10011)]
        [TestCase("/2ndpagealias", 10011)]
        [TestCase("/only/one/alias", 100111)]
        [TestCase("/ONLY/one/Alias", 100111)]
        [TestCase("/alias43", 100121)]
        public async Task Lookup_By_Url_Alias(string urlAsString, int nodeMatch)
        {
            var umbracoContextAccessor = GetUmbracoContextAccessor(urlAsString);
            var publishedRouter = CreatePublishedRouter(umbracoContextAccessor);
            var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();

            var frequest = await publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);
            var lookup =
                new ContentFinderByUrlAlias(Mock.Of<ILogger<ContentFinderByUrlAlias>>(), Mock.Of<IPublishedValueFallback>(), VariationContextAccessor, umbracoContextAccessor);

            var result = await lookup.TryFindContent(frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(frequest.PublishedContent.Id, nodeMatch);
        }
    }
}
