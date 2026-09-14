using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    public static void ConfigureDisableUnpublishWhenReferencedTrue(IUmbracoBuilder builder)
        => builder.Services.Configure<ContentSettings>(config =>
            config.DisableUnpublishWhenReferenced = true);

    public static void ConfigureDisableDeleteWhenReferencedTrue(IUmbracoBuilder builder)
        => builder.Services.Configure<ContentSettings>(config =>
            config.DisableDeleteWhenReferenced = true);

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Move_To_Recycle_Bin(bool variant)
    {
        var content = await (variant ? CreateCultureVariantContent() : CreateInvariantContent());
        var result = await ContentEditingService.MoveToRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        // re-get and verify move
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.That(content, Is.Not.Null);
        Assert.That(content.Trashed, Is.True);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisableDeleteWhenReferencedTrue))]
    public async Task Cannot_Move_To_Recycle_Bin_When_Content_Is_Related_As_A_Child_And_DisableDeleteWhenReferenced_Is_True()
    {
        // Setup a relation where the page being deleted is related to another page as a child (e.g. the other page has a picker and has selected this page).
        Relate(Subpage2, Subpage);
        var moveAttempt = await ContentEditingService.MoveToRecycleBinAsync(Subpage.Key, Constants.Security.SuperUserKey);
        Assert.That(moveAttempt.Success, Is.False);
        Assert.That(moveAttempt.Status, Is.EqualTo(ContentEditingOperationStatus.CannotMoveToRecycleBinWhenReferenced));

        // re-get and verify not moved
        var content = await ContentEditingService.GetAsync(Subpage.Key);
        Assert.That(content, Is.Not.Null);
        Assert.That(content.Trashed, Is.False);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisableUnpublishWhenReferencedTrue))]
    public async Task Can_Move_To_Recycle_Bin_When_Content_Is_Related_And_DisableUnpublishWhenReferenced_Is_True()
    {
        // DisableUnpublishWhenReferenced should NOT block trashing — only unpublishing.
        Relate(Subpage2, Subpage);
        var moveAttempt = await ContentEditingService.MoveToRecycleBinAsync(Subpage.Key, Constants.Security.SuperUserKey);
        Assert.That(moveAttempt.Success, Is.True);
        Assert.That(moveAttempt.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        // re-get and verify moved
        var content = await ContentEditingService.GetAsync(Subpage.Key);
        Assert.That(content, Is.Not.Null);
        Assert.That(content.Trashed, Is.True);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisableDeleteWhenReferencedTrue))]
    public async Task Can_Move_To_Recycle_Bin_When_Content_Is_Related_As_A_Parent_And_Configured_To_Disable_When_Related()
    {
        // Setup a relation where the page being deleted is related to another page as a child (e.g. the other page has a picker and has selected this page).
        Relate(Subpage, Subpage2);
        var moveAttempt = await ContentEditingService.MoveToRecycleBinAsync(Subpage.Key, Constants.Security.SuperUserKey);
        Assert.That(moveAttempt.Success, Is.True);
        Assert.That(moveAttempt.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        // re-get and verify moved
        var content = await ContentEditingService.GetAsync(Subpage.Key);
        Assert.That(content, Is.Not.Null);
        Assert.That(content.Trashed, Is.True);
    }

    [Test]
    public async Task Cannot_Move_Non_Existing_To_Recycle_Bin()
    {
        var result = await ContentEditingService.MoveToRecycleBinAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotFound));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Cannot_Move_To_Recycle_Bin_If_Already_In_Recycle_Bin(bool variant)
    {
        var content = await (variant ? CreateCultureVariantContent() : CreateInvariantContent());
        await ContentEditingService.MoveToRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);
        var result = await ContentEditingService.MoveToRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.InTrash));

        // re-get and verify that it still is in the recycle bin
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.That(content, Is.Not.Null);
        Assert.That(content.Trashed, Is.True);
    }

    // Companion to the media regression test for https://github.com/umbraco/Umbraco-CMS/issues/22661.
    // ContentService already updated WriterId on trash; this guards against future regressions.
    [Test]
    public async Task Move_To_Recycle_Bin_Updates_WriterId_To_The_Trashing_User()
    {
        var creator = await CreateAdminUserAsync("creator");
        var deleter = await CreateAdminUserAsync("deleter");

        var contentType = await CreateInvariantContentType();
        var createResult = await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = Constants.System.RootKey,
                Variants = [new VariantModel { Name = "Document To Trash" }],
                Properties =
                [
                    new PropertyValueModel { Alias = "title", Value = "The title" },
                    new PropertyValueModel { Alias = "text", Value = "The text" }
                ],
            },
            creator.Key);
        Assert.That(createResult.Success, Is.True);
        var content = createResult.Result.Content!;
        Assert.That(content.CreatorId, Is.EqualTo(creator.Id));
        Assert.That(content.WriterId, Is.EqualTo(creator.Id));

        var trashAttempt = await ContentEditingService.MoveToRecycleBinAsync(content.Key, deleter.Key);
        Assert.That(trashAttempt.Success, Is.True);

        var trashedContent = await ContentEditingService.GetAsync(content.Key);
        Assert.That(trashedContent, Is.Not.Null);
        Assert.That(trashedContent.Trashed, Is.True);
        Assert.That(trashedContent.CreatorId, Is.EqualTo(creator.Id), "CreatorId must not change on trash.");
        Assert.That(trashedContent.WriterId, Is.EqualTo(deleter.Id), "WriterId must reflect the user who trashed the item.");
    }

    [Test]
    public async Task Move_To_Recycle_Bin_Records_AuditType_Move_Against_The_Trashing_User()
    {
        var auditService = GetRequiredService<IAuditService>();
        var deleter = await CreateAdminUserAsync("deleter");
        var content = await CreateInvariantContent();

        var trashAttempt = await ContentEditingService.MoveToRecycleBinAsync(content.Key, deleter.Key);
        Assert.That(trashAttempt.Success, Is.True);

        var moveEntries = await auditService.GetItemsByEntityAsync(
            content.Id,
            skip: 0,
            take: 100,
            auditTypeFilter: [AuditType.Move]);

        Assert.That(moveEntries.Items.Count(), Is.EqualTo(1), "Expected one AuditType.Move entry for the trash operation.");
        Assert.That(moveEntries.Items.Single().UserId, Is.EqualTo(deleter.Id));
    }

    private async Task<IUser> CreateAdminUserAsync(string identifier)
    {
        var adminGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var createModel = new UserCreateModel
        {
            UserName = $"{identifier}@example.com",
            Email = $"{identifier}@example.com",
            Name = identifier,
            UserGroupKeys = new HashSet<Guid> { adminGroup!.Key },
        };

        var result = await UserService.CreateAsync(Constants.Security.SuperUserKey, createModel);
        Assert.That(result.Success, Is.True);
        return result.Result.CreatedUser!;
    }
}
