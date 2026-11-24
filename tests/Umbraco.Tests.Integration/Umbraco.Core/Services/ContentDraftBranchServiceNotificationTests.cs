// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Tests covering the notifications fired by <see cref="IContentDraftBranchService" /> around
///     create, save, promote, and abandon.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
internal sealed class ContentDraftBranchServiceNotificationTests : UmbracoIntegrationTestWithContent
{
    private IContentDraftBranchService ContentDraftBranchService => GetRequiredService<IContentDraftBranchService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<ContentDraftBranchCreatingNotification, ContentDraftBranchNotificationHandler>()
        .AddNotificationHandler<ContentDraftBranchCreatedNotification, ContentDraftBranchNotificationHandler>()
        .AddNotificationHandler<ContentDraftBranchSavingNotification, ContentDraftBranchNotificationHandler>()
        .AddNotificationHandler<ContentDraftBranchSavedNotification, ContentDraftBranchNotificationHandler>()
        .AddNotificationHandler<ContentDraftBranchPromotingNotification, ContentDraftBranchNotificationHandler>()
        .AddNotificationHandler<ContentDraftBranchPromotedNotification, ContentDraftBranchNotificationHandler>()
        .AddNotificationHandler<ContentDraftBranchAbandoningNotification, ContentDraftBranchNotificationHandler>()
        .AddNotificationHandler<ContentDraftBranchAbandonedNotification, ContentDraftBranchNotificationHandler>()
        .AddNotificationHandler<ContentPublishingNotification, ContentPublishNotificationHandler>()
        .AddNotificationHandler<ContentPublishedNotification, ContentPublishNotificationHandler>();

    [TearDown]
    public void ResetNotificationHandlers()
    {
        ContentDraftBranchNotificationHandler.Creating = null;
        ContentDraftBranchNotificationHandler.Created = null;
        ContentDraftBranchNotificationHandler.Saving = null;
        ContentDraftBranchNotificationHandler.Saved = null;
        ContentDraftBranchNotificationHandler.Promoting = null;
        ContentDraftBranchNotificationHandler.Promoted = null;
        ContentDraftBranchNotificationHandler.Abandoning = null;
        ContentDraftBranchNotificationHandler.Abandoned = null;
        ContentPublishNotificationHandler.Publishing = null;
        ContentPublishNotificationHandler.Published = null;
    }

