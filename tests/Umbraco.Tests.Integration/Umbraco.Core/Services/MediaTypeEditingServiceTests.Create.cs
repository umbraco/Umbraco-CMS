using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class MediaTypeEditingServiceTests
{
    [Test]
    public async Task Can_Create_With_All_Basic_Settings()
    {
        var createModel = MediaTypeCreateModel("Test Media Type", "testMediaType");
        createModel.Description = "This is the Test description";
        createModel.Icon = "icon icon-something";
        createModel.AllowedAsRoot = true;
        var result = await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // Ensure it's actually persisted
        var mediaType = await MediaTypeService.GetAsync(result.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(mediaType, Is.Not.Null);
            Assert.That(mediaType.Alias, Is.EqualTo("testMediaType"));
            Assert.That(mediaType.Name, Is.EqualTo("Test Media Type"));
            Assert.That(mediaType.Id, Is.EqualTo(result.Result.Id));
            Assert.That(mediaType.Key, Is.EqualTo(result.Result.Key));
            Assert.That(mediaType.Description, Is.EqualTo("This is the Test description"));
            Assert.That(mediaType.Icon, Is.EqualTo("icon icon-something"));
            Assert.That(mediaType.AllowedAsRoot, Is.True);
        });
    }

    [Test]
    public async Task Can_Create_In_A_Folder()
    {
        var containerResult = MediaTypeService.CreateContainer(Constants.System.Root, Guid.NewGuid(), "Test folder");
        Assert.That(containerResult.Success, Is.True);
        var container = containerResult.Result?.Entity;
        Assert.That(container, Is.Not.Null);

        var createModel = MediaTypeCreateModel("Test", "test", containerKey: container.Key);
        var result = await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted in the folder
        var mediaType = await MediaTypeService.GetAsync(result.Result!.Key);
        Assert.That(mediaType, Is.Not.Null);
        Assert.That(mediaType.ParentId, Is.EqualTo(container.Id));
    }

    [Test]
    public async Task Can_Create_With_Properties_In_A_Container()
    {
        var createModel = MediaTypeCreateModel("Test", "test");
        var container = MediaTypePropertyContainerModel();
        createModel.Containers = new[] { container };

        var propertyType = MediaTypePropertyTypeModel(name: "Test Property", alias: "testProperty", containerKey: container.Key);
        createModel.Properties = new[] { propertyType };

        var result = await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        var mediaType = await MediaTypeService.GetAsync(result.Result!.Key);

        Assert.That(mediaType, Is.Not.Null);
        Assert.That(mediaType.PropertyGroups, Has.Count.EqualTo(1));
        Assert.That(mediaType.PropertyTypes.Count(), Is.EqualTo(1));
        Assert.That(mediaType.PropertyGroups.First().PropertyTypes!, Has.Count.EqualTo(1));
        Assert.That(mediaType.PropertyTypes.First().Alias, Is.EqualTo("testProperty"));
        Assert.That(mediaType.PropertyGroups.First().PropertyTypes!.First().Alias, Is.EqualTo("testProperty"));
        Assert.That(mediaType.NoGroupPropertyTypes, Is.Empty);
    }

    [Test]
    public async Task Can_Create_As_Child()
    {
        var parentProperty = MediaTypePropertyTypeModel("Parent Property", "parentProperty");
        var parentModel = MediaTypeCreateModel(
            name: "Parent",
            propertyTypes: new[] { parentProperty });

        var parentResult = await MediaTypeEditingService.CreateAsync(parentModel, Constants.Security.SuperUserKey);
        Assert.That(parentResult.Success, Is.True);

        var childProperty = MediaTypePropertyTypeModel("Child Property", "childProperty");
        var parentKey = parentResult.Result!.Key;
        Composition[] composition =
        {
            new()
            {
                CompositionType = CompositionType.Inheritance, Key = parentKey,
            },
        };

        var childModel = MediaTypeCreateModel(
            name: "Child",
            propertyTypes: new[] { childProperty },
            compositions: composition);

        var result = await MediaTypeEditingService.CreateAsync(childModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);

        var mediaType = result.Result!;

        Assert.That(mediaType.ParentId, Is.EqualTo(parentResult.Result.Id));
        Assert.That(mediaType.ContentTypeComposition.Count(), Is.EqualTo(1));
        Assert.That(mediaType.ContentTypeComposition.FirstOrDefault()?.Key, Is.EqualTo(parentResult.Result.Key));
        Assert.That(mediaType.CompositionPropertyTypes.Count(), Is.EqualTo(2));
        Assert.That(mediaType.CompositionPropertyTypes.Any(x => x.Alias == parentProperty.Alias), Is.True);
        Assert.That(mediaType.CompositionPropertyTypes.Any(x => x.Alias == childProperty.Alias), Is.True);
    }

    [Test]
    public async Task Can_Create_Composite()
    {
        var compositionBase = MediaTypeCreateModel("Composition Base");

        // Let's add a property to ensure that it passes through
        var container = MediaTypePropertyContainerModel();
        compositionBase.Containers = new[] { container };

        var compositionProperty = MediaTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: container.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await MediaTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(compositionResult.Success, Is.True);
        var compositionType = compositionResult.Result;

        // Create media type using the composition
        var createModel = MediaTypeCreateModel(
            compositions: new[]
            {
                new Composition { CompositionType = CompositionType.Composition, Key = compositionType.Key, },
            });

        var result = await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var mediaType = await MediaTypeService.GetAsync(result.Result!.Key);

        Assert.Multiple(() =>
        {
            Assert.That(mediaType.ContentTypeComposition.Count(), Is.EqualTo(1));
            Assert.That(mediaType.ContentTypeComposition.First().Key, Is.EqualTo(compositionType.Key));
            Assert.That(compositionType.CompositionPropertyGroups.Count(), Is.EqualTo(1));
            Assert.That(compositionType.CompositionPropertyGroups.First().Key, Is.EqualTo(container.Key));
            Assert.That(compositionType.CompositionPropertyTypes.Count(), Is.EqualTo(1));
            Assert.That(compositionType.CompositionPropertyTypes.First().Key, Is.EqualTo(compositionProperty.Key));
        });
    }

    [Test]
    public async Task Cannot_Create_Composite_With_ContentType()
    {
        var compositionBase = ContentTypeCreateModel("Composition Base");

        // Let's add a property to ensure that it passes through
        var container = ContentTypePropertyContainerModel();
        compositionBase.Containers = new[] { container };

        var compositionProperty = ContentTypePropertyTypeModel("Composition Property", "compositionProperty", containerKey: container.Key);
        compositionBase.Properties = new[] { compositionProperty };

        var compositionResult = await ContentTypeEditingService.CreateAsync(compositionBase, Constants.Security.SuperUserKey);
        Assert.That(compositionResult.Success, Is.True);
        var compositionType = compositionResult.Result;

        // Create media type using the composition
        var createModel = MediaTypeCreateModel(
            compositions: new[]
            {
                new Composition { CompositionType = CompositionType.Composition, Key = compositionType.Key, },
            });

        var result = await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.InvalidComposition));
    }
}
