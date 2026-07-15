using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisableDeleteWhenReferenced))]
    public async Task Cannot_Delete_When_Element_Is_Referenced_By_External_Block_Content_And_Configured_To_Disable_When_Referenced()
    {
        var elementType = await CreateInvariantElementType();
        var referencingElement = await CreateInvariantElement(contentTypeKey: elementType.Key);
        var referencedElement = await CreateInvariantElement(contentTypeKey: elementType.Key);

        // Embedding an element as external block content records this relation, which must protect it from deletion.
        RelationService.Relate(
            referencingElement.Id,
            referencedElement.Id,
            Constants.Conventions.RelationTypes.RelatedExternalBlockElementAlias);

        var result = await ElementEditingService.DeleteAsync(referencedElement.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.CannotDeleteWhenReferenced, result.Status);
        });

        var element = await ElementEditingService.GetAsync(referencedElement.Key);
        Assert.IsNotNull(element);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Delete_FromOutsideOfRecycleBin(bool variant)
    {
        var element = await (variant ? CreateCultureVariantElement() : CreateInvariantElement());

        var result = await ElementEditingService.DeleteAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        // re-get and verify deletion
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.IsNull(element);
    }

    [Test]
    public async Task Can_Delete_FromRecycleBin()
    {
        var element = await CreateInvariantElement();
        await ElementEditingService.MoveToRecycleBinAsync(element.Key,  Constants.Security.SuperUserKey);

        var result = await ElementEditingService.DeleteAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        // re-get and verify deletion
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.IsNull(element);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing()
    {
        var result = await ElementEditingService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result.Status);
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

        Assert.AreEqual(5, EntityService.GetRootEntities(UmbracoObjectTypes.Element).Count());
        Assert.AreEqual(5, EntityService.GetPagedTrashedChildren(Constants.System.RecycleBinElementKey, UmbracoObjectTypes.Element, 0, 100, out _).Count());

        var result = await ContentTypeService.DeleteAsync(elementType.Key, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentTypeOperationStatus.Success, result);

        Assert.AreEqual(0, EntityService.GetRootEntities(UmbracoObjectTypes.Element).Count());
        Assert.AreEqual(0, EntityService.GetPagedTrashedChildren(Constants.System.RecycleBinElementKey, UmbracoObjectTypes.Element, 0, 100, out _).Count());
    }
}
