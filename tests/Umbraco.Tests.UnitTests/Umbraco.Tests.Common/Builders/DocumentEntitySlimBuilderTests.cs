// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class DocumentEntitySlimBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int testId = 11;
        const string testName = "Test";
        const int testCreatorId = 22;
        const int testLevel = 3;
        const string testPath = "-1,23";
        const int testParentId = 5;
        const int testSortOrder = 6;
        const bool testHasChildren = true;
        const bool testPublished = true;
        const string testContentTypeAlias = "test1";
        const string testContentTypeIcon = "icon";
        const string testContentTypeThumbnail = "thumb";
        var testKey = Guid.NewGuid();
        var testCreateDate = DateTime.UtcNow.AddHours(-1);
        var testUpdateDate = DateTime.UtcNow;

        var builder = new DocumentEntitySlimBuilder();

        // Act
        var item = builder
            .WithId(testId)
            .WithKey(testKey)
            .WithCreatorId(testCreatorId)
            .WithCreateDate(testCreateDate)
            .WithUpdateDate(testUpdateDate)
            .WithName(testName)
            .WithParentId(testParentId)
            .WithSortOrder(testSortOrder)
            .WithLevel(testLevel)
            .WithPath(testPath)
            .WithContentTypeAlias(testContentTypeAlias)
            .WithContentTypeIcon(testContentTypeIcon)
            .WithContentTypeThumbnail(testContentTypeThumbnail)
            .WithHasChildren(testHasChildren)
            .WithPublished(testPublished)
            .Build();

        // Assert
        Assert.That(item.Id, Is.EqualTo(testId));
        Assert.That(item.Key, Is.EqualTo(testKey));
        Assert.That(item.Name, Is.EqualTo(testName));
        Assert.That(item.CreateDate, Is.EqualTo(testCreateDate));
        Assert.That(item.UpdateDate, Is.EqualTo(testUpdateDate));
        Assert.That(item.CreatorId, Is.EqualTo(testCreatorId));
        Assert.That(item.ParentId, Is.EqualTo(testParentId));
        Assert.That(item.SortOrder, Is.EqualTo(testSortOrder));
        Assert.That(item.Path, Is.EqualTo(testPath));
        Assert.That(item.Level, Is.EqualTo(testLevel));
        Assert.That(item.ContentTypeAlias, Is.EqualTo(testContentTypeAlias));
        Assert.That(item.ContentTypeIcon, Is.EqualTo(testContentTypeIcon));
        Assert.That(item.ContentTypeThumbnail, Is.EqualTo(testContentTypeThumbnail));
        Assert.That(item.HasChildren, Is.EqualTo(testHasChildren));
        Assert.That(item.Published, Is.EqualTo(testPublished));
    }
}
