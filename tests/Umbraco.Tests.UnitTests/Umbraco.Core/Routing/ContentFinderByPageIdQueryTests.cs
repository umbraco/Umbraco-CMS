using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class ContentFinderByPageIdQueryTests : PublishedSnapshotServiceTestBase
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

    [TestCase("/?umbPageId=1046", 1046)]
    [TestCase("/?UMBPAGEID=1046", 1046)]
    [TestCase("/default.aspx?umbPageId=1046", 1046)] // TODO: Should this match??
    [TestCase("/some/other/page?umbPageId=1046", 1046)] // TODO: Should this match??
    [TestCase("/some/other/page.aspx?umbPageId=1046", 1046)] // TODO: Should this match??
    public async Task Lookup_By_Page_Id(string urlAsString, int nodeMatch)
    {
        var umbracoContextAccessor = GetUmbracoContextAccessor(urlAsString);
        var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
        var publishedRouter = CreatePublishedRouter(umbracoContextAccessor);
        var frequest = await publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);

        var queryStrings = HttpUtility.ParseQueryString(umbracoContext.CleanedUmbracoUrl.Query);

        var mockRequestAccessor = new Mock<IRequestAccessor>();
        mockRequestAccessor.Setup(x => x.GetRequestValue("umbPageID"))
            .Returns(queryStrings["umbPageID"]);

        var lookup = new ContentFinderByPageIdQuery(mockRequestAccessor.Object, umbracoContextAccessor);

        var result = await lookup.TryFindContent(frequest);

        Assert.IsTrue(result);
        Assert.AreEqual(frequest.PublishedContent.Id, nodeMatch);
    }
}
