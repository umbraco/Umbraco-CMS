using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
[Platform("Linux", Reason = "This uses too much memory when running both caches, should be removed when nucache is removed")]
public class MediaHybridCacheMediaTypeTests : UmbracoIntegrationTestWithMediaEditing
{
    private IPublishedMediaCache PublishedMediaHybridCache => GetRequiredService<IPublishedMediaCache>();

    private new IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();


    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    // Currently failing, unsure if actual issue or test issue
    [Test]
    public async Task Can_Get_Media_By_Id()
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
    public async Task Can_Get_Media_By_Key()
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
    public async Task Media_Gets_Removed_When_MediaType_Is_Deleted()
    {
        // Load into cache
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubTestMedia.Key.Value);
        Assert.IsNotNull(media);

        await MediaTypeService.DeleteAsync(CustomMediaType.Key, Constants.Security.SuperUserKey);

        var newMedia = await PublishedMediaHybridCache.GetByIdAsync(SubTestMedia.Key.Value);
        Assert.IsNull(newMedia);
    }
}
