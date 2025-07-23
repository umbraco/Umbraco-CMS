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
        Assert.IsNotNull(media);
        Assert.AreEqual("RootFolder", media.Name);
        Assert.AreEqual(RootFolder.ContentTypeKey, media.ContentType.Key);
    }

    [Test]
    public async Task Can_Get_Root_Media_By_Key()
    {
        var media = await PublishedMediaHybridCache.GetByIdAsync(RootFolder.Key.Value);

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("RootFolder", media.Name);
        Assert.AreEqual(RootFolder.ContentTypeKey, media.ContentType.Key);
    }

    [Test]
    public async Task Can_Get_Child_Media_By_Id()
    {
        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImageId);

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("SubImage", media.Name);
        Assert.AreEqual(SubImage.ContentTypeKey, media.ContentType.Key);
        Assert.AreEqual(SubImage.ParentKey, RootFolder.Key);
    }


    [Test]
    public async Task Can_Get_Child_Media_By_Key()
    {
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImage.Key.Value);

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("SubImage", media.Name);
        Assert.AreEqual(SubImage.ContentTypeKey, media.ContentType.Key);
        Assert.AreEqual(SubImage.ParentKey, RootFolder.Key);
    }

    [Test]
    public async Task Cannot_Get_Non_Existing_Media_By_Id()
    {
        // Act
        const int nonExistingId = 124214;
        var media = await PublishedMediaHybridCache.GetByIdAsync(nonExistingId);

        // Assert
        Assert.IsNull(media);
    }

    [Test]
    public async Task Cannot_Get_Non_Existing_Media_By_Key()
    {
        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.IsNull(media);
    }

    [Test]
    public async Task Can_Get_Media_Property_By_Id()
    {
        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubTestMediaId);

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("SubTestMedia", media.Name);
        Assert.AreEqual("This is a test", media.Value("testProperty"));
    }

    [Test]
    public async Task Can_Get_Media_Property_By_Key()
    {
        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubTestMedia.Key.Value);

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("SubTestMedia", media.Name);
        Assert.AreEqual("This is a test", media.Value("testProperty"));
    }

    [Test]
    public async Task Can_Get_Updated_Media_By_Id()
    {
        // Arrange
        const string newName = "NewImageName";
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImageId);
        Assert.AreEqual(media.Name, "SubImage");

        var mediaUpdateModel = new MediaUpdateModel
        {
            Properties = SubImage.Properties,
            Variants = [new VariantModel { Name = newName }]
        };

        // Act
        await MediaEditingService.UpdateAsync(SubImage.Key.Value, mediaUpdateModel, Constants.Security.SuperUserKey);
        var updatedMedia = await PublishedMediaHybridCache.GetByIdAsync(SubImageId);

        // Assert
        Assert.IsNotNull(updatedMedia);
        Assert.AreEqual(newName, updatedMedia.Name);
    }

    [Test]
    public async Task Can_Get_Updated_Media_By_Key()
    {
        // Arrange
        const string newName = "NewImageName";
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImage.Key.Value);
        Assert.AreEqual(media.Name, "SubImage");

        var mediaUpdateModel = new MediaUpdateModel
        {
            Properties = SubImage.Properties,
            Variants = [new VariantModel { Name = newName }]
        };

        // Act
        await MediaEditingService.UpdateAsync(SubImage.Key.Value, mediaUpdateModel, Constants.Security.SuperUserKey);
        var updatedMedia = await PublishedMediaHybridCache.GetByIdAsync(SubImageId);

        // Assert
        Assert.IsNotNull(updatedMedia);
        Assert.AreEqual(newName, updatedMedia.Name);
    }

    [Test]
    public async Task Cannot_Get_Deleted_Media_By_Id()
    {
        // Arrange
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImageId);
        Assert.IsNotNull(media);

        await MediaEditingService.DeleteAsync(media.Key, Constants.Security.SuperUserKey);

        // Act
        var deletedMedia = await PublishedMediaHybridCache.GetByIdAsync(media.Id);

        // Assert
        Assert.IsNull(deletedMedia);
    }

    [Test]
    public async Task Cannot_Get_Deleted_Media_By_Key()
    {
        // Arrange
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubImage.Key.Value);

        Assert.IsNotNull(media);

        await MediaEditingService.DeleteAsync(media.Key, Constants.Security.SuperUserKey);

        // Act
        var deletedMedia = await PublishedMediaHybridCache.GetByIdAsync(media.Key);

        // Assert
        Assert.IsNull(deletedMedia);
    }
}
