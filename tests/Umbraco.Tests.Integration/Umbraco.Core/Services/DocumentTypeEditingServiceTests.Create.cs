using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing.Document;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentTypeEditingServiceTests
{

    [Test]
    public async Task Can_Create_Basic_DocumentType()
    {
        var name = "Test";
        var alias = "test";

        var createModel = CreateCreateModel(alias: alias, name: name, isElement: true);
        var response = await DocumentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // Ensure it's actually persisted
        var documentType = await ContentTypeService.GetAsync(response.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(documentType);
            Assert.IsTrue(documentType.IsElement);
            Assert.AreEqual(alias, documentType.Alias);
            Assert.AreEqual(name, documentType.Name);
            Assert.AreEqual(response.Result.Id, documentType.Id);
            Assert.AreEqual(response.Result.Key, documentType.Key);
        });
    }

    [Test]
    public async Task Can_Specify_Key()
    {
        var key = new Guid("33C326F6-CB5E-43D6-9730-E946AA5F9C7B");
        var createModel = CreateCreateModel(key: key);

        var response = await DocumentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        var documentType = await ContentTypeService.GetAsync(response.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(documentType);
            Assert.AreEqual(key, documentType.Key);
        });
    }

    [Test]
    public async Task Can_Specify_PropertyType_Key()
    {
        var propertyTypeKey = new Guid("82DDEBD8-D2CA-423E-B88D-6890F26152B4");

        var propertyTypeContainer =
            new DocumentTypePropertyContainer { Name = "Container", Type = TabContainerType, Key = Guid.NewGuid() };
        var propertyTypeCreateModel = CreatePropertyType(key: propertyTypeKey, containerKey: propertyTypeContainer.Key);

        var createModel = CreateCreateModel(
            propertyTypes: new[] { propertyTypeCreateModel },
            containers: new[] { propertyTypeContainer });

        var response = await DocumentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        var documentType = await ContentTypeService.GetAsync(response.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(documentType);
            var propertyType = documentType.PropertyGroups.FirstOrDefault()?.PropertyTypes?.FirstOrDefault();
            Assert.IsNotNull(propertyType);
            Assert.AreEqual(propertyTypeKey, propertyType.Key);
        });
    }

    [Test]
    public async Task Can_Create_Composite_DocumentType()
    {
        var compositionBase = CreateCreateModel(
            alias: "compositionBase",
            name: "Composition Base",
            isElement: true);

        // Let's add a property to ensure that it passes through
        var container = new DocumentTypePropertyContainer { Key = Guid.NewGuid(), Name = "Container", Type = TabContainerType };
        compositionBase.Containers = new[] { container };

        var compositionProperty = CreatePropertyType(name: "Composition Property", alias: "compositionProperty", containerKey: container.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await DocumentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(compositionResult.Success);
        var compositionType = compositionResult.Result;

        // Create doc type using the composition
        var documentType = CreateCreateModel(
            compositions: new[]
            {
                new ContentTypeComposition
                {
                CompositionType = ContentTypeCompositionType.Composition,
                Key = compositionType.Key,
                },
            }
        );

        var result = await DocumentTypeEditingService.CreateAsync(documentType, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var createdDocumentType = await ContentTypeService.GetAsync(result.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, createdDocumentType.ContentTypeComposition.Count());
            Assert.AreEqual(compositionType.Key, createdDocumentType.ContentTypeComposition.First().Key);
            Assert.AreEqual(1, compositionType.CompositionPropertyGroups.Count());
            Assert.AreEqual(container.Key, compositionType.CompositionPropertyGroups.First().Key);
            Assert.AreEqual(1, compositionType.CompositionPropertyTypes.Count());
            Assert.AreEqual(compositionProperty.Key, compositionType.CompositionPropertyTypes.First().Key);
        });
    }
}
