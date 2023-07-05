﻿using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing.Document;
using Umbraco.Cms.Core.Models.ContentTypeEditing.PropertyTypes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentTypeEditingServiceTests
{

    [Test]
    public async Task Can_Create_Basic_DocumentType()
    {
        var name = "Test";
        var alias = "test";

        var createModel = new DocumentTypeCreateModel { Alias = alias, Name = name };
        createModel.IsElement = true;

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
        var createModel = new DocumentTypeCreateModel { Alias = "test", Name = "Test", Key = key };

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
    public async Task Can_Create_Composite_DocumentType()
    {
        var compositionBase = new DocumentTypeCreateModel
        {
            Alias = "compositionBase",
            Name = "Composition Base",
            IsElement = true,
        };

        // Let's add a property to ensure that it passes through
        var container = new DocumentTypePropertyContainer { Key = Guid.NewGuid(), Name = "Container", Type = "Tab" };
        compositionBase.Containers = new[] { container };

        var compositionProperty = CreatePropertyType(name: "Composition Property", alias: "compositionProperty", containerKey: container.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await DocumentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.IsTrue(compositionResult.Success);
        var compositionType = compositionResult.Result;

        // Create doc type using the composition
        var documentType = new DocumentTypeCreateModel
        {
            Alias = "test",
            Name = "Test",
            Compositions = new[]
            {
                new ContentTypeComposition
                {
                CompositionType = ContentTypeCompositionType.Composition,
                Key = compositionType.Key,
                },
            },
        };

        var result = await DocumentTypeEditingService.CreateAsync(documentType, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var createdDocumentType = result.Result;

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
