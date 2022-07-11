using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PublishedCache;

[TestFixture]
public class RootNodeTests : PublishedSnapshotServiceTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();

        var xml = PublishedContentXml.TestWithDatabaseXml(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);
    }

    [Test]
    public void PublishedContentHasNoRootNode()
    {
        var snapshot = GetPublishedSnapshot();

        // there is no content node with ID -1
        var content = snapshot.Content.GetById(-1);
        Assert.IsNull(content);

        // content at root has null parent
        content = snapshot.Content.GetById(1046);
        Assert.IsNotNull(content);
        Assert.AreEqual(1, content.Level);
        Assert.IsNull(content.Parent);

        // non-existing content is null
        content = snapshot.Content.GetById(666);
        Assert.IsNull(content);
    }
}
