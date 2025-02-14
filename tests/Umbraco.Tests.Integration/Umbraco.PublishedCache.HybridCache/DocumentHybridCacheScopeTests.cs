using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DocumentHybridCacheScopeTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private ICoreScopeProvider CoreScopeProvider => GetRequiredService<ICoreScopeProvider>();

    [Test]
    public async Task Can_Get_Correct_Content_After_Rollback_With_Id()
    {
        using (CoreScopeProvider.CreateCoreScope())
        {
            await ContentPublishingService.PublishAsync(Textpage.Key.Value, CultureAndSchedule, Constants.Security.SuperUserKey);
        }

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId);

        // Published page should not be in cache, as we rolled scope back.
        Assert.IsNull(textPage);
    }

    [Test]
    public async Task Can_Get_Correct_Content_After_Rollback_With_Key()
    {
        using (CoreScopeProvider.CreateCoreScope())
        {
            await ContentPublishingService.PublishAsync(Textpage.Key.Value, CultureAndSchedule, Constants.Security.SuperUserKey);
        }

        // Act
        var textPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value);

        // Published page should not be in cache, as we rolled scope back.
        Assert.IsNull(textPage);
    }

    [Test]
    public async Task Can_Get_Document_After_Scope_Complete_With_Id()
    {
        using (var scope = CoreScopeProvider.CreateCoreScope())
        {
            await ContentPublishingService.PublishAsync(Textpage.Key.Value, CultureAndSchedule, Constants.Security.SuperUserKey);
            scope.Complete();
        }

        // Act
        var publishedPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId);

        // Published page should not be in cache, as we rolled scope back.
        Assert.IsNotNull(publishedPage);
    }

    [Test]
    public async Task Can_Get_Document_After_Scope_Completes_With_Key()
    {
        using (var scope = CoreScopeProvider.CreateCoreScope())
        {
            await ContentPublishingService.PublishAsync(Textpage.Key.Value, CultureAndSchedule, Constants.Security.SuperUserKey);
            scope.Complete();
        }

        // Act
        var publishedPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value);

        // Published page should not be in cache, as we rolled scope back.
        Assert.IsNotNull(publishedPage);
    }

    [Test]
    public async Task Can_Save_And_Publish_In_Same_Scope()
    {
        var key = Guid.NewGuid();
        using (var scope = CoreScopeProvider.CreateCoreScope())
        {
            Textpage.Key = key;
            var result = await ContentEditingService.CreateAsync(Textpage, Constants.Security.SuperUserKey);
            Assert.IsTrue(result);
            var publishResult = await ContentPublishingService.PublishAsync(Textpage.Key.Value, new List<CulturePublishScheduleModel>
            {
                new() { Culture = "*" },
            }, Constants.Security.SuperUserKey);
            Assert.IsTrue(publishResult.Success);

            scope.Complete();
        }

        var published = await PublishedContentHybridCache.GetByIdAsync(key);
        Assert.IsNotNull(published);
    }
}
