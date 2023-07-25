using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class MediaTypeEditingServiceTests
{
    [Test]
    public async Task Can_Create_Basic_MediaType()
    {
        var createModel = CreateCreateModel("Test Media Type", "testMediaType");
        var result = await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // Ensure it's actually persisted
        var contentType = await MediaTypeService.GetAsync(result.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(contentType);
            Assert.AreEqual("testMediaType", contentType.Alias);
            Assert.AreEqual("Test Media Type", contentType.Name);
            Assert.AreEqual(result.Result.Id, contentType.Id);
            Assert.AreEqual(result.Result.Key, contentType.Key);
        });
    }

    [Test]
    public async Task Can_Create_MediaType_In_A_Folder()
    {
        var containerResult = MediaTypeService.CreateContainer(Constants.System.Root, Guid.NewGuid(), "Test folder");
        Assert.IsTrue(containerResult.Success);
        var container = containerResult.Result?.Entity;
        Assert.IsNotNull(container);

        var createModel = CreateCreateModel("Test", "test", parentKey: container.Key);
        var result = await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted in the folder
        var contentType = await MediaTypeService.GetAsync(result.Result!.Key);
        Assert.IsNotNull(contentType);
        Assert.AreEqual(container.Id, contentType.ParentId);
    }

    [Test]
    public async Task Can_Create_ContentType_With_Properties_In_A_Container()
    {
        var createModel = CreateCreateModel("Test", "test");
        var container = CreateContainer();
        createModel.Containers = new[] { container };

        var propertyType = CreatePropertyType(name: "Test Property", alias: "testProperty", containerKey: container.Key);
        createModel.Properties = new[] { propertyType };

        var result = await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted
        var contentType = await MediaTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(contentType);
        Assert.AreEqual(1, contentType.PropertyGroups.Count);
        Assert.AreEqual(1, contentType.PropertyTypes.Count());
        Assert.AreEqual(1, contentType.PropertyGroups.First().PropertyTypes!.Count);
        Assert.AreEqual("testProperty", contentType.PropertyTypes.First().Alias);
        Assert.AreEqual("testProperty", contentType.PropertyGroups.First().PropertyTypes!.First().Alias);
        Assert.IsEmpty(contentType.NoGroupPropertyTypes);
    }

    [Test]
    public async Task Can_Create_Child_MediaType()
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

        var contentType = result.Result!;

        Assert.AreEqual(parentResult.Result.Id, contentType.ParentId);
        Assert.AreEqual(1, contentType.ContentTypeComposition.Count());
        Assert.AreEqual(parentResult.Result.Key, contentType.ContentTypeComposition.FirstOrDefault()?.Key);
        Assert.AreEqual(2, contentType.CompositionPropertyTypes.Count());
        Assert.IsTrue(contentType.CompositionPropertyTypes.Any(x => x.Alias == parentProperty.Alias));
        Assert.IsTrue(contentType.CompositionPropertyTypes.Any(x => x.Alias == childProperty.Alias));
    }
}
