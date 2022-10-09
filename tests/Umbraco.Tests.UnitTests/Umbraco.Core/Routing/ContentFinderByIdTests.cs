using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class ContentFinderByIdTests : PublishedSnapshotServiceTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();

        var xml = PublishedContentXml.BaseWebTestXml(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);
    }

    [TestCase("/1046", 1046)]
    public async Task Lookup_By_Id(string urlAsString, int nodeMatch)
    {
        var umbracoContextAccessor = GetUmbracoContextAccessor(urlAsString);
        var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
        var publishedRouter = CreatePublishedRouter(umbracoContextAccessor);
        var frequest = await publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);
        var webRoutingSettings = new WebRoutingSettings();
        var lookup = new ContentFinderByIdPath(
            Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == webRoutingSettings),
            Mock.Of<ILogger<ContentFinderByIdPath>>(),
            Mock.Of<IRequestAccessor>(),
            umbracoContextAccessor);

        var result = await lookup.TryFindContent(frequest);

        Assert.IsTrue(result);
        Assert.AreEqual(frequest.PublishedContent.Id, nodeMatch);
    }
}
