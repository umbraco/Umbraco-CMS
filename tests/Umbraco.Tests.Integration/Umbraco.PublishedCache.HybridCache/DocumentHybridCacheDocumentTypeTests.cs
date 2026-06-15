using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheDocumentTypeTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeChangedNotification, ContentTypeChangedDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    [Test]
    public async Task Structural_Update_Removes_Property_From_Draft_Content_By_Id()
    {
        // Act
        var oldTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.That(oldTextPage.Value("title"), Is.Not.Null);

        ContentType.RemovePropertyType("title");
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Assert
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.That(newTextPage.Value("title"), Is.Null);
    }

    [Test]
    public async Task Structural_Update_Removes_Property_From_Draft_Content_By_Key()
    {
        // Act
        await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);

        ContentType.RemovePropertyType("title");
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        //Assert
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);
        Assert.That(newTextPage.Value("title"), Is.Null);
    }

    [Test]
    public async Task Non_Structural_Update_Preserves_Property_Values_In_Draft_Content()
    {
        // Arrange - load content into cache and verify the title property has a value.
        var oldTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.That(oldTextPage, Is.Not.Null);
        var originalTitle = oldTextPage.Value("title");
        Assert.That(originalTitle, Is.Not.Null);

        // Act - perform a non-structural change (rename the content type).
        ContentType.Name = "Renamed Textpage";
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Assert - property values should still be available from cache.
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(TextpageId, true);
        Assert.That(newTextPage, Is.Not.Null);
        Assert.That(newTextPage.Value("title"), Is.EqualTo(originalTitle));
    }

    [Test]
    public async Task Non_Structural_Update_Preserves_Property_Values_When_Fetched_By_Key()
    {
        // Arrange - load content into cache by key and verify the title property has a value.
        var oldTextPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);
        Assert.That(oldTextPage, Is.Not.Null);
        var originalTitle = oldTextPage.Value("title");
        Assert.That(originalTitle, Is.Not.Null);

        // Act - perform a non-structural change (update the description).
        ContentType.Description = "Updated description";
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Assert - property values should still be available from cache.
        var newTextPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, true);
        Assert.That(newTextPage, Is.Not.Null);
        Assert.That(newTextPage.Value("title"), Is.EqualTo(originalTitle));
    }

    [Test]
    public async Task Content_Gets_Removed_When_DocumentType_Is_Deleted()
    {
        // Load into cache
        var textPage = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, preview: true);
        Assert.That(textPage, Is.Not.Null);

        await ContentTypeService.DeleteAsync(textPage.ContentType.Key, Constants.Security.SuperUserKey);

        var textPageAgain = await PublishedContentHybridCache.GetByIdAsync(Textpage.Key.Value, preview: true);
        Assert.That(textPageAgain, Is.Null);
    }
}
