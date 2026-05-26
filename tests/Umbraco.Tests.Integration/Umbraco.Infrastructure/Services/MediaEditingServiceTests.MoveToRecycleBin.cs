using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

internal sealed partial class MediaEditingServiceTests
{
    public static void ConfigureDisableDeleteWhenReferencedTrue(IUmbracoBuilder builder)
        => builder.Services.Configure<ContentSettings>(config =>
            config.DisableDeleteWhenReferenced = true);

    private async Task<IMedia> CreateFolderMediaAsync(string name)
    {
        var folderMediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.Folder);
        var createModel = new MediaCreateModel
        {
            ContentTypeKey = folderMediaType!.Key,
            ParentKey = Constants.System.RootKey,
            Key = Guid.NewGuid(),
            Variants = [new VariantModel { Name = name }],
        };

        var result = await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    private async Task RelateAsync(IMedia parent, IMedia child)
    {
        var relationType = await RelationService.GetRelationTypeByAliasAsync(Constants.Conventions.RelationTypes.RelatedMediaAlias);
        await RelationService.RelateAsync(parent.Id, child.Id, relationType!);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisableDeleteWhenReferencedTrue))]
    public async Task Cannot_Move_To_Recycle_Bin_When_Media_Is_Referenced_And_DisableDeleteWhenReferenced_Is_True()
    {
        // Setup a relation where the media being trashed is the child (i.e. another media item references it).
        var referencer = await CreateFolderMediaAsync("Referencer");
        var mediaToTrash = await CreateFolderMediaAsync("Media To Trash");
        await RelateAsync(referencer, mediaToTrash);

        var moveAttempt = await MediaEditingService.MoveToRecycleBinAsync(mediaToTrash.Key, Constants.Security.SuperUserKey);
        Assert.IsFalse(moveAttempt.Success);
        Assert.AreEqual(ContentEditingOperationStatus.CannotMoveToRecycleBinWhenReferenced, moveAttempt.Status);

        // Verify the item was not moved.
        var media = await MediaEditingService.GetAsync(mediaToTrash.Key);
        Assert.IsNotNull(media);
        Assert.IsFalse(media.Trashed);
    }

    // Regression test for https://github.com/umbraco/Umbraco-CMS/issues/22661.
    // The "Media deleted" entry in the History panel is written by RelateOnTrashNotificationHandler,
    // which falls back to the entity's WriterId when no back-office user can be resolved. Before the
    // fix, MediaService.PerformMoveMediaLocked never updated WriterId on a trash, so the fallback
    // attributed the deletion to the original creator. The fix updates WriterId to the acting user,
    // mirroring ContentService.PerformMoveContentLocked.
    [Test]
    public async Task Move_To_Recycle_Bin_Updates_WriterId_To_The_Trashing_User()
    {
        var creator = await CreateAdminUserAsync("creator");
        var deleter = await CreateAdminUserAsync("deleter");

        var media = await CreateFolderMediaAsync("Media To Trash", creator.Key);
        Assert.AreEqual(creator.Id, media.CreatorId);
        Assert.AreEqual(creator.Id, media.WriterId);

        var trashAttempt = await MediaEditingService.MoveToRecycleBinAsync(media.Key, deleter.Key);
        Assert.IsTrue(trashAttempt.Success);

        var trashedMedia = await MediaEditingService.GetAsync(media.Key);
        Assert.IsNotNull(trashedMedia);
        Assert.IsTrue(trashedMedia.Trashed);
        Assert.AreEqual(creator.Id, trashedMedia.CreatorId, "CreatorId must not change on trash.");
        Assert.AreEqual(deleter.Id, trashedMedia.WriterId, "WriterId must reflect the user who trashed the item.");
    }

    [Test]
    public async Task Move_To_Recycle_Bin_Records_AuditType_Move_Against_The_Trashing_User()
    {
        var deleter = await CreateAdminUserAsync("deleter");
        var media = await CreateFolderMediaAsync("Media To Trash");

        var trashAttempt = await MediaEditingService.MoveToRecycleBinAsync(media.Key, deleter.Key);
        Assert.IsTrue(trashAttempt.Success);

        var moveEntries = await AuditService.GetItemsByEntityAsync(
            media.Id,
            skip: 0,
            take: 100,
            auditTypeFilter: [AuditType.Move]);

        Assert.AreEqual(1, moveEntries.Items.Count(), "Expected one AuditType.Move entry for the trash operation.");
        Assert.AreEqual(deleter.Id, moveEntries.Items.Single().UserId);
    }
}
