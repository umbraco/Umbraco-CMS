using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Requirements for named draft branches, so that an umbrella feature (Draft Sets / Change Sets) can build on
///     them. Each test asserts the behaviour we agreed the feature needs. Most fail today, on purpose.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
internal sealed class ContentDraftBranchRequirementTests : UmbracoIntegrationTestWithContent
{
    private IContentDraftBranchService ContentDraftBranchService => GetRequiredService<IContentDraftBranchService>();

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    private ICoreScopeProvider CoreScopeProvider => GetRequiredService<ICoreScopeProvider>();

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<ContentSavedNotification, RequirementNotificationHandler>()
        .AddNotificationHandler<ContentDraftBranchPromotingNotification, RequirementNotificationHandler>();

    [TearDown]
    public void ResetNotificationHandlers()
    {
        RequirementNotificationHandler.ContentSavedCount = 0;
        RequirementNotificationHandler.Promoting = null;
    }

    // 1. Branch versions exist in isolation: saving one changes nothing on the node or its default draft.

    [Test]
    public async Task SaveAsync_Does_Not_Change_The_Nodes_Tree_Name()
    {
        (IContentDraftBranch branch, IContent branchContent) = await CreateBranchAsync(Textpage);
        branchContent.Name = "Black Friday Edit";
        await SaveBranchAsync(Textpage, branch, branchContent);

        Assert.Multiple(() =>
        {
            // passes today: IContent.Name is read from the default draft's version row
            Assert.That(ContentService.GetById(Textpage.Key)!.Name, Is.EqualTo("Textpage"));

            // fails today: the tree name is read from umbracoNode.text, which the branch save overwrites
            Assert.That(EntityService.Get(Textpage.Key)!.Name, Is.EqualTo("Textpage"));
        });
    }

    [Test]
    public async Task SaveAsync_Does_Not_Mark_A_Published_Node_As_Edited()
    {
        ContentService.Publish(ContentService.GetById(Textpage.Key)!, ["*"]);
        Assert.That(ContentService.GetById(Textpage.Key)!.Edited, Is.False, "precondition");

        (IContentDraftBranch branch, IContent branchContent) = await CreateBranchAsync(Textpage);
        branchContent.Name = "Black Friday Edit";
        await SaveBranchAsync(Textpage, branch, branchContent);

        Assert.That(ContentService.GetById(Textpage.Key)!.Edited, Is.False);
    }

    [Test]
    public async Task SaveAsync_Does_Not_Move_The_Node()
    {
        (IContentDraftBranch branch, IContent branchContent) = await CreateBranchAsync(Subpage2);
        branchContent.ParentId = Subpage.Id;
        await SaveBranchAsync(Subpage2, branch, branchContent);

        Assert.That(EntityService.Get(Subpage2.Key)!.ParentId, Is.EqualTo(Textpage.Id));
    }

    [Test]
    public async Task SaveAsync_Does_Not_Publish_Content_Saved_Notifications()
    {
        (IContentDraftBranch branch, IContent branchContent) = await CreateBranchAsync(Textpage);
        RequirementNotificationHandler.ContentSavedCount = 0;

        branchContent.Name = "Black Friday Edit";
        await SaveBranchAsync(Textpage, branch, branchContent);

        Assert.That(RequirementNotificationHandler.ContentSavedCount, Is.EqualTo(0));
    }

    // 2. An outer scope works as a unit of work: a failed promote rolls back the promotes before it.

