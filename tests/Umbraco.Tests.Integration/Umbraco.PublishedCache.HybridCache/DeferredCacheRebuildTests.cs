using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DeferredCacheRebuildTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeChangedNotification, ContentTypeChangedDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.Services.PostConfigure<CacheSettings>(options =>
            options.ContentTypeRebuildMode = ContentTypeRebuildMode.Deferred);
    }

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private DeferredCacheRebuildService DeferredCacheRebuildService =>
        (DeferredCacheRebuildService)GetRequiredService<IDeferredCacheRebuildService>();

    [Test]
    public async Task Deferred_Structural_Update_Eventually_Removes_Property()
    {
        // Arrange — verify the property exists before the structural change.
        var oldTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.IsNotNull(oldTextPage.Value("title"));

        // Act — remove the property (structural change). In Deferred mode, the rebuild is queued.
        ContentType.RemovePropertyType("title");
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Wait for the deferred rebuild to complete.
        await DeferredCacheRebuildService.WaitForPendingRebuildsAsync();

        // Assert — after the deferred rebuild, the property should be gone.
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.IsNull(newTextPage.Value("title"));
    }

    [Test]
    public async Task Deferred_Non_Structural_Update_Preserves_Property_Values()
    {
        // Arrange — load content and verify.
        var oldTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.IsNotNull(oldTextPage);
        var originalTitle = oldTextPage.Value("title");
        Assert.IsNotNull(originalTitle);

        // Act — non-structural change (rename). Should not trigger a rebuild at all.
        ContentType.Name = "Renamed Textpage";
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Assert — property values preserved (no rebuild needed).
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.IsNotNull(newTextPage);
        Assert.AreEqual(originalTitle, newTextPage.Value("title"));
    }
}
