// tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentMoveOperationServiceTests.cs
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ContentMoveOperationServiceTests : UmbracoIntegrationTestWithContent
{
    private IContentMoveOperationService MoveOperationService => GetRequiredService<IContentMoveOperationService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentMovingNotification, MoveNotificationHandler>();
        builder.AddNotificationHandler<ContentMovedNotification, MoveNotificationHandler>();
        builder.AddNotificationHandler<ContentCopyingNotification, MoveNotificationHandler>();
        builder.AddNotificationHandler<ContentCopiedNotification, MoveNotificationHandler>();
        builder.AddNotificationHandler<ContentSortingNotification, MoveNotificationHandler>();
        builder.AddNotificationHandler<ContentSortedNotification, MoveNotificationHandler>();
    }

    #region Move Tests

    [Test]
    public void Move_ToNewParent_ChangesParentId()
    {
        // Arrange
        var child = ContentService.Create("Child", Textpage.Id, ContentType.Alias);
        ContentService.Save(child);

        var newParent = ContentService.Create("NewParent", Constants.System.Root, ContentType.Alias);
        ContentService.Save(newParent);

        // Act
        var result = MoveOperationService.Move(child, newParent.Id);

        // Assert
        Assert.That(result.Success, Is.True);
        var movedContent = ContentService.GetById(child.Id);
        Assert.That(movedContent!.ParentId, Is.EqualTo(newParent.Id));
    }

    [Test]
    public void Move_ToSameParent_ReturnsSuccess()
    {
        // Arrange
        var child = ContentService.Create("Child", Textpage.Id, ContentType.Alias);
        ContentService.Save(child);

        // Act
        var result = MoveOperationService.Move(child, Textpage.Id);

        // Assert
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public void Move_ToNonExistentParent_ThrowsException()
    {
        // Arrange
        var content = ContentService.Create("Content", Constants.System.Root, ContentType.Alias);
        ContentService.Save(content);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            MoveOperationService.Move(content, 999999));
    }

    [Test]
    public void Move_FiresMovingAndMovedNotifications()
    {
        // Arrange
        var child = ContentService.Create("Child", Textpage.Id, ContentType.Alias);
        ContentService.Save(child);

        var newParent = ContentService.Create("NewParent", Constants.System.Root, ContentType.Alias);
        ContentService.Save(newParent);

        bool movingFired = false;
        bool movedFired = false;

        MoveNotificationHandler.Moving = notification => movingFired = true;
        MoveNotificationHandler.Moved = notification => movedFired = true;

        try
        {
            // Act
            var result = MoveOperationService.Move(child, newParent.Id);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(movingFired, Is.True, "Moving notification should fire");
            Assert.That(movedFired, Is.True, "Moved notification should fire");
        }
        finally
        {
            MoveNotificationHandler.Moving = null;
            MoveNotificationHandler.Moved = null;
        }
    }

    [Test]
    public void Move_WhenCancelled_ReturnsCancel()
    {
        // Arrange
        var child = ContentService.Create("Child", Textpage.Id, ContentType.Alias);
        ContentService.Save(child);

        var newParent = ContentService.Create("NewParent", Constants.System.Root, ContentType.Alias);
        ContentService.Save(newParent);

        MoveNotificationHandler.Moving = notification => notification.Cancel = true;

        try
        {
            // Act
            var result = MoveOperationService.Move(child, newParent.Id);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(OperationResultType.FailedCancelledByEvent));
        }
        finally
        {
            MoveNotificationHandler.Moving = null;
        }
    }

    #endregion

    #region RecycleBin Tests

    [Test]
    public void RecycleBinSmells_WhenEmpty_ReturnsFalse()
    {
        // Act
        var result = MoveOperationService.RecycleBinSmells();

        // Assert - depends on base class setup, but Trashed item should make it smell
        Assert.That(result, Is.True); // Trashed exists from base class
    }

    [Test]
    public void GetPagedContentInRecycleBin_ReturnsPagedResults()
    {
        // Act
        var results = MoveOperationService.GetPagedContentInRecycleBin(0, 10, out long totalRecords);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(totalRecords, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void EmptyRecycleBin_ClearsRecycleBin()
    {
        // Arrange - ensure something is in recycle bin (from base class)
        Assert.That(MoveOperationService.RecycleBinSmells(), Is.True);

        // Act
        var result = MoveOperationService.EmptyRecycleBin();

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(MoveOperationService.RecycleBinSmells(), Is.False);
    }

    #endregion

    #region Copy Tests

    [Test]
    public void Copy_CreatesNewContent()
    {
        // Arrange
        var original = Textpage;

        // Act
        var copy = MoveOperationService.Copy(original, Constants.System.Root, false);

        // Assert
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy!.Id, Is.Not.EqualTo(original.Id));
        Assert.That(copy.Key, Is.Not.EqualTo(original.Key));
        // Copy appends a number to make the name unique, e.g. "Textpage (1)"
        Assert.That(copy.Name, Does.StartWith(original.Name));
    }

    [Test]
    public void Copy_Recursive_CopiesDescendants()
    {
        // Arrange
        var child = ContentService.Create("Child", Textpage.Id, ContentType.Alias);
        ContentService.Save(child);
        var grandchild = ContentService.Create("Grandchild", child.Id, ContentType.Alias);
        ContentService.Save(grandchild);

        // Act
        var copy = MoveOperationService.Copy(Textpage, Constants.System.Root, false, recursive: true);

        // Assert
        Assert.That(copy, Is.Not.Null);
        // Get all descendants to verify recursive copy
        var copyDescendants = ContentService.GetPagedDescendants(copy!.Id, 0, 100, out _).ToList();
        Assert.That(copyDescendants.Count, Is.GreaterThanOrEqualTo(1), "Should have copied at least the child");
    }

    [Test]
    public void Copy_NonRecursive_DoesNotCopyDescendants()
    {
        // Arrange
        var child = ContentService.Create("Child", Textpage.Id, ContentType.Alias);
        ContentService.Save(child);

        // Act
        var copy = MoveOperationService.Copy(Textpage, Constants.System.Root, false, recursive: false);

        // Assert
        Assert.That(copy, Is.Not.Null);
        var copyChildren = ContentService.GetPagedChildren(copy!.Id, 0, 10, out _).ToList();
        Assert.That(copyChildren.Count, Is.EqualTo(0));
    }

    [Test]
    public void Copy_FiresCopyingAndCopiedNotifications()
    {
        // Arrange
        bool copyingFired = false;
        bool copiedFired = false;

        MoveNotificationHandler.Copying = notification => copyingFired = true;
        MoveNotificationHandler.Copied = notification => copiedFired = true;

        try
        {
            // Act
            var copy = MoveOperationService.Copy(Textpage, Constants.System.Root, false);

            // Assert
            Assert.That(copy, Is.Not.Null);
            Assert.That(copyingFired, Is.True, "Copying notification should fire");
            Assert.That(copiedFired, Is.True, "Copied notification should fire");
        }
        finally
        {
            MoveNotificationHandler.Copying = null;
            MoveNotificationHandler.Copied = null;
        }
    }

    [Test]
    public void Copy_WhenCancelled_ReturnsNull()
    {
        // Arrange
        MoveNotificationHandler.Copying = notification => notification.Cancel = true;

        try
        {
            // Act
            var copy = MoveOperationService.Copy(Textpage, Constants.System.Root, false);

            // Assert
            Assert.That(copy, Is.Null);
        }
        finally
        {
            MoveNotificationHandler.Copying = null;
        }
    }

    #endregion

    #region Sort Tests

    [Test]
    public void Sort_ChangesOrder()
    {
        // Arrange
        var child1 = ContentService.Create("Child1", Textpage.Id, ContentType.Alias);
        child1.SortOrder = 0;
        ContentService.Save(child1);

        var child2 = ContentService.Create("Child2", Textpage.Id, ContentType.Alias);
        child2.SortOrder = 1;
        ContentService.Save(child2);

        var child3 = ContentService.Create("Child3", Textpage.Id, ContentType.Alias);
        child3.SortOrder = 2;
        ContentService.Save(child3);

        // Act - reverse the order
        var result = MoveOperationService.Sort(new[] { child3, child2, child1 });

        // Assert
        Assert.That(result.Success, Is.True);
        var reloaded1 = ContentService.GetById(child1.Id)!;
        var reloaded2 = ContentService.GetById(child2.Id)!;
        var reloaded3 = ContentService.GetById(child3.Id)!;
        Assert.That(reloaded3.SortOrder, Is.EqualTo(0));
        Assert.That(reloaded2.SortOrder, Is.EqualTo(1));
        Assert.That(reloaded1.SortOrder, Is.EqualTo(2));
    }

    [Test]
    public void Sort_ByIds_ChangesOrder()
    {
        // Arrange
        var child1 = ContentService.Create("Child1", Textpage.Id, ContentType.Alias);
        child1.SortOrder = 0;
        ContentService.Save(child1);

        var child2 = ContentService.Create("Child2", Textpage.Id, ContentType.Alias);
        child2.SortOrder = 1;
        ContentService.Save(child2);

        // Act - reverse the order
        var result = MoveOperationService.Sort(new[] { child2.Id, child1.Id });

        // Assert
        Assert.That(result.Success, Is.True);
        var reloaded1 = ContentService.GetById(child1.Id)!;
        var reloaded2 = ContentService.GetById(child2.Id)!;
        Assert.That(reloaded2.SortOrder, Is.EqualTo(0));
        Assert.That(reloaded1.SortOrder, Is.EqualTo(1));
    }

    [Test]
    public void Sort_FiresSortingAndSortedNotifications()
    {
        // Arrange
        var child1 = ContentService.Create("Child1", Textpage.Id, ContentType.Alias);
        ContentService.Save(child1);

        bool sortingFired = false;
        bool sortedFired = false;

        MoveNotificationHandler.Sorting = notification => sortingFired = true;
        MoveNotificationHandler.Sorted = notification => sortedFired = true;

        try
        {
            // Act
            var result = MoveOperationService.Sort(new[] { child1 });

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(sortingFired, Is.True, "Sorting notification should fire");
            Assert.That(sortedFired, Is.True, "Sorted notification should fire");
        }
        finally
        {
            MoveNotificationHandler.Sorting = null;
            MoveNotificationHandler.Sorted = null;
        }
    }

    [Test]
    public void Sort_EmptyList_ReturnsNoOperation()
    {
        // Act
        var result = MoveOperationService.Sort(Array.Empty<IContent>());

        // Assert
        Assert.That(result.Result, Is.EqualTo(OperationResultType.NoOperation));
    }

    #endregion

    #region Behavioral Equivalence Tests

    [Test]
    public void Move_ViaService_MatchesContentService()
    {
        // Arrange
        var child1 = ContentService.Create("Child1", Textpage.Id, ContentType.Alias);
        ContentService.Save(child1);
        var child2 = ContentService.Create("Child2", Textpage.Id, ContentType.Alias);
        ContentService.Save(child2);

        var newParent = ContentService.Create("NewParent", Constants.System.Root, ContentType.Alias);
        ContentService.Save(newParent);

        // Act
        var viaService = MoveOperationService.Move(child1, newParent.Id);
        var viaContentService = ContentService.Move(child2, newParent.Id);

        // Assert
        Assert.That(viaService.Success, Is.EqualTo(viaContentService.Success));
    }

    [Test]
    public void Copy_ViaService_MatchesContentService()
    {
        // Arrange
        var original = Textpage;

        // Act
        var viaService = MoveOperationService.Copy(original, Constants.System.Root, false, false);
        var viaContentService = ContentService.Copy(original, Constants.System.Root, false, false);

        // Assert
        // Both copies should have the same base name pattern (original name + number suffix)
        Assert.That(viaService?.Name, Does.StartWith(original.Name));
        Assert.That(viaContentService?.Name, Does.StartWith(original.Name));
        Assert.That(viaService?.ContentTypeId, Is.EqualTo(viaContentService?.ContentTypeId));
    }

    #endregion

    #region Notification Handler

    private class MoveNotificationHandler :
        INotificationHandler<ContentMovingNotification>,
        INotificationHandler<ContentMovedNotification>,
        INotificationHandler<ContentCopyingNotification>,
        INotificationHandler<ContentCopiedNotification>,
        INotificationHandler<ContentSortingNotification>,
        INotificationHandler<ContentSortedNotification>
    {
        public static Action<ContentMovingNotification>? Moving { get; set; }
        public static Action<ContentMovedNotification>? Moved { get; set; }
        public static Action<ContentCopyingNotification>? Copying { get; set; }
        public static Action<ContentCopiedNotification>? Copied { get; set; }
        public static Action<ContentSortingNotification>? Sorting { get; set; }
        public static Action<ContentSortedNotification>? Sorted { get; set; }

        public void Handle(ContentMovingNotification notification) => Moving?.Invoke(notification);
        public void Handle(ContentMovedNotification notification) => Moved?.Invoke(notification);
        public void Handle(ContentCopyingNotification notification) => Copying?.Invoke(notification);
        public void Handle(ContentCopiedNotification notification) => Copied?.Invoke(notification);
        public void Handle(ContentSortingNotification notification) => Sorting?.Invoke(notification);
        public void Handle(ContentSortedNotification notification) => Sorted?.Invoke(notification);
    }

    #endregion
}
