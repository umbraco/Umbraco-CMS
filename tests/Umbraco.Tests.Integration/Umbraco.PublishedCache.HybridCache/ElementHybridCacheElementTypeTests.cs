// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementHybridCacheElementTypeTests : UmbracoIntegrationTest
{
    private IContentType _elementType = null!;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ElementTreeChangeNotification, ElementTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeChangedNotification, ContentTypeChangedDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private IPublishedElementCache PublishedElementCache => GetRequiredService<IPublishedElementCache>();

    [SetUp]
    public async Task SetupTest()
    {
        _elementType = ContentTypeBuilder.CreateSimpleElementType();
        _elementType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(_elementType, Constants.Security.SuperUserKey);
    }

    [Test]
    public async Task Structural_Update_Removes_Property_From_Draft_Element()
    {
        // Arrange
        var elementKey = await CreateAndPublishElement("Test Element");
        var oldElement = await PublishedElementCache.GetByIdAsync(elementKey, true);
        Assert.That(oldElement, Is.Not.Null);
        Assert.That(oldElement!.GetProperty("title"), Is.Not.Null);

        // Act — remove the title property (structural change)
        _elementType.RemovePropertyType("title");
        await ContentTypeService.UpdateAsync(_elementType, Constants.Security.SuperUserKey);

        // Assert
        var newElement = await PublishedElementCache.GetByIdAsync(elementKey, true);
        Assert.That(newElement, Is.Not.Null);
        Assert.That(newElement!.GetProperty("title"), Is.Null);
    }

    [Test]
    public async Task Non_Structural_Update_Preserves_Property_Values()
    {
        // Arrange
        var elementKey = await CreateAndPublishElement("Test Element");
        var oldElement = await PublishedElementCache.GetByIdAsync(elementKey, true);
        Assert.That(oldElement, Is.Not.Null);
        Assert.That(oldElement!.GetProperty("title"), Is.Not.Null);

        // Act — non-structural change (rename the element type)
        _elementType.Name = "Renamed Element Type";
        await ContentTypeService.UpdateAsync(_elementType, Constants.Security.SuperUserKey);

        // Assert — property values should still be available
        var newElement = await PublishedElementCache.GetByIdAsync(elementKey, true);
        Assert.That(newElement, Is.Not.Null);
        Assert.That(newElement!.GetProperty("title"), Is.Not.Null);
    }

    [Test]
    public async Task Element_Gets_Removed_When_ElementType_Is_Deleted()
    {
        // Arrange
        var elementKey = await CreateAndPublishElement("Test Element");
        var element = await PublishedElementCache.GetByIdAsync(elementKey, preview: true);
        Assert.That(element, Is.Not.Null);

        // Act
        await ContentTypeService.DeleteAsync(element!.ContentType.Key, Constants.Security.SuperUserKey);

        // Assert
        var elementAgain = await PublishedElementCache.GetByIdAsync(elementKey, preview: true);
        Assert.That(elementAgain, Is.Null);
    }

    private async Task<Guid> CreateAndPublishElement(string name)
    {
        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _elementType.Key,
                Variants = [new() { Name = name }],
            },
            Constants.Security.SuperUserKey);

        Assert.That(createResult.Success, Is.True);
        var elementKey = createResult.Result.Content!.Key;

        var publishResult = await ElementPublishingService.PublishAsync(
            elementKey,
            [new CulturePublishScheduleModel { Culture = "*", Schedule = null }],
            Constants.Security.SuperUserKey);

        Assert.That(publishResult.Success, Is.True);
        return elementKey;
    }
}
