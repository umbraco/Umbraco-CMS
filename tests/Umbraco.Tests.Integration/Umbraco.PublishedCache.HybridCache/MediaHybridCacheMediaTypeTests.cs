using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaHybridCacheMediaTypeTests : UmbracoIntegrationTestWithMediaEditing
{
    private IPublishedMediaCache PublishedMediaHybridCache => GetRequiredService<IPublishedMediaCache>();

    private new IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<MediaTreeChangeNotification, MediaTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<MediaTypeChangedNotification, MediaTypeChangedDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    [Test]
    public async Task Structural_Update_Removes_Property_From_Media_By_Id()
    {
        // Arrange
        var oldMedia = await PublishedMediaHybridCache.GetByIdAsync(SubTestMediaId);
        Assert.IsNotNull(oldMedia.Value("testProperty"));
        MediaTypeUpdateHelper mediaTypeUpdateHelper = new MediaTypeUpdateHelper();

        // Act
        var updateModel = mediaTypeUpdateHelper.CreateMediaTypeUpdateModel(CustomMediaType);
        updateModel.Properties = [];
        updateModel.Containers = [];
        await MediaTypeEditingService.UpdateAsync(CustomMediaType, updateModel, Constants.Security.SuperUserKey);

        // Assert
        var newMedia = await PublishedMediaHybridCache.GetByIdAsync(SubTestMediaId);
        Assert.IsNull(newMedia.Value("testProperty"));
    }

    [Test]
    public async Task Structural_Update_Removes_Property_From_Media_By_Key()
    {
        // Arrange
        var oldMedia = await PublishedMediaHybridCache.GetByIdAsync(SubTestMedia.Key.Value);
        Assert.IsNotNull(oldMedia.Value("testProperty"));
        MediaTypeUpdateHelper mediaTypeUpdateHelper = new MediaTypeUpdateHelper();

        // Act
        var updateModel = mediaTypeUpdateHelper.CreateMediaTypeUpdateModel(CustomMediaType);
        updateModel.Properties = [];
        updateModel.Containers = [];
        await MediaTypeEditingService.UpdateAsync(CustomMediaType, updateModel, Constants.Security.SuperUserKey);

        // Assert
        var newMedia = await PublishedMediaHybridCache.GetByIdAsync(SubTestMedia.Key.Value);
        Assert.IsNull(newMedia.Value("testProperty"));
    }

    [Test]
    public async Task Non_Structural_Update_Preserves_Property_Values_In_Media()
    {
        // Arrange - load media into cache and verify the property has a value.
        var oldMedia = await PublishedMediaHybridCache.GetByIdAsync(SubTestMediaId);
        Assert.IsNotNull(oldMedia);
        var originalValue = oldMedia.Value("testProperty");
        Assert.IsNotNull(originalValue);
        MediaTypeUpdateHelper mediaTypeUpdateHelper = new MediaTypeUpdateHelper();

        // Act - perform a non-structural change (rename the media type).
        var updateModel = mediaTypeUpdateHelper.CreateMediaTypeUpdateModel(CustomMediaType);
        updateModel.Name = "Renamed Media Type";
        await MediaTypeEditingService.UpdateAsync(CustomMediaType, updateModel, Constants.Security.SuperUserKey);

        // Assert - property values should still be available from cache
        var newMedia = await PublishedMediaHybridCache.GetByIdAsync(SubTestMediaId);
        Assert.IsNotNull(newMedia);
        Assert.AreEqual(originalValue, newMedia.Value("testProperty"));
    }

    [Test]
    public async Task Media_Gets_Removed_When_MediaType_Is_Deleted()
    {
        // Arrange
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubTestMedia.Key.Value);
        Assert.IsNotNull(media);

        // Act
        await MediaTypeService.DeleteAsync(CustomMediaType.Key, Constants.Security.SuperUserKey);

        // Assert
        var newMedia = await PublishedMediaHybridCache.GetByIdAsync(SubTestMedia.Key.Value);
        Assert.IsNull(newMedia);
    }
}
