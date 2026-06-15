using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaHybridCacheTests : UmbracoIntegrationTestWithMediaEditing
{
    private IPublishedMediaCache PublishedMediaHybridCache => GetRequiredService<IPublishedMediaCache>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<MediaTreeChangeNotification, MediaTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    // Media with crops
    [Test]
    public async Task Can_Get_Root_Media_By_Id()
    {
        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(RootFolderId);

        // Assert
        Assert.That(media, Is.Not.Null);
        Assert.That(media.Name, Is.EqualTo("RootFolder"));
        Assert.That(media.ContentType.Key, Is.EqualTo(RootFolder.ContentTypeKey));
    }

    [Test]
    public async Task Can_Get_Root_Media_By_Key()
    {
        var media = await PublishedMediaHybridCache.GetByIdAsync(RootFolder.Key.Value);

        // Assert
        Assert.That(media, Is.Not.Null);
        Assert.That(media.Name, Is.EqualTo("RootFolder"));
        Assert.That(media.ContentType.Key, Is.EqualTo(RootFolder.ContentTypeKey));
    }

    [Test]
    public async Task Can_Get_Child_Media_By_Id()
    {
        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImageId);

        // Assert
        Assert.That(media, Is.Not.Null);
        Assert.That(media.Name, Is.EqualTo("SubImage"));
        Assert.That(media.ContentType.Key, Is.EqualTo(SubImage.ContentTypeKey));
        Assert.That(RootFolder.Key, Is.EqualTo(SubImage.ParentKey));
    }


    [Test]
    public async Task Can_Get_Child_Media_By_Key()
    {
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImage.Key.Value);

        // Assert
        Assert.That(media, Is.Not.Null);
        Assert.That(media.Name, Is.EqualTo("SubImage"));
        Assert.That(media.ContentType.Key, Is.EqualTo(SubImage.ContentTypeKey));
        Assert.That(RootFolder.Key, Is.EqualTo(SubImage.ParentKey));
    }

    [Test]
    public async Task Cannot_Get_Non_Existing_Media_By_Id()
    {
        // Act
        const int nonExistingId = 124214;
        var media = await PublishedMediaHybridCache.GetByIdAsync(nonExistingId);

        // Assert
        Assert.That(media, Is.Null);
    }

    [Test]
    public async Task Cannot_Get_Non_Existing_Media_By_Key()
    {
        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(media, Is.Null);
    }

    [Test]
    public async Task Can_Get_Media_Property_By_Id()
    {
        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubTestMediaId);

        // Assert
        Assert.That(media, Is.Not.Null);
        Assert.That(media.Name, Is.EqualTo("SubTestMedia"));
        Assert.That(media.Value("testProperty"), Is.EqualTo("This is a test"));
    }

    [Test]
    public async Task Can_Get_Media_Property_By_Key()
    {
        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubTestMedia.Key.Value);

        // Assert
        Assert.That(media, Is.Not.Null);
        Assert.That(media.Name, Is.EqualTo("SubTestMedia"));
        Assert.That(media.Value("testProperty"), Is.EqualTo("This is a test"));
    }

    [Test]
    public async Task Can_Get_Updated_Media_By_Id()
    {
        // Arrange
        const string newName = "NewImageName";
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImageId);
        Assert.That(media.Name, Is.EqualTo("SubImage"));

        var mediaUpdateModel = new MediaUpdateModel
        {
            Properties = SubImage.Properties,
            Variants = [new VariantModel { Name = newName }]
        };

        // Act
        await MediaEditingService.UpdateAsync(SubImage.Key.Value, mediaUpdateModel, Constants.Security.SuperUserKey);
        var updatedMedia = await PublishedMediaHybridCache.GetByIdAsync(SubImageId);

        // Assert
        Assert.That(updatedMedia, Is.Not.Null);
        Assert.That(updatedMedia.Name, Is.EqualTo(newName));
    }

    [Test]
    public async Task Can_Get_Updated_Media_By_Key()
    {
        // Arrange
        const string newName = "NewImageName";
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImage.Key.Value);
        Assert.That(media.Name, Is.EqualTo("SubImage"));

        var mediaUpdateModel = new MediaUpdateModel
        {
            Properties = SubImage.Properties,
            Variants = [new VariantModel { Name = newName }]
        };

        // Act
        await MediaEditingService.UpdateAsync(SubImage.Key.Value, mediaUpdateModel, Constants.Security.SuperUserKey);
        var updatedMedia = await PublishedMediaHybridCache.GetByIdAsync(SubImageId);

        // Assert
        Assert.That(updatedMedia, Is.Not.Null);
        Assert.That(updatedMedia.Name, Is.EqualTo(newName));
    }

    [Test]
    public async Task Cannot_Get_Trashed_Media_By_Key()
    {
        // Arrange
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImage.Key.Value);
        Assert.That(media, Is.Not.Null, "Media should be in cache before trashing");

        // Act
        var trashResult = await MediaEditingService.MoveToRecycleBinAsync(SubImage.Key.Value, Constants.Security.SuperUserKey);
        Assert.That(trashResult.Success, Is.True);

        // Assert
        var trashedMedia = await PublishedMediaHybridCache.GetByIdAsync(SubImage.Key.Value);
        Assert.That(trashedMedia, Is.Null, "Trashed media should not be in cache");
    }

    [Test]
    public async Task Cannot_Get_Trashed_Media_By_Id()
    {
        // Arrange
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImageId);
        Assert.That(media, Is.Not.Null, "Media should be in cache before trashing");

        // Act
        var trashResult = await MediaEditingService.MoveToRecycleBinAsync(SubImage.Key.Value, Constants.Security.SuperUserKey);
        Assert.That(trashResult.Success, Is.True);

        // Assert
        var trashedMedia = await PublishedMediaHybridCache.GetByIdAsync(SubImageId);
        Assert.That(trashedMedia, Is.Null, "Trashed media should not be in cache");
    }

    [Test]
    public async Task Restored_Media_Is_Available_In_Cache()
    {
        // Arrange - Verify media is in cache, then trash it
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImage.Key.Value);
        Assert.That(media, Is.Not.Null, "Media should be in cache before trashing");

        var trashResult = await MediaEditingService.MoveToRecycleBinAsync(SubImage.Key.Value, Constants.Security.SuperUserKey);
        Assert.That(trashResult.Success, Is.True);

        var trashedMedia = await PublishedMediaHybridCache.GetByIdAsync(SubImage.Key.Value);
        Assert.That(trashedMedia, Is.Null, "Trashed media should not be in cache");

        // Act - Restore to root (original location)
        var restoreResult = await MediaEditingService.RestoreAsync(SubImage.Key.Value, null, Constants.Security.SuperUserKey);
        Assert.That(restoreResult.Success, Is.True);

        // Assert - Restored media should be back in the cache
        var restoredMedia = await PublishedMediaHybridCache.GetByIdAsync(SubImage.Key.Value);
        Assert.That(restoredMedia, Is.Not.Null, "Restored media should be in the cache");
    }

    [Test]
    public async Task Cannot_Get_Deleted_Media_By_Id()
    {
        // Arrange
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImageId);
        Assert.That(media, Is.Not.Null);

        await MediaEditingService.DeleteAsync(media.Key, Constants.Security.SuperUserKey);

        // Act
        var deletedMedia = await PublishedMediaHybridCache.GetByIdAsync(media.Id);

        // Assert
        Assert.That(deletedMedia, Is.Null);
    }

    [Test]
    public async Task Cannot_Get_Deleted_Media_By_Key()
    {
        // Arrange
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImage.Key.Value);

        Assert.That(media, Is.Not.Null);

        await MediaEditingService.DeleteAsync(media.Key, Constants.Security.SuperUserKey);

        // Act
        var deletedMedia = await PublishedMediaHybridCache.GetByIdAsync(media.Key);

        // Assert
        Assert.That(deletedMedia, Is.Null);
    }
}
