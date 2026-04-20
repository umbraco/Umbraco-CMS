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
internal sealed class ElementHybridCacheTests : UmbracoIntegrationTest
{
    private IContentType _elementType = null!;
    private Guid _elementTypeKey;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ElementTreeChangeNotification, ElementTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IPublishedElementCache PublishedElementCache => GetRequiredService<IPublishedElementCache>();

    [SetUp]
    public async Task SetupTest()
    {
        _elementType = ContentTypeBuilder.CreateBasicElementType();
        _elementType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(_elementType, Constants.Security.SuperUserKey);
        _elementTypeKey = _elementType.Key;
    }

    [Test]
    public async Task Can_Get_Draft_Element_By_Key()
    {
        // Arrange
        var elementKey = await CreateElement("Draft Element");

        // Act
        IPublishedElement? result = await PublishedElementCache.GetByKeyAsync(elementKey, preview: true);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Key, Is.EqualTo(elementKey));
    }

    [Test]
    public async Task Can_Get_Published_Element_By_Key()
    {
        // Arrange
        var elementKey = await CreateAndPublishElement("Published Element");

        // Act
        IPublishedElement? result = await PublishedElementCache.GetByKeyAsync(elementKey, preview: false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Key, Is.EqualTo(elementKey));
    }

    [Test]
    public async Task Cannot_Get_Unpublished_Element_Without_Preview()
    {
        // Arrange — save only, don't publish
        var elementKey = await CreateElement("Unpublished Element");

        // Act
        IPublishedElement? result = await PublishedElementCache.GetByKeyAsync(elementKey, preview: false);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Can_Get_Draft_Of_Published_Element()
    {
        // Arrange
        var elementKey = await CreateAndPublishElement("Published Element");

        // Act — request draft version of published element
        IPublishedElement? result = await PublishedElementCache.GetByKeyAsync(elementKey, preview: true);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Key, Is.EqualTo(elementKey));
    }

    [Test]
    public async Task Can_Get_Updated_Draft_Element()
    {
        // Arrange
        var elementKey = await CreateElement("Initial Name");

        // Update the element
        var updateResult = await ElementEditingService.UpdateAsync(
            elementKey,
            new ElementUpdateModel
            {
                Variants = [new() { Name = "Updated Name" }],
            },
            Constants.Security.SuperUserKey);
        Assert.That(updateResult.Success, Is.True);

        // Act
        IPublishedElement? result = await PublishedElementCache.GetByKeyAsync(elementKey, preview: true);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Updated Name"));
    }

    [Test]
    public async Task Can_Not_Get_Deleted_Element()
    {
        // Arrange
        var elementKey = await CreateAndPublishElement("To Be Deleted");

        // Verify it exists first
        var before = await PublishedElementCache.GetByKeyAsync(elementKey, preview: false);
        Assert.That(before, Is.Not.Null);

        // Act — delete via the element service
        var elementService = GetRequiredService<IElementService>();
        var element = elementService.GetById(elementKey);
        Assert.That(element, Is.Not.Null);
        elementService.Delete(element!);

        // Assert
        IPublishedElement? result = await PublishedElementCache.GetByKeyAsync(elementKey, preview: false);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Can_Get_Element_Name()
    {
        // Arrange
        var elementKey = await CreateAndPublishElement("My Element Name");

        // Act
        IPublishedElement? result = await PublishedElementCache.GetByKeyAsync(elementKey, preview: false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("My Element Name"));
    }

    [Test]
    public async Task Cannot_Get_Trashed_As_Published()
    {
        // Arrange
        var elementKey = await CreateAndPublishElement("Published Element");

        // Act — trash the element
        await ElementEditingService.MoveToRecycleBinAsync(elementKey, Constants.Security.SuperUserKey);

        // Assert
        var result = await PublishedElementCache.GetByKeyAsync(elementKey, preview: false);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Cannot_Get_Published_Again_After_Trashing()
    {
        // Arrange
        var elementKey = await CreateAndPublishElement("Published Element");

        // Verify it's in the published cache
        var before = await PublishedElementCache.GetByKeyAsync(elementKey, preview: false);
        Assert.That(before, Is.Not.Null);

        // Act — trash the element
        await ElementEditingService.MoveToRecycleBinAsync(elementKey, Constants.Security.SuperUserKey);

        // Assert
        var after = await PublishedElementCache.GetByKeyAsync(elementKey, preview: false);
        Assert.That(after, Is.Null);
    }

    [Test]
    public async Task Cannot_Get_Published_Elements_After_Folder_Trashed()
    {
        // Arrange — create a folder with published elements inside
        var folderKey = Guid.NewGuid();
        var folderResult = await ElementContainerService.CreateAsync(folderKey, "Test Folder", null, Constants.Security.SuperUserKey);
        Assert.That(folderResult.Success, Is.True);

        var element1Key = await CreateElementInFolder("Element One", folderKey);
        var element2Key = await CreateElementInFolder("Element Two", folderKey);

        await PublishElement(element1Key);
        await PublishElement(element2Key);

        // Verify both elements are in the published cache
        var before1 = await PublishedElementCache.GetByKeyAsync(element1Key, preview: false);
        var before2 = await PublishedElementCache.GetByKeyAsync(element2Key, preview: false);
        Assert.That(before1, Is.Not.Null);
        Assert.That(before2, Is.Not.Null);

        // Act — trash the folder
        var trashResult = await ElementContainerService.MoveToRecycleBinAsync(folderKey, Constants.Security.SuperUserKey);
        Assert.That(trashResult.Success, Is.True);

        // Assert — elements should no longer be in the published cache
        var after1 = await PublishedElementCache.GetByKeyAsync(element1Key, preview: false);
        var after2 = await PublishedElementCache.GetByKeyAsync(element2Key, preview: false);
        Assert.That(after1, Is.Null);
        Assert.That(after2, Is.Null);
    }

    private async Task<Guid> CreateElementInFolder(string name, Guid parentKey)
    {
        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _elementTypeKey,
                ParentKey = parentKey,
                Variants = [new() { Name = name }],
            },
            Constants.Security.SuperUserKey);

        Assert.That(createResult.Success, Is.True);
        return createResult.Result.Content!.Key;
    }

    private async Task PublishElement(Guid elementKey)
    {
        var publishResult = await ElementPublishingService.PublishAsync(
            elementKey,
            [new CulturePublishScheduleModel { Culture = "*", Schedule = null }],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);
    }

    private async Task<Guid> CreateElement(string name)
    {
        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _elementTypeKey,
                Variants = [new() { Name = name }],
            },
            Constants.Security.SuperUserKey);

        Assert.That(createResult.Success, Is.True);
        return createResult.Result.Content!.Key;
    }

    private async Task<Guid> CreateAndPublishElement(string name)
    {
        var elementKey = await CreateElement(name);

        var publishResult = await ElementPublishingService.PublishAsync(
            elementKey,
            [new CulturePublishScheduleModel { Culture = "*", Schedule = null }],
            Constants.Security.SuperUserKey);

        Assert.That(publishResult.Success, Is.True);
        return elementKey;
    }
}
