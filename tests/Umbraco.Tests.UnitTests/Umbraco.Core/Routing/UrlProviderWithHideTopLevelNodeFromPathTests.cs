using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class UrlProviderWithHideTopLevelNodeFromPathTests : PublishedSnapshotServiceTestBase
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

        GlobalSettings.HideTopLevelNodeFromPath = true;
    }

    [TestCase(1046, "/")]
    [TestCase(1173, "/sub1/")]
    [TestCase(1174, "/sub1/sub2/")]
    [TestCase(1176, "/sub1/sub-3/")]
    [TestCase(1177, "/sub1/custom-sub-1/")]
    [TestCase(1178, "/sub1/custom-sub-2/")]
    [TestCase(1175, "/sub-2/")]
    [TestCase(1172, "/test-page/")] // not hidden because not first root
    public void Get_Url_Hiding_Top_Level(int nodeId, string niceUrlMatch)
    {
        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };

        var umbracoContextAccessor = GetUmbracoContextAccessor("/test");

        var urlProvider = GetUrlProvider(umbracoContextAccessor, requestHandlerSettings, new WebRoutingSettings(), out var uriUtility);

        var result = urlProvider.GetUrl(nodeId);
        Assert.AreEqual(niceUrlMatch, result);
    }
}
