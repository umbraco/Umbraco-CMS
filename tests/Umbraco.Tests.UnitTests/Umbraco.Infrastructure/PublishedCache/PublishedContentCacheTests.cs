using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PublishedCache;

[TestFixture]
public class PublishContentCacheTests : PublishedSnapshotServiceTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();

        var xml = PublishedContentXml.PublishContentCacheTestsXml();

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        // configure the Home content type to be composed of another for tests.
        var compositionType = new ContentType(TestHelper.ShortStringHelper, -1) { Alias = "MyCompositionAlias" };
        contentTypes.First(x => x.Alias == "Home").AddContentType(compositionType);

        InitializedCache(kits, contentTypes, dataTypes);

        _cache = GetPublishedSnapshot().Content;
    }

    private IPublishedContentCache _cache;

    [Test]
    public void Has_Content() => Assert.IsTrue(_cache.HasContent());

    [Test]
    public void Get_Root_Docs()
    {
        var result = _cache.GetAtRoot().ToArray();
        Assert.AreEqual(2, result.Length);
        Assert.AreEqual(1046, result.ElementAt(0).Id);
        Assert.AreEqual(1172, result.ElementAt(1).Id);
    }

    [TestCase("/", 1046)]
    [TestCase("/home", 1046)]
    [TestCase("/Home", 1046)] // test different cases
    [TestCase("/home/sub1", 1173)]
    [TestCase("/Home/sub1", 1173)]
    [TestCase("/home/Sub1", 1173)] // test different cases
    [TestCase("/home/Sub'Apostrophe", 1177)]
    public void Get_Node_By_Route(string route, int nodeId)
    {
        var result = _cache.GetByRoute(route, false);
        Assert.IsNotNull(result);
        Assert.AreEqual(nodeId, result.Id);
    }

    [TestCase("/", 1046)]
    [TestCase("/sub1", 1173)]
    [TestCase("/Sub1", 1173)]
    public void Get_Node_By_Route_Hiding_Top_Level_Nodes(string route, int nodeId)
    {
        var result = _cache.GetByRoute(route, true);
        Assert.IsNotNull(result);
        Assert.AreEqual(nodeId, result.Id);
    }
}
