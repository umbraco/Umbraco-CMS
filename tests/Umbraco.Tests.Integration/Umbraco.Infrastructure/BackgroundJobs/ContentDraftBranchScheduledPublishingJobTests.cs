// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.DistributedJobs;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.BackgroundJobs;

/// <summary>
///     Tests covering <see cref="ContentDraftBranchScheduledPublishingJob" /> — the background job that
///     automatically promotes named draft branches once their scheduled publish date has passed, then queues
///     a scheduled release on the resulting default draft so the ordinary <see cref="ScheduledPublishingJob"/>
///     performs the actual publish on its own next run.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
internal sealed class ContentDraftBranchScheduledPublishingJobTests : UmbracoIntegrationTestWithContent
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) =>
        builder.AddNotificationHandler<ContentPublishedNotification, ContentPublishedNotificationHandler>();

    private IContentDraftBranchService ContentDraftBranchService => GetRequiredService<IContentDraftBranchService>();

    private IElementDraftBranchService ElementDraftBranchService => GetRequiredService<IElementDraftBranchService>();

    private IContentDraftBranchRepository ContentDraftBranchRepository => GetRequiredService<IContentDraftBranchRepository>();

    private IElementService ElementService => GetRequiredService<IElementService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private ContentDraftBranchScheduledPublishingJob CreateJob() =>
        new(
            NullLogger<ContentDraftBranchScheduledPublishingJob>.Instance,
            new TestOptionsMonitor<ScheduledPublishingSettings>(new ScheduledPublishingSettings()),
            ContentDraftBranchRepository,
            ContentDraftBranchService,
            ElementDraftBranchService,
            GetRequiredService<IContentService>(),
            ElementService,
            GetRequiredService<IUserIdKeyResolver>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<IUmbracoContextFactory>(),
            GetRequiredService<IServerMessenger>());

    private ScheduledPublishingJob CreateOrdinaryScheduledPublishingJob() =>
        new(
            GetRequiredService<IContentService>(),
            ElementService,
            GetRequiredService<IUmbracoContextFactory>(),
            NullLogger<ScheduledPublishingJob>.Instance,
            GetRequiredService<IServerMessenger>(),
            GetRequiredService<ICoreScopeProvider>(),
            TimeProvider.System,
            new TestOptionsMonitor<ScheduledPublishingSettings>(new ScheduledPublishingSettings()));

    private async Task<IElement> CreateElementAsync(string title)
    {
        IContentType elementType = ContentTypeBuilder.CreateSimpleElementType();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        IElement element = new Element("My Element", elementType);
        element.SetValue("title", title);
        OperationResult result = ElementService.Save(element);
        Assert.That(result.Success, Is.True);

        return element;
    }

    [Test]
    public async Task ExecuteAsync_Promotes_A_Due_Branch_And_Queues_A_Publish_Without_Publishing_Directly()
    {
        Textpage.Name = "Original";
        ContentService.Save(Textpage, -1);

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        IContent branchContent = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key)!;
        branchContent.Name = "Black Friday Edit";
        await ContentDraftBranchService.SaveAsync(Textpage.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        Attempt<ContentDraftBranchOperationStatus> scheduleResult =
            ContentDraftBranchService.Schedule(Textpage.Key, branch.Key, DateTime.UtcNow.AddMinutes(-5), new HashSet<string>());
        Assert.That(scheduleResult.Success, Is.True);

        await CreateJob().ExecuteAsync();

        IContentDraftBranch? branchAfter = ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key);
        Assert.That(branchAfter, Is.Null);

        // the branch was promoted (content copied forward), but publishing itself is deferred to the ordinary
        // ScheduledPublishingJob - this job never publishes directly.
        IContent? updated = ContentService.GetById(Textpage.Key);
        Assert.Multiple(() =>
        {
            Assert.That(updated!.Published, Is.False);
            Assert.That(updated.Name, Is.EqualTo("Black Friday Edit"));
        });

        ContentScheduleCollection schedule = ContentService.GetContentScheduleByContentId(Textpage.Id);
        Assert.That(schedule.GetPending(ContentScheduleAction.Release, DateTime.UtcNow), Is.Not.Empty);
    }

    [Test]
    public async Task ExecuteAsync_And_Then_The_Ordinary_ScheduledPublishingJob_Together_Publish_A_Due_Branch()
    {
        Textpage.Name = "Original";
        ContentService.Save(Textpage, -1);

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        IContent branchContent = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key)!;
        branchContent.Name = "Black Friday Edit";
        await ContentDraftBranchService.SaveAsync(Textpage.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        ContentDraftBranchService.Schedule(Textpage.Key, branch.Key, DateTime.UtcNow.AddMinutes(-5), new HashSet<string>());

        var publishedNotificationFired = false;
        ContentPublishedNotificationHandler.PublishedContent = _ => publishedNotificationFired = true;
        try
        {
            // step 1: the branch job promotes the branch and queues a release - it does not publish.
            await CreateJob().ExecuteAsync();

            IContent? afterBranchJob = ContentService.GetById(Textpage.Key);
            Assert.That(afterBranchJob!.Published, Is.False, "the branch job must never publish directly.");
            Assert.That(publishedNotificationFired, Is.False);

            // step 2: the ordinary ScheduledPublishingJob is the one that actually performs the publish.
            await CreateOrdinaryScheduledPublishingJob().ExecuteAsync();

            IContent? published = ContentService.GetById(Textpage.Key);
            Assert.Multiple(() =>
            {
                Assert.That(published!.Published, Is.True);
                Assert.That(published.Name, Is.EqualTo("Black Friday Edit"));
                Assert.That(publishedNotificationFired, Is.True, "the ordinary ContentPublishedNotification should fire from the ordinary job, not any branch-specific notification.");
            });
        }
        finally
        {
            ContentPublishedNotificationHandler.PublishedContent = null;
        }
    }

    [Test]
    public async Task ExecuteAsync_Does_Not_Publish_A_Branch_Scheduled_In_The_Future()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        ContentDraftBranchService.Schedule(Textpage.Key, branch.Key, DateTime.UtcNow.AddDays(1), new HashSet<string>());

        await CreateJob().ExecuteAsync();

        IContentDraftBranch? branchAfter = ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key);
        Assert.That(branchAfter, Is.Not.Null);
    }

    [Test]
    public async Task ExecuteAsync_Does_Not_Publish_An_Unscheduled_Branch()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        await CreateJob().ExecuteAsync();

        IContentDraftBranch? branchAfter = ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key);
        Assert.That(branchAfter, Is.Not.Null);
    }

    [Test]
    public async Task ExecuteAsync_Promotes_A_Due_Element_Branch_And_Queues_A_Publish_Without_Publishing_Directly()
    {
        IElement defaultDraft = await CreateElementAsync("Original");

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ElementDraftBranchService.CreateAsync(defaultDraft.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        IElement branchContent = ElementDraftBranchService.GetContent(defaultDraft.Key, branch.Key)!;
        branchContent.SetValue("title", "Black Friday Edit");
        await ElementDraftBranchService.SaveAsync(defaultDraft.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        ElementDraftBranchService.Schedule(defaultDraft.Key, branch.Key, DateTime.UtcNow.AddMinutes(-5), new HashSet<string>());

        await CreateJob().ExecuteAsync();

        IContentDraftBranch? branchAfter = ElementDraftBranchService.GetByKey(defaultDraft.Key, branch.Key);
        Assert.That(branchAfter, Is.Null);

        IElement? updated = ElementService.GetById(defaultDraft.Key);
        Assert.Multiple(() =>
        {
            Assert.That(updated!.Published, Is.False);
            Assert.That(updated.GetValue<string>("title"), Is.EqualTo("Black Friday Edit"));
        });

        ContentScheduleCollection schedule = ElementService.GetContentScheduleByContentId(defaultDraft.Id);
        Assert.That(schedule.GetPending(ContentScheduleAction.Release, DateTime.UtcNow), Is.Not.Empty);
    }

    [Test]
    public async Task GetScheduledForPublish_Only_Returns_Due_Publishable_Branches()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> due =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Due", Constants.Security.SuperUserKey);
        ContentDraftBranchService.Schedule(Textpage.Key, due.Result!.Key, DateTime.UtcNow.AddMinutes(-5), new HashSet<string>());

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> future =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Future", Constants.Security.SuperUserKey);
        ContentDraftBranchService.Schedule(Textpage.Key, future.Result!.Key, DateTime.UtcNow.AddDays(1), new HashSet<string>());

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> unscheduled =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Unscheduled", Constants.Security.SuperUserKey);

        using (ICoreScope scope = GetRequiredService<ICoreScopeProvider>().CreateCoreScope(autoComplete: true))
        {
            List<int> dueIds = ContentDraftBranchRepository.GetScheduledForPublish(DateTime.UtcNow).Select(b => b.Id).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(dueIds, Does.Contain(due.Result!.Id));
                Assert.That(dueIds, Does.Not.Contain(future.Result!.Id));
                Assert.That(dueIds, Does.Not.Contain(unscheduled.Result!.Id));
            });
        }
    }

    [Test]
    public async Task ExecuteAsync_Queues_A_Release_Only_For_The_Branchs_Scheduled_Cultures()
    {
        await LanguageService.CreateAsync(new Language("da-DK", "Danish") { FallbackIsoCode = "en-US" }, Constants.Security.SuperUserKey);

        IContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbCultureScopedSchedulePage", "Culture Scoped Schedule Page", defaultTemplateId: ContentType.DefaultTemplateId);
        contentType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IContent content = ContentBuilder.CreateSimpleContent(contentType, "Original EN", culture: "en-US");
        content.SetCultureName("Original EN", "en-US");
        content.SetCultureName("Original DA", "da-DK");
        ContentService.Save(content);
        ContentService.Publish(content, ["en-US", "da-DK"]);

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(content.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        IContent branchContent = ContentDraftBranchService.GetContent(content.Key, branch.Key)!;
        branchContent.SetCultureName("Black Friday EN", "en-US");
        branchContent.SetCultureName("Black Friday DA", "da-DK");
        await ContentDraftBranchService.SaveAsync(content.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        // schedule only en-US, not da-DK
        ContentDraftBranchService.Schedule(content.Key, branch.Key, DateTime.UtcNow.AddMinutes(-5), new HashSet<string> { "en-US" });

        await CreateJob().ExecuteAsync();

        ContentScheduleCollection schedule = ContentService.GetContentScheduleByContentId(content.Id);
        List<string> pendingCultures = schedule.GetPending(ContentScheduleAction.Release, DateTime.UtcNow)
            .Select(s => s.Culture)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(pendingCultures, Does.Contain("en-US"));
            Assert.That(pendingCultures, Does.Not.Contain("da-DK"));
        });
    }

    internal sealed class ContentPublishedNotificationHandler : INotificationHandler<ContentPublishedNotification>
    {
        public static Action<ContentPublishedNotification> PublishedContent { get; set; }

        public void Handle(ContentPublishedNotification notification) => PublishedContent?.Invoke(notification);
    }
}
