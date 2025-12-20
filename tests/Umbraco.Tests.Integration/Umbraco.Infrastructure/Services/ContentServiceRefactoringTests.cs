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

    // Tests 1-2 will be added in Task 2

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