    [Test]
    public async Task PromoteAsync_Within_An_Outer_Scope_Rolls_Back_Earlier_Promotes_When_A_Later_One_Is_Cancelled()
    {
        (IContentDraftBranch first, IContent firstContent) = await CreateBranchAsync(Subpage);
        firstContent.Name = "Promoted then rolled back";
        await SaveBranchAsync(Subpage, first, firstContent);

        (IContentDraftBranch second, _) = await CreateBranchAsync(Subpage2);

        // the promoting notification doesn't expose its branch (Target is protected), so cancel by call order
        var promotes = 0;
        RequirementNotificationHandler.Promoting = notification =>
        {
            if (++promotes == 2)
            {
                notification.Cancel = true;
            }
        };

        using (ICoreScope scope = CoreScopeProvider.CreateCoreScope())
        {
            Attempt<ContentDraftBranchOperationStatus> firstResult = await ContentDraftBranchService.PromoteAsync(
                Subpage.Key, first.Key, new HashSet<string>(), Constants.Security.SuperUserKey);
            Attempt<ContentDraftBranchOperationStatus> secondResult = await ContentDraftBranchService.PromoteAsync(
                Subpage2.Key, second.Key, new HashSet<string>(), Constants.Security.SuperUserKey);

            Assert.That(firstResult.Success, Is.True, "first promote");
            Assert.That(secondResult.Success, Is.False, "second promote");

            // a caller that completes the outer scope anyway must not get a partial result
            try
            {
                scope.Complete();
            }
            catch (InvalidOperationException)
            {
                // an uncompletable scope is an acceptable way to refuse
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(ContentService.GetById(Subpage.Key)!.Name, Is.EqualTo("Text Page 1"), "first promote rolled back");
            Assert.That(ContentDraftBranchService.GetByKey(Subpage.Key, first.Key), Is.Not.Null, "first branch kept");
        });
    }

    // 3. Deleting a node deletes its branches.

    [Test]
    public async Task Deleting_A_Node_With_Branches_Deletes_The_Branches()
    {
        (IContentDraftBranch branch, _) = await CreateBranchAsync(Subpage2);

        Assert.DoesNotThrow(() => ContentService.Delete(ContentService.GetById(Subpage2.Key)!));
        Assert.Multiple(() =>
        {
            Assert.That(ContentService.GetById(Subpage2.Key), Is.Null);
            Assert.That(ContentDraftBranchService.GetByKey(Subpage2.Key, branch.Key), Is.Null);
        });
    }

    // 4. Branch versions are not part of the node's history until they are promoted.

    [Test]
    public async Task Abandon_Does_Not_Add_The_Branch_Version_To_The_Nodes_History()
    {
        (IContentDraftBranch branch, IContent branchContent) = await CreateBranchAsync(Textpage);
        branchContent.Name = "Rejected idea";
        await SaveBranchAsync(Textpage, branch, branchContent);

        ContentDraftBranchService.Abandon(Textpage.Key, branch.Key);

        Assert.That(ContentService.GetVersions(Textpage.Id).Select(version => version.Name), Does.Not.Contain("Rejected idea"));
    }

    // 5. Conflict detection needs the values a branch started from, after the default draft has moved on.

    [Test]
    public async Task The_Branchs_Base_Values_Survive_A_Save_Of_The_Default_Draft()
    {
        (IContentDraftBranch branch, _) = await CreateBranchAsync(Textpage);

        IContent defaultDraft = ContentService.GetById(Textpage.Key)!;
        defaultDraft.Name = "Changed after fork";
        ContentService.Save(defaultDraft);

        // BasedOnVersionId points at the default draft's current version, which a plain save updates in place.
        // Wherever the base ends up being stored, it must still hold the values the branch started from.
        IContent? baseVersion = ContentService.GetVersion(branch.BasedOnVersionId!.Value);
        Assert.That(baseVersion?.Name, Is.EqualTo("Textpage"));
    }

    private async Task<(IContentDraftBranch Branch, IContent Content)> CreateBranchAsync(IContent content)
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(content.Key, "Black Friday", Constants.Security.SuperUserKey);
        Assert.That(created.Success, Is.True, "create branch");

        IContentDraftBranch branch = created.Result!;
        return (branch, ContentDraftBranchService.GetContent(content.Key, branch.Key)!);
    }

    private async Task SaveBranchAsync(IContent content, IContentDraftBranch branch, IContent branchContent)
    {
        Attempt<ContentDraftBranchOperationStatus> saved =
            await ContentDraftBranchService.SaveAsync(content.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);
        Assert.That(saved.Success, Is.True, "save branch");
    }

    internal sealed class RequirementNotificationHandler :
        INotificationHandler<ContentSavedNotification>,
        INotificationHandler<ContentDraftBranchPromotingNotification>
    {
        public static int ContentSavedCount { get; set; }

        public static Action<ContentDraftBranchPromotingNotification>? Promoting { get; set; }

        public void Handle(ContentSavedNotification notification) => ContentSavedCount++;

        public void Handle(ContentDraftBranchPromotingNotification notification) => Promoting?.Invoke(notification);
    }
}
