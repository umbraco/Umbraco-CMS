﻿using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class MediaTypeEditingServiceTests
{
    [Test]
    public async Task Can_Create_With_All_Basic_Settings()
    {
        var createModel = CreateCreateModel("Test Media Type", "testMediaType");
        createModel.Description = "This is the Test description";
        createModel.Icon = "icon icon-something";
        createModel.AllowedAsRoot = true;
        var result = await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // Ensure it's actually persisted
        var mediaType = await MediaTypeService.GetAsync(result.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(mediaType);
            Assert.AreEqual("testMediaType", mediaType.Alias);
            Assert.AreEqual("Test Media Type", mediaType.Name);
            Assert.AreEqual(result.Result.Id, mediaType.Id);
            Assert.AreEqual(result.Result.Key, mediaType.Key);
            Assert.AreEqual("This is the Test description", mediaType.Description);
            Assert.AreEqual("icon icon-something", mediaType.Icon);
            Assert.IsTrue(mediaType.AllowedAsRoot);
        });
    }

    [Test]
    public async Task Can_Create_In_A_Folder()
    {
        var containerResult = MediaTypeService.CreateContainer(Constants.System.Root, Guid.NewGuid(), "Test folder");
        Assert.IsTrue(containerResult.Success);
        var container = containerResult.Result?.Entity;
        Assert.IsNotNull(container);

        var createModel = CreateCreateModel("Test", "test", parentKey: container.Key);
        var result = await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted in the folder
        var mediaType = await MediaTypeService.GetAsync(result.Result!.Key);
        Assert.IsNotNull(mediaType);
        Assert.AreEqual(container.Id, mediaType.ParentId);
    }

    [Test]
    public async Task Can_Create_With_Properties_In_A_Container()
    {
        var createModel = CreateCreateModel("Test", "test");
        var container = CreateContainer();
        createModel.Containers = new[] { container };

        var propertyType = CreatePropertyType(name: "Test Property", alias: "testProperty", containerKey: container.Key);
        createModel.Properties = new[] { propertyType };

        var result = await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted
        var mediaType = await MediaTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(mediaType);
        Assert.AreEqual(1, mediaType.PropertyGroups.Count);
        Assert.AreEqual(1, mediaType.PropertyTypes.Count());
        Assert.AreEqual(1, mediaType.PropertyGroups.First().PropertyTypes!.Count);
        Assert.AreEqual("testProperty", mediaType.PropertyTypes.First().Alias);
        Assert.AreEqual("testProperty", mediaType.PropertyGroups.First().PropertyTypes!.First().Alias);
        Assert.IsEmpty(mediaType.NoGroupPropertyTypes);
    }

    [Test]
    public async Task Can_Create_As_Child()
    {
        var parentProperty = CreatePropertyType("Parent Property", "parentProperty");
        var parentModel = CreateCreateModel(
            name: "Parent",
            propertyTypes: new[] { parentProperty });

        var parentResult = await MediaTypeEditingService.CreateAsync(parentModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(parentResult.Success);

        var childProperty = CreatePropertyType("Child Property", "childProperty");
        var parentKey = parentResult.Result!.Key;
        Composition[] composition =
        {
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = parentKey,
            },
        };

        var childModel = CreateCreateModel(
            name: "Child",
            propertyTypes: new[] { childProperty },
            compositions: composition);

        var result = await MediaTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);

        var mediaType = result.Result!;

        Assert.AreEqual(parentResult.Result.Id, mediaType.ParentId);
        Assert.AreEqual(1, mediaType.ContentTypeComposition.Count());
        Assert.AreEqual(parentResult.Result.Key, mediaType.ContentTypeComposition.FirstOrDefault()?.Key);
        Assert.AreEqual(2, mediaType.CompositionPropertyTypes.Count());
        Assert.IsTrue(mediaType.CompositionPropertyTypes.Any(x => x.Alias == parentProperty.Alias));
        Assert.IsTrue(mediaType.CompositionPropertyTypes.Any(x => x.Alias == childProperty.Alias));
    }
}
