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
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheContentTypeSeedingTests : UmbracoIntegrationTestWithContent
{
    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private Content _subSubPage;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.Services.PostConfigure<CacheSettings>(options =>
        {
            options.ContentTypeKeys = [new Guid(TextpageContentTypeKey)];
            options.DocumentBreadthFirstSeedCount = 0;
        });
    }

    public override void Setup()
    {
        base.Setup();
        _subSubPage = ContentBuilder.CreateSimpleContent(ContentType, "_subSubPage", Subpage.Id);
        _subSubPage.Key = Guid.Parse("E4C369B5-CCCA-4981-ADAC-389824CF6B0B");
        ContentService.Save(_subSubPage, -1);
    }

    [Test]
    public async Task Seeding_By_Content_Type_Skips_A_Published_Page_Whose_Parent_Is_Unpublished()
    {
        await ContentPublishingService.PublishBranchAsync(Textpage.Key, Array.Empty<string>(), PublishBranchFilter.All, Constants.Security.SuperUserKey, false);
        await ContentPublishingService.UnpublishAsync(Subpage.Key, null, Constants.Security.SuperUserKey);

        var cacheService = DocumentCacheService as DocumentCacheService;
        cacheService!.ResetSeedKeys();
        await DocumentCacheService.ClearMemoryCacheAsync(CancellationToken.None);

        Assert.IsTrue(await DocumentCacheService.HasContentByIdAsync(Textpage.Id));
        Assert.IsNull(await PublishedContentCache.GetByIdAsync(_subSubPage.Key));

        await ContentPublishingService.PublishAsync(Subpage.Key, [new() { Culture = Constants.System.InvariantCulture }], Constants.Security.SuperUserKey);
        CacheTestsHelper.AssertPage(_subSubPage, await PublishedContentCache.GetByIdAsync(_subSubPage.Key));
    }
}
