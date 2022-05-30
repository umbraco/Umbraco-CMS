using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PublishedCache;

[TestFixture]
public class PublishedContentExtensionTests : PublishedSnapshotServiceTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            XmlContent,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        // configure inheritance for content types
        var baseType = new ContentType(TestHelper.ShortStringHelper, -1) { Alias = "Base" };
        contentTypes[0].AddContentType(baseType);

        InitializedCache(kits, contentTypes, dataTypes);
    }

    private const string XmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[
<!ELEMENT inherited ANY>
<!ATTLIST inherited id ID #REQUIRED>
]>
<root id=""-1"">
    <inherited id=""1100"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""1"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""home"" writerName=""admin"" creatorName=""admin"" path=""-1,1046"" isDoc=""""/>
</root>";

    [Test]
    public void IsDocumentType_NonRecursive_ActualType_ReturnsTrue()
    {
        var publishedContent = GetContent(1100);
        Assert.That(publishedContent.IsDocumentType("Inherited", false));
    }

    [Test]
    public void IsDocumentType_NonRecursive_BaseType_ReturnsFalse()
    {
        var publishedContent = GetContent(1100);
        Assert.That(publishedContent.IsDocumentType("Base", false), Is.False);
    }

    [Test]
    public void IsDocumentType_Recursive_ActualType_ReturnsTrue()
    {
        var publishedContent = GetContent(1100);
        Assert.That(publishedContent.IsDocumentType("Inherited", true));
    }

    [Test]
    public void IsDocumentType_Recursive_BaseType_ReturnsTrue()
    {
        var publishedContent = GetContent(1100);
        Assert.That(publishedContent.IsDocumentType("Base", true));
    }

    [Test]
    public void IsDocumentType_Recursive_InvalidBaseType_ReturnsFalse()
    {
        var publishedContent = GetContent(1100);
        Assert.That(publishedContent.IsDocumentType("invalidbase", true), Is.False);
    }
}
