using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class MediaHybridCacheTests : UmbracoIntegrationTest
{
    private IPublishedMediaHybridCache PublishedMediaHybridCache => GetRequiredService<IPublishedMediaHybridCache>();

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
    public async Task Can_Get_Media_Property_By_Key()
    {
        // Arrange
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

        // Act
        var media = await PublishedMediaHybridCache.GetByKeyAsync(result.Result.Content.Key);

        UmbracoContextFactory.EnsureUmbracoContext();

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("Image", media.Name);
        Assert.AreEqual(mediaType.Key, media.ContentType.Key);
        Assert.AreEqual("NewTitle", media.Value("title"));
    }

    [Test]
    public async Task Can_Get_Media_Property_By_Id()
    {
        // Arrange
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

        // Act
        var media = await PublishedMediaHybridCache.GetByKeyAsync(result.Result.Content.Key);

        UmbracoContextFactory.EnsureUmbracoContext();

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("Image", media.Name);
        Assert.AreEqual(mediaType.Key, media.ContentType.Key);
        Assert.AreEqual("NewTitle", media.Value("title"));
    }
}
