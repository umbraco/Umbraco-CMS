// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
/// Integration tests specifically for validating ContentService refactoring.
/// These tests establish behavioral baselines that must pass throughout the refactoring phases.
/// </summary>
[TestFixture]
[NonParallelizable] // Required: static notification handler state is shared across tests
[Category("Refactoring")] // v1.2: Added for easier test filtering during refactoring
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true,
    Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class ContentServiceRefactoringTests : UmbracoIntegrationTestWithContent
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
    private IUserService UserService => GetRequiredService<IUserService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<ContentSavingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentSavedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentPublishingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentPublishedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentUnpublishingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentUnpublishedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentMovingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentMovedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentMovingToRecycleBinNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentMovedToRecycleBinNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentSortingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentSortedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentDeletingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentDeletedNotification, RefactoringTestNotificationHandler>();

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        RefactoringTestNotificationHandler.Reset();
    }

    [TearDown]
    public void Teardown()
    {
        RefactoringTestNotificationHandler.Reset();
    }

    #region Notification Ordering Tests

    /// <summary>
    /// Test 1: Verifies that MoveToRecycleBin for published content fires notifications in the correct order.
    /// Expected order: MovingToRecycleBin -> MovedToRecycleBin
    /// Note: As per design doc, MoveToRecycleBin does NOT unpublish first - content is "masked" not unpublished.
    /// </summary>
    [Test]
    public void MoveToRecycleBin_PublishedContent_FiresNotificationsInCorrectOrder()
    {
        // Arrange - Create and publish content
        // First publish parent if not already published
        if (!Textpage.Published)
        {
            ContentService.Publish(Textpage, new[] { "*" });
        }

        var content = ContentBuilder.CreateSimpleContent(ContentType, "TestContent", Textpage.Id);
        ContentService.Save(content);
        ContentService.Publish(content, new[] { "*" });

        // Verify it's published
        Assert.That(content.Published, Is.True, "Content should be published before test");

        // Clear notification tracking
        RefactoringTestNotificationHandler.Reset();

        // Act
        var result = ContentService.MoveToRecycleBin(content);

        // Assert
        Assert.That(result.Success, Is.True, "MoveToRecycleBin should succeed");

        var notifications = RefactoringTestNotificationHandler.NotificationOrder;

        // Verify notification sequence
        Assert.That(notifications, Does.Contain(nameof(ContentMovingToRecycleBinNotification)),
            "MovingToRecycleBin notification should fire");
        Assert.That(notifications, Does.Contain(nameof(ContentMovedToRecycleBinNotification)),
            "MovedToRecycleBin notification should fire");

        // Verify order: Moving comes before Moved
        var movingIndex = notifications.IndexOf(nameof(ContentMovingToRecycleBinNotification));
        var movedIndex = notifications.IndexOf(nameof(ContentMovedToRecycleBinNotification));
        Assert.That(movingIndex, Is.LessThan(movedIndex),
            "MovingToRecycleBin should fire before MovedToRecycleBin");
    }

    /// <summary>
    /// Test 2: Verifies that MoveToRecycleBin for unpublished content only fires move notifications.
    /// No publish/unpublish notifications should be fired.
    /// </summary>
    [Test]
    public void MoveToRecycleBin_UnpublishedContent_OnlyFiresMoveNotifications()
    {
        // Arrange - Create content but don't publish
        // First publish parent if not already published (required for creating child content)
        if (!Textpage.Published)
        {
            ContentService.Publish(Textpage, new[] { "*" });
        }

        var content = ContentBuilder.CreateSimpleContent(ContentType, "UnpublishedContent", Textpage.Id);
        ContentService.Save(content);

        // Verify it's not published
        Assert.That(content.Published, Is.False, "Content should not be published before test");

        // Clear notification tracking
        RefactoringTestNotificationHandler.Reset();

        // Act
        var result = ContentService.MoveToRecycleBin(content);

        // Assert
        Assert.That(result.Success, Is.True, "MoveToRecycleBin should succeed");

        var notifications = RefactoringTestNotificationHandler.NotificationOrder;

        // Verify move notifications fire
        Assert.That(notifications, Does.Contain(nameof(ContentMovingToRecycleBinNotification)),
            "MovingToRecycleBin notification should fire");
        Assert.That(notifications, Does.Contain(nameof(ContentMovedToRecycleBinNotification)),
            "MovedToRecycleBin notification should fire");

        // Verify no publish/unpublish notifications
        Assert.That(notifications, Does.Not.Contain(nameof(ContentPublishingNotification)),
            "Publishing notification should not fire for unpublished content");
        Assert.That(notifications, Does.Not.Contain(nameof(ContentPublishedNotification)),
            "Published notification should not fire for unpublished content");
        Assert.That(notifications, Does.Not.Contain(nameof(ContentUnpublishingNotification)),
            "Unpublishing notification should not fire for unpublished content");
        Assert.That(notifications, Does.Not.Contain(nameof(ContentUnpublishedNotification)),
            "Unpublished notification should not fire for unpublished content");
    }

    #endregion

    #region Sort Operation Tests

    // Tests 3-5 will be added in Task 3

    #endregion

    #region DeleteOfType Tests

    // Tests 6-8 will be added in Task 4

    #endregion

    #region Permission Tests

    // Tests 9-12 will be added in Task 5

    #endregion

    #region Transaction Boundary Tests

    // Tests 13-15 will be added in Task 6

    #endregion

    /// <summary>
    /// Notification handler that tracks the order of notifications for test verification.
    /// </summary>
    internal sealed class RefactoringTestNotificationHandler :
        INotificationHandler<ContentSavingNotification>,
        INotificationHandler<ContentSavedNotification>,
        INotificationHandler<ContentPublishingNotification>,
        INotificationHandler<ContentPublishedNotification>,
        INotificationHandler<ContentUnpublishingNotification>,
        INotificationHandler<ContentUnpublishedNotification>,
        INotificationHandler<ContentMovingNotification>,
        INotificationHandler<ContentMovedNotification>,
        INotificationHandler<ContentMovingToRecycleBinNotification>,
        INotificationHandler<ContentMovedToRecycleBinNotification>,
        INotificationHandler<ContentSortingNotification>,
        INotificationHandler<ContentSortedNotification>,
        INotificationHandler<ContentDeletingNotification>,
        INotificationHandler<ContentDeletedNotification>
    {
        private static readonly List<string> _notificationOrder = new();
        private static readonly object _lock = new();

        public static IReadOnlyList<string> NotificationOrder
        {
            get
            {
                lock (_lock)
                {
                    return _notificationOrder.ToList();
                }
            }
        }

        public static void Reset()
        {
            lock (_lock)
            {
                _notificationOrder.Clear();
            }
        }

        private static void Record(string notificationType)
        {
            lock (_lock)
            {
                _notificationOrder.Add(notificationType);
            }
        }

        public void Handle(ContentSavingNotification notification) => Record(nameof(ContentSavingNotification));
        public void Handle(ContentSavedNotification notification) => Record(nameof(ContentSavedNotification));
        public void Handle(ContentPublishingNotification notification) => Record(nameof(ContentPublishingNotification));
        public void Handle(ContentPublishedNotification notification) => Record(nameof(ContentPublishedNotification));
        public void Handle(ContentUnpublishingNotification notification) => Record(nameof(ContentUnpublishingNotification));
        public void Handle(ContentUnpublishedNotification notification) => Record(nameof(ContentUnpublishedNotification));
        public void Handle(ContentMovingNotification notification) => Record(nameof(ContentMovingNotification));
        public void Handle(ContentMovedNotification notification) => Record(nameof(ContentMovedNotification));
        public void Handle(ContentMovingToRecycleBinNotification notification) => Record(nameof(ContentMovingToRecycleBinNotification));
        public void Handle(ContentMovedToRecycleBinNotification notification) => Record(nameof(ContentMovedToRecycleBinNotification));
        public void Handle(ContentSortingNotification notification) => Record(nameof(ContentSortingNotification));
        public void Handle(ContentSortedNotification notification) => Record(nameof(ContentSortedNotification));
        public void Handle(ContentDeletingNotification notification) => Record(nameof(ContentDeletingNotification));
        public void Handle(ContentDeletedNotification notification) => Record(nameof(ContentDeletedNotification));
    }
}
