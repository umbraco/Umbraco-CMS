using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

internal sealed partial class MediaEditingServiceTests
{
    public static void ConfigureDisableDeleteWhenReferencedTrue(IUmbracoBuilder builder)
        => builder.Services.Configure<ContentSettings>(config =>
            config.DisableDeleteWhenReferenced = true);

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisableDeleteWhenReferencedTrue))]
    public async Task Cannot_Move_To_Recycle_Bin_When_Media_Is_Referenced_And_DisableDeleteWhenReferenced_Is_True()
    {
        // Setup a relation where the media being trashed is the child (i.e. another media item references it).
        var referencer = await CreateFolderMediaAsync("Referencer");
        var mediaToTrash = await CreateFolderMediaAsync("Media To Trash");
        Relate(referencer, mediaToTrash);

        var moveAttempt = await MediaEditingService.MoveToRecycleBinAsync(mediaToTrash.Key, Constants.Security.SuperUserKey);
        Assert.That(moveAttempt.Success, Is.False);
        Assert.That(moveAttempt.Status, Is.EqualTo(ContentEditingOperationStatus.CannotMoveToRecycleBinWhenReferenced));

        // Verify the item was not moved.
        var media = await MediaEditingService.GetAsync(mediaToTrash.Key);
        Assert.That(media, Is.Not.Null);
        Assert.That(media.Trashed, Is.False);
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
        Assert.That(media.CreatorId, Is.EqualTo(creator.Id));
        Assert.That(media.WriterId, Is.EqualTo(creator.Id));

        var trashAttempt = await MediaEditingService.MoveToRecycleBinAsync(media.Key, deleter.Key);
        Assert.That(trashAttempt.Success, Is.True);

        var trashedMedia = await MediaEditingService.GetAsync(media.Key);
        Assert.That(trashedMedia, Is.Not.Null);
        Assert.That(trashedMedia.Trashed, Is.True);
        Assert.That(trashedMedia.CreatorId, Is.EqualTo(creator.Id), "CreatorId must not change on trash.");
        Assert.That(trashedMedia.WriterId, Is.EqualTo(deleter.Id), "WriterId must reflect the user who trashed the item.");
    }

    [Test]
    public async Task Move_To_Recycle_Bin_Records_AuditType_Move_Against_The_Trashing_User()
    {
        var deleter = await CreateAdminUserAsync("deleter");
        var media = await CreateFolderMediaAsync("Media To Trash");

        var trashAttempt = await MediaEditingService.MoveToRecycleBinAsync(media.Key, deleter.Key);
        Assert.That(trashAttempt.Success, Is.True);

        var moveEntries = await AuditService.GetItemsByEntityAsync(
            media.Id,
            skip: 0,
            take: 100,
            auditTypeFilter: [AuditType.Move]);

        Assert.That(moveEntries.Items.Count(), Is.EqualTo(1), "Expected one AuditType.Move entry for the trash operation.");
        Assert.That(moveEntries.Items.Single().UserId, Is.EqualTo(deleter.Id));
    }
}
