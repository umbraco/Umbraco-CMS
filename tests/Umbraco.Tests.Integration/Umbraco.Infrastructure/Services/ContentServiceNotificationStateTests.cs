// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Verifies that the state of a starting ("ing") content notification reaches every ending ("ed") notification
///     the operation publishes.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ContentServiceNotificationStateTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentVersionService ContentVersionService => GetRequiredService<IContentVersionService>();

    private NotificationStateRecorder Recorder => GetRequiredService<NotificationStateRecorder>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<NotificationStateRecorder>();
        builder
            .AddNotificationHandler<ContentDeletingNotification, NotificationStateWritingHandler<ContentDeletingNotification>>()
            .AddNotificationHandler<ContentDeletedNotification, NotificationStateReadingHandler<ContentDeletedNotification>>()
            .AddNotificationHandler<ContentPublishingNotification, NotificationStateWritingHandler<ContentPublishingNotification>>()
            .AddNotificationHandler<ContentPublishedNotification, NotificationStateReadingHandler<ContentPublishedNotification>>()
            .AddNotificationHandler<ContentDeletingVersionsNotification, NotificationStateWritingHandler<ContentDeletingVersionsNotification>>()
            .AddNotificationHandler<ContentDeletedVersionsNotification, NotificationStateReadingHandler<ContentDeletedVersionsNotification>>();
    }

    [Test]
    public async Task Delete_Shares_Deleting_State_With_Deleted_Notifications_For_Item_And_Descendants()
    {
        IContentType contentType = await CreateContentType();
        IContent root = CreateContent(contentType, "Root");
        IContent child = CreateContent(contentType, "Child", root);
        IContent grandchild = CreateContent(contentType, "Grandchild", child);
        Recorder.Starting.Clear();

        ContentService.Delete(root);

        AssertDeletedNotificationsShareDeletingState([root.Key, child.Key, grandchild.Key]);
    }

    [Test]
    public async Task EmptyRecycleBin_Shares_Deleting_State_With_Deleted_Notifications_For_Items_And_Descendants()
    {
        IContentType contentType = await CreateContentType();
        IContent first = CreateContent(contentType, "First");
        IContent firstChild = CreateContent(contentType, "First Child", first);
        IContent second = CreateContent(contentType, "Second");
        ContentService.MoveToRecycleBin(first);
        ContentService.MoveToRecycleBin(second);
        Recorder.Starting.Clear();

        ContentService.EmptyRecycleBin();

        AssertDeletedNotificationsShareDeletingState([first.Key, firstChild.Key, second.Key]);
    }

    [Test]
    public async Task DeleteOfType_Shares_Deleting_State_With_Deleted_Notifications()
    {
        IContentType contentType = await CreateContentType();
        IContent first = CreateContent(contentType, "First");
        IContent second = CreateContent(contentType, "Second");
        Recorder.Starting.Clear();

        ContentService.DeleteOfType(contentType.Id);

        AssertDeletedNotificationsShareDeletingState([first.Key, second.Key]);
    }

    [Test]
    public async Task PublishBranch_Shares_Root_Publishing_State_With_Published_Notification()
    {
        IContentType contentType = await CreateContentType();
        IContent root = CreateContent(contentType, "Root");
        IContent child = CreateContent(contentType, "Child", root);

        ContentService.PublishBranch(root, PublishBranchFilter.IncludeUnpublished, ["*"]);

        ContentPublishingNotification rootPublishing = Recorder.Starting
            .OfType<ContentPublishingNotification>()
            .Single(x => x.PublishedEntities.Any(e => e.Key == root.Key));
        ContentPublishedNotification published = Recorder.Ending.OfType<ContentPublishedNotification>().Single();

        Assert.Multiple(() =>
        {
            Assert.That(published.PublishedEntities.Select(x => x.Key), Is.EquivalentTo(new[] { root.Key, child.Key }));
            Assert.That(published.State, Is.SameAs(rootPublishing.State));
            Assert.That(published.State.ContainsKey(NotificationStateRecorder.StateKey), Is.True);
        });
    }

    [Test]
    public async Task PerformContentVersionCleanup_Shares_Deleting_State_With_Deleted_Notification_Per_Version()
    {
        IContentType contentType = await CreateContentType(x =>
        {
            x.HistoryCleanup!.PreventCleanup = false;
            x.HistoryCleanup.KeepAllVersionsNewerThanDays = 0;
            x.HistoryCleanup.KeepLatestVersionPerDayForDays = 0;
        });

        IContent content = CreateContent(contentType, "Versioned");
        for (var i = 0; i < 3; i++)
        {
            ContentService.Publish(content, ["*"]);
        }

        ContentVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));

        ContentDeletingVersionsNotification[] deleting = Recorder.Starting.OfType<ContentDeletingVersionsNotification>().ToArray();
        ContentDeletedVersionsNotification[] deleted = Recorder.Ending.OfType<ContentDeletedVersionsNotification>().ToArray();

        Assert.That(deleted, Has.Length.GreaterThan(1));
        Assert.That(deleted, Has.Length.EqualTo(deleting.Length));
        Assert.Multiple(() =>
        {
            foreach (ContentDeletedVersionsNotification notification in deleted)
            {
                ContentDeletingVersionsNotification matching = deleting.Single(x => x.SpecificVersion == notification.SpecificVersion);
                Assert.That(notification.State, Is.SameAs(matching.State), $"Version {notification.SpecificVersion}");
            }
        });
    }

    private void AssertDeletedNotificationsShareDeletingState(Guid[] expectedDeletedKeys)
    {
        ContentDeletingNotification deleting = Recorder.Starting.OfType<ContentDeletingNotification>().Single();
        ContentDeletedNotification[] deleted = Recorder.Ending.OfType<ContentDeletedNotification>().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(deleted.SelectMany(x => x.DeletedEntities).Select(x => x.Key), Is.EquivalentTo(expectedDeletedKeys));
            Assert.That(deleted.Select(x => x.State), Has.All.SameAs(deleting.State));
            Assert.That(deleting.State.ContainsKey(NotificationStateRecorder.StateKey), Is.True);
        });
    }

    private async Task<IContentType> CreateContentType(Action<IContentType>? configure = null)
    {
        ITemplate template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        IContentType contentType = ContentTypeBuilder.CreateSimpleContentType("statePage", "State Page", defaultTemplateId: template.Id);
        configure?.Invoke(contentType);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType;
    }

    private IContent CreateContent(IContentType contentType, string name, IContent? parent = null)
    {
        Content content = parent is null
            ? ContentBuilder.CreateSimpleContent(contentType, name)
            : ContentBuilder.CreateSimpleContent(contentType, name, parent);
        ContentService.Save(content);
        return content;
    }
}
