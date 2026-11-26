// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Tests covering <see cref="IContentDraftBranchService" /> — named draft branches forked from a content
///     item's current default draft.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
internal sealed class ContentDraftBranchServiceTests : UmbracoIntegrationTestWithContent
{
    private IContentDraftBranchService ContentDraftBranchService => GetRequiredService<IContentDraftBranchService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    [Test]
    public async Task CreateAsync_Forks_From_Default_Draft_Without_Affecting_It()
    {
        // arrange: make an unpublished edit to the default draft so we have known, distinguishable property values
        Textpage.Name = "Textpage - edited draft";
        ContentService.Save(Textpage, -1);

        IContent originalDraft = ContentService.GetById(Textpage.Key)!;
        var originalVersionId = originalDraft.VersionId;

        // act
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> result =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);

        // assert: branch metadata is correct
        Assert.That(result.Success, Is.True);
        IContentDraftBranch branch = result.Result!;
        Assert.Multiple(() =>
        {
            Assert.That(branch.NodeId, Is.EqualTo(Textpage.Id));
            Assert.That(branch.Name, Is.EqualTo("Black Friday"));
            Assert.That(branch.BasedOnVersionId, Is.EqualTo(originalVersionId));
        });

        // assert: the default draft is completely untouched by branch creation
        IContent? defaultDraftAfter = ContentService.GetById(Textpage.Key);
        Assert.That(defaultDraftAfter, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(defaultDraftAfter!.VersionId, Is.EqualTo(originalVersionId));
            Assert.That(defaultDraftAfter.Name, Is.EqualTo("Textpage - edited draft"));
        });

        // assert: exactly one new, current, branch-scoped version row exists
        using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
        List<ContentVersionDto> branchVersions = scope.Database.Fetch<ContentVersionDto>(
            scope.Database.SqlContext.Sql()
                .Select<ContentVersionDto>()
                .From<ContentVersionDto>()
                .Where<ContentVersionDto>(x => x.NodeId == Textpage.Id && x.BranchId == branch.Id));

        Assert.That(branchVersions, Has.Count.EqualTo(1));
        ContentVersionDto branchVersion = branchVersions[0];
        Assert.Multiple(() =>
        {
            Assert.That(branchVersion.Current, Is.True);
            Assert.That(branchVersion.Id, Is.Not.EqualTo(originalVersionId));
        });

        // assert: the branch version's property data was cloned from the default draft
        List<PropertyDataDto> clonedProperties = scope.Database.Fetch<PropertyDataDto>(
            scope.Database.SqlContext.Sql()
                .Select<PropertyDataDto>()
                .From<PropertyDataDto>()
                .Where<PropertyDataDto>(x => x.VersionId == branchVersion.Id));

        List<PropertyDataDto> originalProperties = scope.Database.Fetch<PropertyDataDto>(
            scope.Database.SqlContext.Sql()
                .Select<PropertyDataDto>()
                .From<PropertyDataDto>()
                .Where<PropertyDataDto>(x => x.VersionId == originalVersionId));

        Assert.That(clonedProperties, Has.Count.EqualTo(originalProperties.Count));
    }

    [Test]
    public async Task CreateAsync_Returns_ContentNotFound_For_Unknown_Content()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> result =
            await ContentDraftBranchService.CreateAsync(Guid.NewGuid(), "Black Friday", Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentDraftBranchOperationStatus.ContentNotFound));
        });
    }

    [Test]
    public async Task GetByContentKey_Returns_All_Branches_For_The_Node()
    {
        await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        await ContentDraftBranchService.CreateAsync(Textpage.Key, "Summer Sale", Constants.Security.SuperUserKey);

        List<IContentDraftBranch> branches = ContentDraftBranchService.GetByContentKey(Textpage.Key).ToList();

        Assert.That(branches, Has.Count.EqualTo(2));
        Assert.That(branches.Select(b => b.Name), Is.EquivalentTo(new[] { "Black Friday", "Summer Sale" }));
    }

    [Test]
    public async Task Abandon_Deletes_The_Branch()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Summer Sale", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        Attempt<ContentDraftBranchOperationStatus> abandonResult = ContentDraftBranchService.Abandon(Textpage.Key, branch.Key);

        Assert.That(abandonResult.Success, Is.True);

        // abandoning is a hard, immediate delete - the branch row itself no longer exists
        IContentDraftBranch? reloaded = ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key);
        Assert.That(reloaded, Is.Null);
    }

    [Test]
    public async Task Abandon_Supersedes_The_Branchs_Own_Version_Row()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Summer Sale", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IContent branchContentBeforeAbandon = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key)!;
        var branchVersionId = branchContentBeforeAbandon.VersionId;

        Attempt<ContentDraftBranchOperationStatus> abandonResult = ContentDraftBranchService.Abandon(Textpage.Key, branch.Key);
        Assert.That(abandonResult.Success, Is.True);

        // the underlying version row is no longer "current" - it's ordinary history now, not a live draft
        using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
        ContentVersionDto abandonedVersion = scope.Database.Fetch<ContentVersionDto>(
            scope.Database.SqlContext.Sql()
                .Select<ContentVersionDto>()
                .From<ContentVersionDto>()
                .Where<ContentVersionDto>(x => x.Id == branchVersionId)).Single();
        Assert.That(abandonedVersion.Current, Is.False);

        // GetContent (which resolves via the branch's Current=true row) no longer finds anything
        IContent? branchContentAfterAbandon = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key);
        Assert.That(branchContentAfterAbandon, Is.Null);

        // the branch row itself is gone too - not merely superseded
        IContentDraftBranch? branchAfterAbandon = ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key);
        Assert.That(branchAfterAbandon, Is.Null);
    }

    [Test]
    public async Task Abandon_Returns_BranchNotFound_For_Already_Abandoned_Branch()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Summer Sale", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        ContentDraftBranchService.Abandon(Textpage.Key, branch.Key);

        // the first abandon already deleted the branch row, so a second attempt can no longer find it at all
        Attempt<ContentDraftBranchOperationStatus> secondAbandon = ContentDraftBranchService.Abandon(Textpage.Key, branch.Key);

        Assert.Multiple(() =>
        {
            Assert.That(secondAbandon.Success, Is.False);
            Assert.That(secondAbandon.Result, Is.EqualTo(ContentDraftBranchOperationStatus.BranchNotFound));
        });
    }

    [Test]
    public async Task PromoteAsync_Copies_The_Branchs_Content_Onto_Default_Draft_Even_If_It_Has_Since_Diverged()
    {
        // arrange: fork a branch capturing "Original", then diverge the default draft afterwards
        Textpage.Name = "Original";
        ContentService.Save(Textpage, -1);

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> siblingCreated =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Summer Sale", Constants.Security.SuperUserKey);
        IContentDraftBranch sibling = siblingCreated.Result!;

        Textpage.Name = "Diverged default draft edit";
        ContentService.Save(Textpage, -1);

        // act
        Attempt<ContentDraftBranchOperationStatus> promoteResult =
            await ContentDraftBranchService.PromoteAsync(Textpage.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);

        // assert: promotion succeeded and the branch's content (not the diverged default draft) now IS the
        // default draft - but promoting never publishes anything on its own
        Assert.That(promoteResult.Success, Is.True);

        IContent? defaultDraft = ContentService.GetById(Textpage.Key);
        Assert.That(defaultDraft, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(defaultDraft!.Name, Is.EqualTo("Original"));
            Assert.That(defaultDraft.Published, Is.False);
        });

        // assert: the promoted branch is gone (an immediate delete), and the sibling is untouched - promoting
        // a branch only ever affects that branch itself
        IContentDraftBranch? branchAfter = ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key);
        IContentDraftBranch? siblingAfter = ContentDraftBranchService.GetByKey(Textpage.Key, sibling.Key);
        Assert.Multiple(() =>
        {
            Assert.That(branchAfter, Is.Null);
            Assert.That(siblingAfter, Is.Not.Null);
        });
    }

    [Test]
    public async Task PromoteAsync_Leaves_Publish_State_Untouched_When_Default_Draft_Was_Already_Published()
    {
        // arrange: publish the default draft first, so there's a live, published version to preserve
        Textpage.Name = "Original";
        ContentService.SaveAndPublish(Textpage, [], Constants.Security.SuperUserId);

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IContent branchContent = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key)!;
        branchContent.Name = "Black Friday Edit";
        await ContentDraftBranchService.SaveAsync(Textpage.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        // act
        Attempt<ContentDraftBranchOperationStatus> promoteResult =
            await ContentDraftBranchService.PromoteAsync(Textpage.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);
        Assert.That(promoteResult.Success, Is.True);

        // assert: the draft now carries the branch's edit, but the live, published values are untouched -
        // promotion is never a publish
        IContent? defaultDraft = ContentService.GetById(Textpage.Key);
        Assert.Multiple(() =>
        {
            Assert.That(defaultDraft!.Name, Is.EqualTo("Black Friday Edit"));
            Assert.That(defaultDraft.Published, Is.True);
            Assert.That(defaultDraft.PublishName, Is.EqualTo("Original"));
        });
    }

    [Test]
    public async Task PromoteAsync_Returns_BranchNotFound_For_Already_Promoted_Branch()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        await ContentDraftBranchService.PromoteAsync(Textpage.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);
        Attempt<ContentDraftBranchOperationStatus> secondPromote =
            await ContentDraftBranchService.PromoteAsync(Textpage.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(secondPromote.Success, Is.False);
            Assert.That(secondPromote.Result, Is.EqualTo(ContentDraftBranchOperationStatus.BranchNotFound));
        });
    }

    [Test]
    public async Task PromoteAsync_Returns_BranchNotFound_For_Unknown_Branch()
    {
        Attempt<ContentDraftBranchOperationStatus> result =
            await ContentDraftBranchService.PromoteAsync(Textpage.Key, Guid.NewGuid(), new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.BranchNotFound));
        });
    }

    [Test]
    public async Task SaveAsync_Edits_Branch_Content_Without_Affecting_Default_Draft_Or_Siblings()
    {
        Textpage.Name = "Original";
        ContentService.Save(Textpage, -1);

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> siblingCreated =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Summer Sale", Constants.Security.SuperUserKey);
        IContentDraftBranch sibling = siblingCreated.Result!;

        IContent? branchContent = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key);
        Assert.That(branchContent, Is.Not.Null);
        Assert.That(branchContent!.Name, Is.EqualTo("Original"));

        // act: edit the branch's own draft
        branchContent.Name = "Black Friday Edit";
        Attempt<ContentDraftBranchOperationStatus> saveResult =
            await ContentDraftBranchService.SaveAsync(Textpage.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        // assert: the save succeeded and the branch reflects the edit
        Assert.That(saveResult.Success, Is.True);
        IContent? reloadedBranchContent = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key);
        Assert.That(reloadedBranchContent!.Name, Is.EqualTo("Black Friday Edit"));

        // assert: neither the default draft nor the sibling branch were touched
        IContent? defaultDraftAfter = ContentService.GetById(Textpage.Key);
        IContent? siblingContentAfter = ContentDraftBranchService.GetContent(Textpage.Key, sibling.Key);
        Assert.Multiple(() =>
        {
            Assert.That(defaultDraftAfter!.Name, Is.EqualTo("Original"));
            Assert.That(siblingContentAfter!.Name, Is.EqualTo("Original"));
        });
    }

    [Test]
    public async Task SaveAsync_Returns_BranchNotFound_For_Unknown_Branch()
    {
        Attempt<ContentDraftBranchOperationStatus> result =
            await ContentDraftBranchService.SaveAsync(Textpage.Key, Guid.NewGuid(), Textpage, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.BranchNotFound));
        });
    }

    [Test]
    public async Task SaveAsync_Returns_BranchNotFound_For_Already_Promoted_Branch()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IContent branchContent = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key)!;

        await ContentDraftBranchService.PromoteAsync(Textpage.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);

        branchContent.Name = "Too late";
        Attempt<ContentDraftBranchOperationStatus> saveResult =
            await ContentDraftBranchService.SaveAsync(Textpage.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(saveResult.Success, Is.False);
            Assert.That(saveResult.Result, Is.EqualTo(ContentDraftBranchOperationStatus.BranchNotFound));
        });
    }

    [Test]
    public async Task PromoteAsync_Copies_The_Branchs_Edited_Content_Not_Just_Its_Forked_Snapshot()
    {
        // arrange: fork a branch, then edit the branch itself (not the default draft) via SaveAsync
        Textpage.Name = "Original";
        ContentService.Save(Textpage, -1);

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        IContent branchContent = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key)!;
        branchContent.Name = "Black Friday Edit";
        await ContentDraftBranchService.SaveAsync(Textpage.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        // act
        Attempt<ContentDraftBranchOperationStatus> promoteResult =
            await ContentDraftBranchService.PromoteAsync(Textpage.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);

        // assert: the default draft reflects the branch's edit, not its state at fork time - and remains
        // unpublished, since promoting never publishes anything on its own
        Assert.That(promoteResult.Success, Is.True);
        IContent? defaultDraft = ContentService.GetById(Textpage.Key);
        Assert.Multiple(() =>
        {
            Assert.That(defaultDraft!.Published, Is.False);
            Assert.That(defaultDraft.Name, Is.EqualTo("Black Friday Edit"));
        });
    }

    [Test]
    public async Task GetVersions_Without_A_Branch_Key_Excludes_Branch_Scoped_Version_Rows()
    {
        IContent originalDraft = ContentService.GetById(Textpage.Key)!;
        var defaultDraftVersionId = originalDraft.VersionId;

        await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);

        List<IContent> defaultVersions = ContentDraftBranchService.GetVersions(Textpage.Key).ToList();

        Assert.That(defaultVersions.Select(v => v.VersionId), Is.EquivalentTo(new[] { defaultDraftVersionId }));
    }

    [Test]
    public async Task GetVersions_With_A_Branch_Key_Returns_Only_That_Branchs_Own_Version_Row()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IContent branchContent = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key)!;

        List<IContent> branchVersions = ContentDraftBranchService.GetVersions(Textpage.Key, branch.Key).ToList();

        Assert.That(branchVersions.Select(v => v.VersionId), Is.EquivalentTo(new[] { branchContent.VersionId }));
    }

    [Test]
    public async Task GetVersions_Returns_Empty_For_Unknown_Branch_Key()
    {
        await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);

        List<IContent> versions = ContentDraftBranchService.GetVersions(Textpage.Key, Guid.NewGuid()).ToList();

        Assert.That(versions, Is.Empty);
    }

    [Test]
    public void GetVersions_Returns_Empty_For_Unknown_Content()
    {
        List<IContent> versions = ContentDraftBranchService.GetVersions(Guid.NewGuid()).ToList();

        Assert.That(versions, Is.Empty);
    }

    [Test]
    public async Task GetVersionsSlim_Without_A_Branch_Key_Excludes_Branch_Scoped_Version_Rows()
    {
        IContent originalDraft = ContentService.GetById(Textpage.Key)!;
        var defaultDraftVersionId = originalDraft.VersionId;

        await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);

        List<IContent> defaultVersions = ContentDraftBranchService.GetVersionsSlim(Textpage.Key, 0, 10).ToList();

        Assert.That(defaultVersions.Select(v => v.VersionId), Is.EquivalentTo(new[] { defaultDraftVersionId }));
    }

    [Test]
    public async Task GetVersionsSlim_With_A_Branch_Key_Returns_Only_That_Branchs_Own_Version_Row()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IContent branchContent = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key)!;

        List<IContent> branchVersions = ContentDraftBranchService.GetVersionsSlim(Textpage.Key, 0, 10, branch.Key).ToList();

        Assert.That(branchVersions.Select(v => v.VersionId), Is.EquivalentTo(new[] { branchContent.VersionId }));
    }

    [Test]
    public async Task Schedule_Sets_And_Clears_The_Scheduled_Publish_Date()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        var publishDate = new DateTime(2030, 11, 29, 0, 0, 0, DateTimeKind.Utc);

        Attempt<ContentDraftBranchOperationStatus> scheduleResult = ContentDraftBranchService.Schedule(Textpage.Key, branch.Key, publishDate, new HashSet<string>());
        Assert.That(scheduleResult.Success, Is.True);

        IContentDraftBranch? scheduled = ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key);
        Assert.That(scheduled!.ScheduledPublishDate, Is.EqualTo(publishDate));

        Attempt<ContentDraftBranchOperationStatus> clearResult = ContentDraftBranchService.Schedule(Textpage.Key, branch.Key, null, new HashSet<string>());
        Assert.That(clearResult.Success, Is.True);

        IContentDraftBranch? cleared = ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key);
        Assert.That(cleared!.ScheduledPublishDate, Is.Null);
    }

    [Test]
    public void Schedule_Returns_BranchNotFound_For_Unknown_Branch()
    {
        Attempt<ContentDraftBranchOperationStatus> result =
            ContentDraftBranchService.Schedule(Textpage.Key, Guid.NewGuid(), DateTime.UtcNow.AddDays(1), new HashSet<string>());

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.BranchNotFound));
        });
    }

    [Test]
    public async Task Schedule_Returns_BranchNotFound_For_Already_Promoted_Branch()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        await ContentDraftBranchService.PromoteAsync(Textpage.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);

        Attempt<ContentDraftBranchOperationStatus> result =
            ContentDraftBranchService.Schedule(Textpage.Key, branch.Key, DateTime.UtcNow.AddDays(1), new HashSet<string>());

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.BranchNotFound));
        });
    }

    [Test]
    public async Task PromoteAsync_Then_Explicit_Publish_Applies_The_Branchs_Edits_Across_Multiple_Non_Default_Cultures()
    {
        // every prior branch test that used culture-variant content only ever exercised a single culture
        // ("en-US", which is also the install's default/invariant language) - nothing has proven that a
        // branch correctly carries edits across MULTIPLE simultaneously-variant cultures through fork,
        // per-culture edit, save, promote, and (separately) publish.
        await LanguageService.CreateAsync(new Language("da-DK", "Danish") { FallbackIsoCode = "en-US" }, Constants.Security.SuperUserKey);

        IContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMultiCulturePage", "Multi Culture Page", defaultTemplateId: ContentType.DefaultTemplateId);
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

        // edit both cultures on the branch's own draft
        IContent branchContent = ContentDraftBranchService.GetContent(content.Key, branch.Key)!;
        branchContent.SetCultureName("Black Friday EN", "en-US");
        branchContent.SetCultureName("Black Friday DA", "da-DK");
        Attempt<ContentDraftBranchOperationStatus> saveResult =
            await ContentDraftBranchService.SaveAsync(content.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);
        Assert.That(saveResult.Success, Is.True);

        // act: promote the branch - this only copies the edits onto the default draft, it does not publish them
        Attempt<ContentDraftBranchOperationStatus> promoteResult =
            await ContentDraftBranchService.PromoteAsync(content.Key, branch.Key, new HashSet<string> { "en-US", "da-DK" }, Constants.Security.SuperUserKey);
        Assert.That(promoteResult.Success, Is.True);

        IContent? promoted = ContentService.GetById(content.Key);
        Assert.Multiple(() =>
        {
            Assert.That(promoted!.GetCultureName("en-US"), Is.EqualTo("Black Friday EN"));
            Assert.That(promoted.GetPublishName("en-US"), Is.EqualTo("Original EN"));
            Assert.That(promoted.GetCultureName("da-DK"), Is.EqualTo("Black Friday DA"));
            Assert.That(promoted.GetPublishName("da-DK"), Is.EqualTo("Original DA"));
        });

        // act: only now, via an ordinary editorial publish through the content service, do the edits go live
        PublishResult publishResult = ContentService.SaveAndPublish(promoted!, ["en-US", "da-DK"], Constants.Security.SuperUserId);
        Assert.That(publishResult.Success, Is.True);

        IContent? published = ContentService.GetById(content.Key);
        Assert.Multiple(() =>
        {
            Assert.That(published!.Published, Is.True);
            Assert.That(published.GetCultureName("en-US"), Is.EqualTo("Black Friday EN"));
            Assert.That(published.GetPublishName("en-US"), Is.EqualTo("Black Friday EN"));
            Assert.That(published.GetCultureName("da-DK"), Is.EqualTo("Black Friday DA"));
            Assert.That(published.GetPublishName("da-DK"), Is.EqualTo("Black Friday DA"));
        });
    }

    [Test]
    public async Task PromoteAsync_Copies_Only_The_Requested_Cultures()
    {
        IContent content = await CreateMixedVarianceContentAsync();

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(content.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        IContent branchContent = ContentDraftBranchService.GetContent(content.Key, branch.Key)!;
        branchContent.SetCultureName("Black Friday EN", "en-US");
        branchContent.SetCultureName("Black Friday DA", "da-DK");
        await ContentDraftBranchService.SaveAsync(content.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        // act: promote only en-US
        Attempt<ContentDraftBranchOperationStatus> promoteResult =
            await ContentDraftBranchService.PromoteAsync(content.Key, branch.Key, new HashSet<string> { "en-US" }, Constants.Security.SuperUserKey);
        Assert.That(promoteResult.Success, Is.True);

        IContent? defaultDraft = ContentService.GetById(content.Key);
        Assert.Multiple(() =>
        {
            Assert.That(defaultDraft!.GetCultureName("en-US"), Is.EqualTo("Black Friday EN"));

            // da-DK was never included in the promote - it must remain exactly as the default draft had it
            Assert.That(defaultDraft.GetCultureName("da-DK"), Is.EqualTo("Original DA"));
        });
    }

    [Test]
    public async Task PromoteAsync_Returns_CultureMissing_For_Empty_Cultures_On_Variant_Content()
    {
        IContent content = await CreateMixedVarianceContentAsync();
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(content.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        Attempt<ContentDraftBranchOperationStatus> result =
            await ContentDraftBranchService.PromoteAsync(content.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.CultureMissing));
        });

        // the branch is preserved - a failed attempt must not delete it
        Assert.That(ContentDraftBranchService.GetByKey(content.Key, branch.Key), Is.Not.Null);
    }

    [Test]
    public async Task PromoteAsync_Returns_InvalidCulture_For_Unknown_Culture()
    {
        IContent content = await CreateMixedVarianceContentAsync();
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(content.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        Attempt<ContentDraftBranchOperationStatus> result = await ContentDraftBranchService.PromoteAsync(
            content.Key,
            branch.Key,
            new HashSet<string> { "fr-FR" },
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.InvalidCulture));
        });
        Assert.That(ContentDraftBranchService.GetByKey(content.Key, branch.Key), Is.Not.Null);
    }

    [Test]
    public async Task PromoteAsync_Returns_InvalidCulture_When_Cultures_Supplied_For_Invariant_Content()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        Attempt<ContentDraftBranchOperationStatus> result = await ContentDraftBranchService.PromoteAsync(
            Textpage.Key,
            branch.Key,
            new HashSet<string> { "en-US" },
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.InvalidCulture));
        });
    }

    [Test]
    public async Task PromoteAsync_Still_Deletes_The_Whole_Branch_Even_On_Partial_Culture_Promote()
    {
        // this locks in intentional, documented behavior: promoting only a subset of a branch's cultures does
        // NOT preserve the branch for its remaining cultures - the whole branch row is gone afterward, and any
        // content for cultures that weren't included is permanently lost.
        IContent content = await CreateMixedVarianceContentAsync();
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(content.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        Attempt<ContentDraftBranchOperationStatus> promoteResult =
            await ContentDraftBranchService.PromoteAsync(content.Key, branch.Key, new HashSet<string> { "en-US" }, Constants.Security.SuperUserKey);
        Assert.That(promoteResult.Success, Is.True);

        Assert.That(ContentDraftBranchService.GetByKey(content.Key, branch.Key), Is.Null);
    }

    [Test]
    public async Task PromoteAsync_Includes_Invariant_Properties_When_User_Has_Default_Language_Access()
    {
        IContent content = await CreateMixedVarianceContentAsync();
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(content.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        IContent branchContent = ContentDraftBranchService.GetContent(content.Key, branch.Key)!;
        branchContent.SetValue("author", "Black Friday Author");
        await ContentDraftBranchService.SaveAsync(content.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        // the super user has access to every language, including the default (en-US)
        Attempt<ContentDraftBranchOperationStatus> promoteResult =
            await ContentDraftBranchService.PromoteAsync(content.Key, branch.Key, new HashSet<string> { "en-US" }, Constants.Security.SuperUserKey);
        Assert.That(promoteResult.Success, Is.True);

        IContent? defaultDraft = ContentService.GetById(content.Key);
        Assert.That(defaultDraft!.GetValue<string>("author"), Is.EqualTo("Black Friday Author"));
    }

    [Test]
    public async Task PromoteAsync_Excludes_Invariant_Properties_When_User_Lacks_Default_Language_Access_And_Flag_Disabled()
    {
        IContent content = await CreateMixedVarianceContentAsync();
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(content.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        IContent branchContent = ContentDraftBranchService.GetContent(content.Key, branch.Key)!;
        branchContent.SetValue("author", "Black Friday Author");
        await ContentDraftBranchService.SaveAsync(content.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        IUser restrictedUser = await CreateUserRestrictedToCultureAsync("da-DK");

        Attempt<ContentDraftBranchOperationStatus> promoteResult =
            await ContentDraftBranchService.PromoteAsync(content.Key, branch.Key, new HashSet<string> { "da-DK" }, restrictedUser.Key);
        Assert.That(promoteResult.Success, Is.True);

        // the variant culture the user IS allowed to touch was still promoted...
        IContent? defaultDraft = ContentService.GetById(content.Key);
        Assert.Multiple(() =>
        {
            Assert.That(defaultDraft!.GetCultureName("da-DK"), Is.EqualTo("Original DA"));

            // ...but the invariant property was left as it was - the user has no access to the default
            // language, and AllowEditInvariantFromNonDefault defaults to false.
            Assert.That(defaultDraft.GetValue<string>("author"), Is.EqualTo("Original Author"));
        });
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowEditInvariantFromNonDefaultTrue))]
    public async Task PromoteAsync_Includes_Invariant_Properties_When_Flag_Enabled_Despite_Lacking_Default_Language_Access()
    {
        IContent content = await CreateMixedVarianceContentAsync();
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(content.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        IContent branchContent = ContentDraftBranchService.GetContent(content.Key, branch.Key)!;
        branchContent.SetValue("author", "Black Friday Author");
        await ContentDraftBranchService.SaveAsync(content.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        IUser restrictedUser = await CreateUserRestrictedToCultureAsync("da-DK");

        Attempt<ContentDraftBranchOperationStatus> promoteResult =
            await ContentDraftBranchService.PromoteAsync(content.Key, branch.Key, new HashSet<string> { "da-DK" }, restrictedUser.Key);
        Assert.That(promoteResult.Success, Is.True);

        IContent? defaultDraft = ContentService.GetById(content.Key);
        Assert.That(defaultDraft!.GetValue<string>("author"), Is.EqualTo("Black Friday Author"));
    }

    public static void ConfigureAllowEditInvariantFromNonDefaultTrue(IUmbracoBuilder builder)
        => builder.Services.Configure<ContentSettings>(config => config.AllowEditInvariantFromNonDefault = true);

    [Test]
    public async Task Schedule_Persists_Requested_Cultures()
    {
        IContent content = await CreateMixedVarianceContentAsync();
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(content.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        var publishDate = new DateTime(2030, 11, 29, 0, 0, 0, DateTimeKind.Utc);

        Attempt<ContentDraftBranchOperationStatus> scheduleResult =
            ContentDraftBranchService.Schedule(content.Key, branch.Key, publishDate, new HashSet<string> { "en-US" });
        Assert.That(scheduleResult.Success, Is.True);

        IContentDraftBranch? reloaded = ContentDraftBranchService.GetByKey(content.Key, branch.Key);
        Assert.That(reloaded!.ScheduledCultures, Is.EquivalentTo(new[] { "en-US" }));
    }

    [Test]
    public async Task Schedule_Returns_CultureMissing_For_Empty_Cultures_On_Variant_Content()
    {
        IContent content = await CreateMixedVarianceContentAsync();
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(content.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        Attempt<ContentDraftBranchOperationStatus> result =
            ContentDraftBranchService.Schedule(content.Key, branch.Key, DateTime.UtcNow.AddDays(1), new HashSet<string>());

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.CultureMissing));
        });
    }

    [Test]
    public async Task Schedule_Returns_InvalidCulture_For_Unknown_Culture()
    {
        IContent content = await CreateMixedVarianceContentAsync();
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(content.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;

        Attempt<ContentDraftBranchOperationStatus> result = ContentDraftBranchService.Schedule(
            content.Key,
            branch.Key,
            DateTime.UtcNow.AddDays(1),
            new HashSet<string> { "fr-FR" });

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.InvalidCulture));
        });
    }

    // Every branch-scoped operation validates that a branch actually belongs to the content item it's addressed
    // through - a branch key paired with the WRONG (but real, existing) content key must behave exactly as if
    // the branch didn't exist at all, and must never leak or mutate the branch's real data. This is enforced by
    // the service itself (GetValidatedBranch), independent of any API-layer check.

    [Test]
    public async Task GetByKey_Returns_Null_When_Branch_Belongs_To_A_Different_Content_Item()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IContent unrelatedContent = CreateUnrelatedContent();

        IContentDraftBranch? result = ContentDraftBranchService.GetByKey(unrelatedContent.Key, branch.Key);

        Assert.That(result, Is.Null);

        // the branch is untouched - this is purely a scoping mismatch, not a real lookup failure
        Assert.That(ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key), Is.Not.Null);
    }

    [Test]
    public async Task GetContent_Returns_Null_When_Branch_Belongs_To_A_Different_Content_Item()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IContent unrelatedContent = CreateUnrelatedContent();

        IContent? result = ContentDraftBranchService.GetContent(unrelatedContent.Key, branch.Key);

        Assert.That(result, Is.Null);
        Assert.That(ContentDraftBranchService.GetContent(Textpage.Key, branch.Key), Is.Not.Null);
    }

    [Test]
    public async Task SaveAsync_Returns_BranchNotFound_When_Branch_Belongs_To_A_Different_Content_Item()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IContent unrelatedContent = CreateUnrelatedContent();

        IContent branchContent = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key)!;
        var originalName = branchContent.Name;
        branchContent.Name = "Should not be saved";

        Attempt<ContentDraftBranchOperationStatus> result =
            await ContentDraftBranchService.SaveAsync(unrelatedContent.Key, branch.Key, branchContent, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.BranchNotFound));
        });

        // the branch's real content was never touched by the rejected save
        IContent? branchContentAfter = ContentDraftBranchService.GetContent(Textpage.Key, branch.Key);
        Assert.That(branchContentAfter!.Name, Is.EqualTo(originalName));
    }

    [Test]
    public async Task Abandon_Returns_BranchNotFound_When_Branch_Belongs_To_A_Different_Content_Item()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IContent unrelatedContent = CreateUnrelatedContent();

        Attempt<ContentDraftBranchOperationStatus> result = ContentDraftBranchService.Abandon(unrelatedContent.Key, branch.Key);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.BranchNotFound));
        });

        // the branch was NOT deleted by the mismatched-content attempt
        Assert.That(ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key), Is.Not.Null);
    }

    [Test]
    public async Task PromoteAsync_Returns_BranchNotFound_When_Branch_Belongs_To_A_Different_Content_Item()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IContent unrelatedContent = CreateUnrelatedContent();

        Attempt<ContentDraftBranchOperationStatus> result =
            await ContentDraftBranchService.PromoteAsync(unrelatedContent.Key, branch.Key, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.BranchNotFound));
        });

        // the branch was NOT promoted (and thus not deleted) by the mismatched-content attempt
        Assert.That(ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key), Is.Not.Null);
    }

    [Test]
    public async Task Schedule_Returns_BranchNotFound_When_Branch_Belongs_To_A_Different_Content_Item()
    {
        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> created =
            await ContentDraftBranchService.CreateAsync(Textpage.Key, "Black Friday", Constants.Security.SuperUserKey);
        IContentDraftBranch branch = created.Result!;
        IContent unrelatedContent = CreateUnrelatedContent();

        Attempt<ContentDraftBranchOperationStatus> result =
            ContentDraftBranchService.Schedule(unrelatedContent.Key, branch.Key, DateTime.UtcNow.AddDays(1), new HashSet<string>());

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDraftBranchOperationStatus.BranchNotFound));
        });

        // the branch's schedule was NOT set by the mismatched-content attempt
        Assert.That(ContentDraftBranchService.GetByKey(Textpage.Key, branch.Key)!.ScheduledPublishDate, Is.Null);
    }

    /// <summary>
    /// Creates a second, real content item unrelated to <see cref="Textpage"/> - used to prove that resolving a
    /// branch by key is scoped to the content item it actually belongs to, not just "some content item that
    /// happens to exist."
    /// </summary>
    private IContent CreateUnrelatedContent()
    {
        IContent content = ContentService.Create("Unrelated Page", -1, Textpage.ContentType.Alias);
        ContentService.Save(content);
        return content;
    }

    /// <summary>
    /// Creates and publishes a content item whose type has a mix of culture-variant properties ("title",
    /// "bodyText") and an invariant property ("author") - the flexible-variance shape needed to exercise the
    /// promote/schedule culture-selection and invariant-inclusion rules together.
    /// </summary>
    private async Task<IContent> CreateMixedVarianceContentAsync()
    {
        if (await LanguageService.GetAsync("da-DK") is null)
        {
            await LanguageService.CreateAsync(new Language("da-DK", "Danish") { FallbackIsoCode = "en-US" }, Constants.Security.SuperUserKey);
        }

        IContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMixedVariancePage", "Mixed Variance Page", defaultTemplateId: ContentType.DefaultTemplateId);
        contentType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in contentType.PropertyTypes)
        {
            propertyType.Variations = propertyType.Alias == "author" ? ContentVariation.Nothing : ContentVariation.Culture;
        }

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // CreateSimpleContent's WithPropertyValues convenience overload sets every property with the same
        // single culture, which fails here since "author" is invariant - build and set values manually instead.
        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Original EN")
            .Build();
        content.SetCultureName("Original EN", "en-US");
        content.SetCultureName("Original DA", "da-DK");
        content.SetValue("author", "Original Author");
        ContentService.Save(content);
        ContentService.Publish(content, ["en-US", "da-DK"]);

        return content;
    }

    /// <summary>
    /// Creates a user, in a user group with no access to all languages, restricted to exactly the given culture -
    /// used to exercise the invariant-inclusion rule's "user lacks default language access" branch.
    /// </summary>
    private async Task<IUser> CreateUserRestrictedToCultureAsync(string isoCode)
    {
        ILanguage language = (await LanguageService.GetAsync(isoCode))!;

        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Restricted Language Group",
            Alias = "restrictedLanguageGroup" + Guid.NewGuid().ToString("N"),
            HasAccessToAllLanguages = false,
        };
        userGroup.AddAllowedLanguage(language.Id);
        Attempt<IUserGroup, UserGroupOperationStatus> groupCreated = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.That(groupCreated.Success, Is.True);

        IUser user = UserService.CreateUserWithIdentity("restricteduser" + Guid.NewGuid().ToString("N"), $"{Guid.NewGuid():N}@test.com");
        user.AddGroup(groupCreated.Result.ToReadOnlyGroup());
        UserService.Save(user);

        return user;
    }
}
