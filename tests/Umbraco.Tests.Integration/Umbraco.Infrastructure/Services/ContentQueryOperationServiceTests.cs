using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
/// Integration tests for ContentQueryOperationService.
/// </summary>
[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    WithApplication = true)]
public class ContentQueryOperationServiceTests : UmbracoIntegrationTestWithContent
{
    private IContentQueryOperationService QueryService => GetRequiredService<IContentQueryOperationService>();

    [Test]
    public void Count_WithNoFilter_ReturnsAllContentCount()
    {
        // Arrange - base class creates Textpage, Subpage, Subpage2, Subpage3, Trashed

        // Act
        var count = QueryService.Count();

        // Assert - should return 5 (all items including Trashed)
        Assert.That(count, Is.EqualTo(5));
    }

    [Test]
    public void Count_WithNonExistentContentTypeAlias_ReturnsZero()
    {
        // Arrange
        var nonExistentAlias = "nonexistent-content-type-alias";

        // Act
        var count = QueryService.Count(nonExistentAlias);

        // Assert
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void Count_WithContentTypeAlias_ReturnsFilteredCount()
    {
        // Arrange
        var alias = ContentType.Alias;

        // Act
        var count = QueryService.Count(alias);

        // Assert - all 5 content items use the same content type
        Assert.That(count, Is.EqualTo(5));
    }

    [Test]
    public void CountChildren_ReturnsChildCount()
    {
        // Arrange - Textpage has children: Subpage, Subpage2, Subpage3
        var parentId = Textpage.Id;

        // Act
        var count = QueryService.CountChildren(parentId);

        // Assert
        Assert.That(count, Is.EqualTo(3));
    }

    [Test]
    public void GetByLevel_ReturnsContentAtLevel()
    {
        // Arrange - level 1 is root content

        // Act
        var items = QueryService.GetByLevel(1);

        // Assert
        Assert.That(items, Is.Not.Null);
        Assert.That(items.All(x => x.Level == 1), Is.True);
    }

    [Test]
    public void GetPagedOfType_ReturnsPaginatedResults()
    {
        // Arrange
        var contentTypeId = ContentType.Id;

        // Act
        var items = QueryService.GetPagedOfType(contentTypeId, 0, 10, out var total);

        // Assert
        Assert.That(items, Is.Not.Null);
        Assert.That(total, Is.EqualTo(5)); // All 5 content items are of this type
    }

    [Test]
    public void GetPagedOfTypes_WithEmptyArray_ReturnsEmpty()
    {
        // Act
        var items = QueryService.GetPagedOfTypes(Array.Empty<int>(), 0, 10, out var total);

        // Assert
        Assert.That(items, Is.Empty);
        Assert.That(total, Is.EqualTo(0));
    }

    [Test]
    public void GetPagedOfTypes_WithNonExistentContentTypeIds_ReturnsEmpty()
    {
        // Arrange
        var nonExistentIds = new[] { 999999, 999998 };

        // Act
        var items = QueryService.GetPagedOfTypes(nonExistentIds, 0, 10, out var total);

        // Assert
        Assert.That(items, Is.Empty);
        Assert.That(total, Is.EqualTo(0));
    }

    [Test]
    public void CountChildren_WithNonExistentParentId_ReturnsZero()
    {
        // Arrange
        var nonExistentParentId = 999999;

        // Act
        var count = QueryService.CountChildren(nonExistentParentId);

        // Assert
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void GetByLevel_WithLevelZero_ReturnsEmpty()
    {
        // Arrange - level 0 doesn't exist (content starts at level 1)

        // Act
        var items = QueryService.GetByLevel(0);

        // Assert
        Assert.That(items, Is.Empty);
    }

    [Test]
    public void GetByLevel_WithNegativeLevel_ReturnsEmpty()
    {
        // Arrange

        // Act
        var items = QueryService.GetByLevel(-1);

        // Assert
        Assert.That(items, Is.Empty);
    }

    [Test]
    public void GetPagedOfType_WithNonExistentContentTypeId_ReturnsEmpty()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var items = QueryService.GetPagedOfType(nonExistentId, 0, 10, out var total);

        // Assert
        Assert.That(items, Is.Empty);
        Assert.That(total, Is.EqualTo(0));
    }

    [Test]
    public void CountDescendants_ReturnsDescendantCount()
    {
        // Arrange - Textpage has descendants: Subpage, Subpage2, Subpage3
        var ancestorId = Textpage.Id;

        // Act
        var count = QueryService.CountDescendants(ancestorId);

        // Assert
        Assert.That(count, Is.EqualTo(3));
    }

    [Test]
    public void CountDescendants_WithNonExistentAncestorId_ReturnsZero()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var count = QueryService.CountDescendants(nonExistentId);

        // Assert
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void CountPublished_WithNoPublishedContent_ReturnsZero()
    {
        // Arrange - base class creates content but doesn't publish

        // Act
        var count = QueryService.CountPublished();

        // Assert
        Assert.That(count, Is.EqualTo(0));
    }
}
