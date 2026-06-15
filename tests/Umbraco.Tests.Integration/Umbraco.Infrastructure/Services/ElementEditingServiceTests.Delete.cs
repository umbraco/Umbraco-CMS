using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Delete_FromOutsideOfRecycleBin(bool variant)
    {
        var element = await (variant ? CreateCultureVariantElement() : CreateInvariantElement());

        var result = await ElementEditingService.DeleteAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        // re-get and verify deletion
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Null);
    }

    [Test]
    public async Task Can_Delete_FromRecycleBin()
    {
        var element = await CreateInvariantElement();
        await ElementEditingService.MoveToRecycleBinAsync(element.Key,  Constants.Security.SuperUserKey);

        var result = await ElementEditingService.DeleteAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        // re-get and verify deletion
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Null);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing()
    {
        var result = await ElementEditingService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotFound));
    }

    [Test]
    public async Task Deleting_Element_Type_Deletes_All_Elements_Of_That_Type()
    {
        var elementType = await CreateInvariantElementType();

        for (var i = 0; i < 10; i++)
        {
            var key = Guid.NewGuid();
            await ElementEditingService.CreateAsync(
                new ElementCreateModel
                {
                    Key = key,
                    ContentTypeKey = elementType.Key,
                    ParentKey = null,
                    Variants = [new() { Name = $"Name {i}" }],
                },
                Constants.Security.SuperUserKey);

            if (i % 2 == 0)
            {
                // move half of the created elements to trash, to ensure that also trashed elements are deleted
                // when deleting the element type
                await ElementEditingService.MoveToRecycleBinAsync(key, Constants.Security.SuperUserKey);
            }
        }

        Assert.That(EntityService.GetRootEntities(UmbracoObjectTypes.Element).Count(), Is.EqualTo(5));
        Assert.That(EntityService.GetPagedTrashedChildren(Constants.System.RecycleBinElementKey, UmbracoObjectTypes.Element, 0, 100, out _).Count(), Is.EqualTo(5));

        var result = await ContentTypeService.DeleteAsync(elementType.Key, Constants.Security.SuperUserKey);
        Assert.That(result, Is.EqualTo(ContentTypeOperationStatus.Success));

        Assert.That(EntityService.GetRootEntities(UmbracoObjectTypes.Element).Count(), Is.EqualTo(0));
        Assert.That(EntityService.GetPagedTrashedChildren(Constants.System.RecycleBinElementKey, UmbracoObjectTypes.Element, 0, 100, out _).Count(), Is.EqualTo(0));
    }
}