    [Test]
    public async Task CreateAsync_Fires_Creating_And_Created_Notifications()
    {
        var creatingFired = false;
        var createdFired = false;
        ContentDraftBranchNotificationHandler.Creating = _ => creatingFired = true;
        ContentDraftBranchNotificationHandler.Created = _ => createdFired = true;

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> result =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(creatingFired, Is.True);
            Assert.That(createdFired, Is.True);
        });
    }

    [Test]
    public async Task Cancelling_Creating_Notification_Prevents_Branch_Creation()
    {
        ContentDraftBranchNotificationHandler.Creating = n => n.Cancel = true;

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> result =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentDraftBranchOperationStatus.CancelledByNotification));
        });

        Assert.That(ContentDraftBranchService.GetByContentKey(Textpage.Key), Is.Empty);
    }

    [Test]
    public async Task SaveAsync_Fires_Saving_And_Saved_Notifications()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IContent branchContent = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key)!;
        branchContent.Name = "Black Friday Edit";

        var savingFired = false;
        var savedFired = false;
        ContentDraftBranchNotificationHandler.Saving = _ => savingFired = true;
        ContentDraftBranchNotificationHandler.Saved = _ => savedFired = true;

        Attempt<ContentDraftBranchOperationStatus> saveResult =
            await ContentDraftBranchService.SaveAsync(Textpage.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(saveResult.Success, Is.True);
            Assert.That(savingFired, Is.True);
            Assert.That(savedFired, Is.True);
        });
    }

    [Test]
    public async Task Cancelling_Saving_Notification_Prevents_Branch_Content_From_Being_Saved()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IContent branchContent = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key)!;
        branchContent.Name = "Should not be saved";

        ContentDraftBranchNotificationHandler.Saving = n => n.Cancel = true;

        Attempt<ContentDraftBranchOperationStatus> saveResult =
            await ContentDraftBranchService.SaveAsync(Textpage.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        Assert.That(saveResult.Success, Is.False);
        Assert.That(saveResult.Result, Is.EqualTo(ContentDraftBranchOperationStatus.CancelledByNotification));

        IContent? reloaded = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key);
        Assert.That(reloaded!.Name, Is.Not.EqualTo("Should not be saved"));
    }

    [Test]
    public async Task PromoteAsync_Fires_Promoting_And_Promoted_Notifications()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        var promotingFired = false;
        var promotedFired = false;
        ContentDraftBranchNotificationHandler.Promoting = _ => promotingFired = true;
        ContentDraftBranchNotificationHandler.Promoted = _ => promotedFired = true;

        Attempt<ContentDraftBranchOperationStatus> promoteResult =
            await ContentDraftBranchService.PromoteAsync(Textpage.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(promoteResult.Success, Is.True);
            Assert.That(promotingFired, Is.True);
            Assert.That(promotedFired, Is.True);
        });
    }

    [Test]
    public async Task PromoteAsync_Never_Fires_A_Publishing_Or_Published_Notification()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        var contentPublishingFired = false;
        var contentPublishedFired = false;
        ContentPublishNotificationHandler.Publishing = _ => contentPublishingFired = true;
        ContentPublishNotificationHandler.Published = _ => contentPublishedFired = true;

        Attempt<ContentDraftBranchOperationStatus> promoteResult =
            await ContentDraftBranchService.PromoteAsync(Textpage.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(promoteResult.Success, Is.True);
            Assert.That(contentPublishingFired, Is.False);
            Assert.That(contentPublishedFired, Is.False);
        });
    }

    [Test]
    public async Task Cancelling_Promoting_Notification_Prevents_Branch_Promotion()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        ContentDraftBranchNotificationHandler.Promoting = n => n.Cancel = true;

        Attempt<ContentDraftBranchOperationStatus> promoteResult =
            await ContentDraftBranchService.PromoteAsync(Textpage.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.That(promoteResult.Success, Is.False);
        Assert.That(promoteResult.Result, Is.EqualTo(ContentDraftBranchOperationStatus.CancelledByNotification));

        IContentDraftBranch? branchAfter = ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key);
        Assert.That(branchAfter, Is.Not.Null);

        IContent? defaultDraftAfter = ContentService.GetById(Textpage.Key);
        Assert.That(defaultDraftAfter!.Published, Is.False);
    }

    [Test]
    public async Task Abandon_Fires_Abandoning_And_Abandoned_Notifications()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        var abandoningFired = false;
        var abandonedFired = false;
        ContentDraftBranchNotificationHandler.Abandoning = _ => abandoningFired = true;
        ContentDraftBranchNotificationHandler.Abandoned = _ => abandonedFired = true;

        Attempt<ContentDraftBranchOperationStatus> abandonResult = ContentDraftBranchService.Abandon(Textpage.Key, branch.Key);

        Assert.Multiple(() =>
        {
            Assert.That(abandonResult.Success, Is.True);
            Assert.That(abandoningFired, Is.True);
            Assert.That(abandonedFired, Is.True);
        });
    }

    [Test]
    public async Task Cancelling_Abandoning_Notification_Prevents_Branch_Abandon()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        ContentDraftBranchNotificationHandler.Abandoning = n => n.Cancel = true;

        Attempt<ContentDraftBranchOperationStatus> abandonResult = ContentDraftBranchService.Abandon(Textpage.Key, branch.Key);

        Assert.That(abandonResult.Success, Is.False);
        Assert.That(abandonResult.Result, Is.EqualTo(ContentDraftBranchOperationStatus.CancelledByNotification));

        IContentDraftBranch? branchAfter = ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key);
        Assert.That(branchAfter, Is.Not.Null);
    }

    internal sealed class ContentDraftBranchNotificationHandler :
        INotificationHandler<ContentDraftBranchCreatingNotification>,
        INotificationHandler<ContentDraftBranchCreatedNotification>,
        INotificationHandler<ContentDraftBranchSavingNotification>,
        INotificationHandler<ContentDraftBranchSavedNotification>,
        INotificationHandler<ContentDraftBranchPromotingNotification>,
        INotificationHandler<ContentDraftBranchPromotedNotification>,
        INotificationHandler<ContentDraftBranchAbandoningNotification>,
        INotificationHandler<ContentDraftBranchAbandonedNotification>
    {
        public static Action<ContentDraftBranchCreatingNotification>? Creating { get; set; }

        public static Action<ContentDraftBranchCreatedNotification>? Created { get; set; }

        public static Action<ContentDraftBranchSavingNotification>? Saving { get; set; }

        public static Action<ContentDraftBranchSavedNotification>? Saved { get; set; }

        public static Action<ContentDraftBranchPromotingNotification>? Promoting { get; set; }

        public static Action<ContentDraftBranchPromotedNotification>? Promoted { get; set; }

        public static Action<ContentDraftBranchAbandoningNotification>? Abandoning { get; set; }

        public static Action<ContentDraftBranchAbandonedNotification>? Abandoned { get; set; }

        public void Handle(ContentDraftBranchCreatingNotification notification) => Creating?.Invoke(notification);

        public void Handle(ContentDraftBranchCreatedNotification notification) => Created?.Invoke(notification);

        public void Handle(ContentDraftBranchSavingNotification notification) => Saving?.Invoke(notification);

        public void Handle(ContentDraftBranchSavedNotification notification) => Saved?.Invoke(notification);

        public void Handle(ContentDraftBranchPromotingNotification notification) => Promoting?.Invoke(notification);

        public void Handle(ContentDraftBranchPromotedNotification notification) => Promoted?.Invoke(notification);

        public void Handle(ContentDraftBranchAbandoningNotification notification) => Abandoning?.Invoke(notification);

        public void Handle(ContentDraftBranchAbandonedNotification notification) => Abandoned?.Invoke(notification);
    }

    internal sealed class ContentPublishNotificationHandler :
        INotificationHandler<ContentPublishingNotification>,
        INotificationHandler<ContentPublishedNotification>
    {
        public static Action<ContentPublishingNotification>? Publishing { get; set; }

        public static Action<ContentPublishedNotification>? Published { get; set; }

        public void Handle(ContentPublishingNotification notification) => Publishing?.Invoke(notification);

        public void Handle(ContentPublishedNotification notification) => Published?.Invoke(notification);
    }
}
