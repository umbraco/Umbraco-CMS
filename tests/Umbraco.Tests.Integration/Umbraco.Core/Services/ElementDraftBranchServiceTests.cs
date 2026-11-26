// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Tests covering <see cref="IElementDraftBranchService" /> — named draft branches forked from an element's
///     current default draft. Mirrors <see cref="ContentDraftBranchServiceTests" /> for documents, verifying the
///     shared <see cref="ContentDraftBranchServiceBase{TContent,TRepository}" /> implementation behaves the same
///     way for elements.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
internal sealed class ElementDraftBranchServiceTests : UmbracoIntegrationTest
{
    private IElementDraftBranchService ElementDraftBranchService => GetRequiredService<IElementDraftBranchService>();

    private IElementService ElementService => GetRequiredService<IElementService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private async Task<IElement> CreateElementAsync(string title, string? elementTypeAlias = null)
    {
        IContentType elementType = elementTypeAlias is null
            ? ContentTypeBuilder.CreateSimpleElementType()
            : ContentTypeBuilder.CreateSimpleElementType(elementTypeAlias);
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        IElement element = new Element("My Element", elementType);
        element.SetValue("title", title);
        OperationResult result = ElementService.Save(element);
        Assert.That(result.Success, Is.True);

        return element;
    }

    [Test]
    public async Task CreateAsync_Forks_From_Default_Draft_Without_Affecting_It()
    {
        IElement defaultDraft = await CreateElementAsync("Original");

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> result =
            await ElementDraftBranchService.CreateAsync(defaultDraft.Key, "Black Friday", Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        IContentDraftBranch branch = result.Result!;
        Assert.Multiple(() =>
        {
            Assert.That(branch.NodeId, Is.EqualTo(defaultDraft.Id));
            Assert.That(branch.ContentKey, Is.EqualTo(defaultDraft.Key));
        });

        IElement? branchContent = ElementDraftBranchService.GetContent(defaultDraft.Key, branch.Key);
        Assert.That(branchContent, Is.Not.Null);
        Assert.That(branchContent!.GetValue<string>("title"), Is.EqualTo("Original"));

        IElement? defaultDraftAfter = ElementService.GetById(defaultDraft.Key);
        Assert.That(defaultDraftAfter!.GetValue<string>("title"), Is.EqualTo("Original"));
    }

    [Test]
    public async Task SaveAsync_Edits_Branch_Content_Without_Affecting_Default_Draft()
    {
        IElement defaultDraft = await CreateElementAsync("Original");

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ElementDraftBranchService.CreateAsync(defaultDraft.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        IElement branchContent = ElementDraftBranchService.GetContent(defaultDraft.Key, branch.Key)!;
        branchContent.SetValue("title", "Black Friday Edit");
        Attempt<ContentDraftBranchOperationStatus> saveResult =
            await ElementDraftBranchService.SaveAsync(defaultDraft.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        Assert.That(saveResult.Success, Is.True);

        IElement? reloadedBranchContent = ElementDraftBranchService.GetContent(defaultDraft.Key, branch.Key);
        IElement? defaultDraftAfter = ElementService.GetById(defaultDraft.Key);
        Assert.Multiple(() =>
        {
            Assert.That(reloadedBranchContent!.GetValue<string>("title"), Is.EqualTo("Black Friday Edit"));
            Assert.That(defaultDraftAfter!.GetValue<string>("title"), Is.EqualTo("Original"));
        });
    }

    [Test]
    public async Task PromoteAsync_Copies_The_Branchs_Edited_Content_Not_Just_Its_Forked_Snapshot()
    {
        IElement defaultDraft = await CreateElementAsync("Original");

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ElementDraftBranchService.CreateAsync(defaultDraft.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        IElement branchContent = ElementDraftBranchService.GetContent(defaultDraft.Key, branch.Key)!;
        branchContent.SetValue("title", "Black Friday Edit");
        await ElementDraftBranchService.SaveAsync(defaultDraft.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        Attempt<ContentDraftBranchOperationStatus> promoteResult =
            await ElementDraftBranchService.PromoteAsync(defaultDraft.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.That(promoteResult.Success, Is.True);

        // promoting copies the edit onto the default draft, but never publishes anything on its own
        IElement? defaultDraftAfter = ElementService.GetById(defaultDraft.Key);
        Assert.Multiple(() =>
        {
            Assert.That(defaultDraftAfter!.Published, Is.False);
            Assert.That(defaultDraftAfter.GetValue<string>("title"), Is.EqualTo("Black Friday Edit"));
        });

        // promoting is an immediate delete of the branch row, same as abandoning
        IContentDraftBranch? branchAfter = ElementDraftBranchService.GetByKey(defaultDraft.Key, branch.Key);
        Assert.That(branchAfter, Is.Null);
    }

    [Test]
    public async Task Abandon_Deletes_The_Branch()
    {
        IElement defaultDraft = await CreateElementAsync("Original");

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ElementDraftBranchService.CreateAsync(defaultDraft.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        Attempt<ContentDraftBranchOperationStatus> abandonResult = ElementDraftBranchService.Abandon(defaultDraft.Key, branch.Key);
        Assert.That(abandonResult.Success, Is.True);

        // abandoning is a hard, immediate delete - the branch row itself no longer exists
        IContentDraftBranch? reloaded = ElementDraftBranchService.GetByKey(defaultDraft.Key, branch.Key);
        IElement? branchContentAfterAbandon = ElementDraftBranchService.GetContent(defaultDraft.Key, branch.Key);
        Assert.Multiple(() =>
        {
            Assert.That(reloaded, Is.Null);
            Assert.That(branchContentAfterAbandon, Is.Null);
        });
    }

    [Test]
    public async Task GetVersions_With_And_Without_A_Branch_Key_Separates_Default_Draft_From_Branch_History()
    {
        IElement defaultDraft = await CreateElementAsync("Original");
        var defaultDraftVersionId = defaultDraft.VersionId;

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ElementDraftBranchService.CreateAsync(defaultDraft.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IElement branchContent = ElementDraftBranchService.GetContent(defaultDraft.Key, branch.Key)!;

        List<IElement> defaultVersions = ElementDraftBranchService.GetVersions(defaultDraft.Key).ToList();
        List<IElement> branchVersions = ElementDraftBranchService.GetVersions(defaultDraft.Key, branch.Key).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(defaultVersions.Select(v => v.VersionId), Is.EquivalentTo(new[] { defaultDraftVersionId }));
            Assert.That(branchVersions.Select(v => v.VersionId), Is.EquivalentTo(new[] { branchContent.VersionId }));
        });
    }

    [Test]
    public async Task Schedule_Sets_And_Clears_The_Scheduled_Publish_Date()
    {
        IElement defaultDraft = await CreateElementAsync("Original");

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ElementDraftBranchService.CreateAsync(defaultDraft.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        var publishDate = new DateTime(2030, 11, 29, 0, 0, 0, DateTimeKind.Utc);

        Attempt<ContentDraftBranchOperationStatus> scheduleResult = ElementDraftBranchService.Schedule(defaultDraft.Key, branch.Key, publishDate, new HashSet<string>());
        Assert.That(scheduleResult.Success, Is.True);

        IContentDraftBranch? scheduled = ElementDraftBranchService.GetByKey(defaultDraft.Key, branch.Key);
        Assert.That(scheduled!.ScheduledPublishDate, Is.EqualTo(publishDate));

        Attempt<ContentDraftBranchOperationStatus> clearResult = ElementDraftBranchService.Schedule(defaultDraft.Key, branch.Key, null, new HashSet<string>());
        Assert.That(clearResult.Success, Is.True);

        IContentDraftBranch? cleared = ElementDraftBranchService.GetByKey(defaultDraft.Key, branch.Key);
        Assert.That(cleared!.ScheduledPublishDate, Is.Null);
    }

    [Test]
    public async Task PromoteAsync_Returns_BranchNotFound_When_Branch_Belongs_To_A_Different_Content_Item()
    {
        // mirrors ContentDraftBranchServiceTests' equivalent coverage for documents - proves the same
        // content-ownership scoping (GetValidatedBranch) is enforced by the shared base class for elements too.
        IElement defaultDraft = await CreateElementAsync("Original");
        IElement unrelatedElement = await CreateElementAsync("Unrelated", "unrelatedElementType");

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ElementDraftBranchService.CreateAsync(defaultDraft.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        Attempt<ContentDraftBranchOperationStatus> result =
            await ElementDraftBranchService.PromoteAsync(unrelatedElement.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.BranchNotFound));
        });

        // the branch was NOT promoted (and thus not deleted) by the mismatched-content attempt
        Assert.That(ElementDraftBranchService.GetByKey(defaultDraft.Key, branch.Key), Is.Not.Null);
    }
}
