using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class MediaHybridCacheTests : UmbracoIntegrationTest
{
    private IPublishedMediaCache PublishedMediaHybridCache => GetRequiredService<IPublishedMediaCache>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    [Test]
    public async Task Can_Get_Media_By_Key()
    {
        // Arrange
        var newMediaType = new MediaTypeBuilder()
            .WithAlias("album")
            .WithName("Album")
            .Build();

        newMediaType.AllowedAsRoot = true;
        MediaTypeService.Save(newMediaType);

        var createModel = new MediaCreateModel
        {
            ContentTypeKey = newMediaType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Image",
        };

        var result = await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Act
        var media = await PublishedMediaHybridCache.GetByKeyAsync(result.Result.Content.Key);

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("Image", media.Name);
        Assert.AreEqual(newMediaType.Key, media.ContentType.Key);
    }

    [Test]
    public async Task Can_Get_Media_By_Id()
    {
        // Arrange
        var newMediaType = new MediaTypeBuilder()
            .WithAlias("album")
            .WithName("Album")
            .Build();

        newMediaType.AllowedAsRoot = true;
        MediaTypeService.Save(newMediaType);

        var createModel = new MediaCreateModel
        {
            ContentTypeKey = newMediaType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Image",
        };

        var result = await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(result.Result.Content.Id);

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("Image", media.Name);
        Assert.AreEqual(newMediaType.Key, media.ContentType.Key);
    }

    [Test]
    public async Task Cannot_Get_Non_Existing_Media_By_Key()
    {
        // Act
        var media = await PublishedMediaHybridCache.GetByKeyAsync(Guid.NewGuid());

        // Assert
        Assert.IsNull(media);
    }

    [Test]
    public async Task Cannot_Get_Non_Existing_Media_By_Id()
    {
        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(124214);

        // Assert
        Assert.IsNull(media);
    }

    [Test]
    public async Task Can_Get_Media_Property_By_Key()
    {
        // Arrange
        var media = await CreateMedia();

        // Act
        var publishedMedia = await PublishedMediaHybridCache.GetByKeyAsync(media.Key);

        UmbracoContextFactory.EnsureUmbracoContext();

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("Image", media.Name);
        Assert.AreEqual("NewTitle", publishedMedia.Value("title"));
    }

    [Test]
    public async Task Can_Get_Media_Property_By_Id()
    {
        // Arrange
        var media = await CreateMedia();

        // Act
        var publishedMedia = await PublishedMediaHybridCache.GetByKeyAsync(media.Key);

        UmbracoContextFactory.EnsureUmbracoContext();

        // Assert
        Assert.IsNotNull(publishedMedia);
        Assert.AreEqual("Image", publishedMedia.Name);
        Assert.AreEqual("NewTitle", publishedMedia.Value("title"));
    }

    [Test]
    public async Task Has_Media_By_Id_Returns_True_If_In_Cache()
    {
        // Arrange
        var media = await CreateMedia();
        await PublishedMediaHybridCache.GetByIdAsync(media.Id);

        // Act
        var hasMedia = await PublishedMediaHybridCache.HasByIdAsync(media.Id);

        // Assert
        Assert.IsTrue(hasMedia);
    }

    [Test]
    public async Task Can_Get_Updated_Media()
    {
        // Arrange
        var media = await CreateMedia();
        await PublishedMediaHybridCache.GetByIdAsync(media.Id);

        // Act
        var updateModel = new MediaUpdateModel()
        {
            InvariantName = "Update name",
            InvariantProperties = new List<PropertyValueModel>()
            {
                new()
                {
                    Alias = "title",
                    Value = "Updated Title"
                }
            }
        };

        var updateAttempt = await MediaEditingService.UpdateAsync(media.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(updateAttempt.Success);
        var publishedMedia = await PublishedMediaHybridCache.GetByIdAsync(media.Id);
        UmbracoContextFactory.EnsureUmbracoContext();

        // Assert
        Assert.IsNotNull(publishedMedia);
        Assert.AreEqual("Update name", publishedMedia.Name);
        Assert.AreEqual("Updated Title", publishedMedia.Value("title"));
    }

    [Test]
    public async Task Cannot_Get_Deleted_Media_By_Id()
    {
        // Arrange
        var media = await CreateMedia();
        var publishedMedia = await PublishedMediaHybridCache.GetByIdAsync(media.Id);
        Assert.IsNotNull(publishedMedia);

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
        var media = await CreateMedia();
        var publishedMedia = await PublishedMediaHybridCache.GetByKeyAsync(media.Key);
        Assert.IsNotNull(publishedMedia);

        await MediaEditingService.DeleteAsync(media.Key, Constants.Security.SuperUserKey);

        // Act
        var deletedMedia = await PublishedMediaHybridCache.GetByKeyAsync(media.Key);

        // Assert
        Assert.IsNull(deletedMedia);
    }

    private async Task<IMedia> CreateMedia()
    {
        IMediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("test", "Test");
        mediaType.AllowedAsRoot = true;
        MediaTypeService.Save(mediaType);

        var createModel = new MediaCreateModel
        {
            ContentTypeKey = mediaType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Image",
            InvariantProperties = new List<PropertyValueModel>()
            {
                new()
                {
                    Alias = "title",
                    Value = "NewTitle"
                }
            }
        };

        var result = await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content;
    }
}
