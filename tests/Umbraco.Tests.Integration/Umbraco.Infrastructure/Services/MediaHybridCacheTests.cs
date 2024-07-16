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

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddHybridCache();
        services.AddSingleton<IPublishedMediaHybridCache, MediaCache>();
        services.AddSingleton<IContentCacheService, ContentCacheService>();
        services.AddSingleton<IMediaCacheService, MediaCacheService>();
        services.AddSingleton<INuCacheContentRepository, NuCacheContentRepository>();
        services.AddSingleton<IContentCacheDataSerializerFactory, MsgPackContentNestedDataSerializerFactory>();
        services.AddSingleton<IPropertyCacheCompressionOptions, NoopPropertyCacheCompressionOptions>();
        services.AddNotificationAsyncHandler<MediaRefreshNotification, CacheRefreshingNotificationHandler>();
        services.AddTransient<IPublishedContentFactory, PublishedContentFactory>();
    }

    [Test]
    public async Task Can_Get_Draft_Content_By_Id()
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
}
