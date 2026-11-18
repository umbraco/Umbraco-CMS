using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
public partial class ElementServiceTests : UmbracoIntegrationTest
{
    private IElementService ElementService => GetRequiredService<IElementService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    [Test]
    public async Task Can_Save_Element()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        // Arrange
        IElement element = new Element("My Element", elementType);
        element.SetValue("title", "The Element Title");

        // Act
        var result = await ElementService.SaveAsync(element, Constants.Security.SuperUserKey, null, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(ContentSaveOperationStatus.Success));

        element = await ElementService.GetByIdAsync(element.Key, CancellationToken.None);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.HasIdentity, Is.True);
        Assert.That(element.Name, Is.EqualTo("My Element"));
        Assert.That(element.GetValue<string>("title"), Is.EqualTo("The Element Title"));
    }

    [Test]
    [LongRunning]
    public async Task DeleteOfTypesAsync_DeletesEveryElementOfTheTypeAcrossPagesAndLeavesOtherTypes()
    {
        var deletedType = ContentTypeBuilder.CreateSimpleElementType("deletedType", "Deleted Type");
        var keptType = ContentTypeBuilder.CreateSimpleElementType("keptType", "Kept Type");
        await ContentTypeService.CreateAsync(deletedType, Constants.Security.SuperUserKey);
        await ContentTypeService.CreateAsync(keptType, Constants.Security.SuperUserKey);

        IElement[] elementsOfDeletedType = [.. Enumerable.Range(0, 501).Select(index => new Element($"Element {index}", deletedType))];
        Assert.That((await ElementService.SaveAsync(elementsOfDeletedType, Constants.Security.SuperUserKey, CancellationToken.None)).Success, Is.True);
        IElement trashed = elementsOfDeletedType[0];
        Assert.That((await GetRequiredService<IElementEditingService>().MoveToRecycleBinAsync(trashed.Key, Constants.Security.SuperUserKey)).Success, Is.True);

        IElement kept = new Element("Kept", keptType);
        Assert.That((await ElementService.SaveAsync(kept, Constants.Security.SuperUserKey, null, CancellationToken.None)).Success, Is.True);

        Attempt<ContentDeleteOfTypesOperationStatus> result = await ElementService.DeleteOfTypesAsync(
            [deletedType.Key], Constants.Security.SuperUserKey, CancellationToken.None);

        PagedModel<IElement> remainingOfDeletedType = await ElementService.GetPagedOfTypesAsync([deletedType.Key], 0, 1, null, CancellationToken.None);
        Assert.Multiple(async () =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(remainingOfDeletedType.Total, Is.Zero);
            Assert.That(await ElementService.GetByIdAsync(trashed.Key, CancellationToken.None), Is.Null);
            Assert.That(await ElementService.GetByIdAsync(kept.Key, CancellationToken.None), Is.Not.Null);
        });
    }

    [Test]
    public async Task DeleteOfTypesAsync_WithUnknownElementTypeKey_ReportsNotFound()
    {
        Attempt<ContentDeleteOfTypesOperationStatus> result = await ElementService.DeleteOfTypesAsync(
            [Guid.NewGuid()], Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(ContentDeleteOfTypesOperationStatus.NotFound));
        });
    }
}
