using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class MediaTypeEditingServiceTests
{
    [Test]
    public async Task Can_Update_All_Basic_Settings()
    {
        var createModel = MediaTypeCreateModel("Test Media Type", "testMediaType");
        var mediaType = (await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var updateModel = MediaTypeUpdateModel("Test updated", "testUpdated");
        updateModel.Description = "This is the Test description updated";
        updateModel.Icon = "icon icon-something-updated";
        updateModel.AllowedAsRoot = false;

        var result = await MediaTypeEditingService.UpdateAsync(mediaType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        // Ensure it's actually persisted
        mediaType = await MediaTypeService.GetAsync(result.Result!.Key);
        Assert.IsNotNull(mediaType);

        Assert.AreEqual("testUpdated", mediaType.Alias);
        Assert.AreEqual("Test updated", mediaType.Name);
        Assert.AreEqual(result.Result.Id, mediaType.Id);
        Assert.AreEqual(result.Result.Key, mediaType.Key);
        Assert.AreEqual("This is the Test description updated", mediaType.Description);
        Assert.AreEqual("icon icon-something-updated", mediaType.Icon);
        Assert.IsFalse(mediaType.AllowedAsRoot);
    }

    [Test]
    public async Task Can_Add_Allowed_Types()
    {
        var allowedOne = (await MediaTypeEditingService.CreateAsync(MediaTypeCreateModel("Allowed One", "allowedOne"), Constants.Security.SuperUserKey)).Result!;
        var allowedTwo = (await MediaTypeEditingService.CreateAsync(MediaTypeCreateModel("Allowed Two", "allowedTwo"), Constants.Security.SuperUserKey)).Result!;
        var allowedThree = (await MediaTypeEditingService.CreateAsync(MediaTypeCreateModel("Allowed Three", "allowedThree"), Constants.Security.SuperUserKey)).Result!;

        var createModel = MediaTypeCreateModel("Test", "test");
        createModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(allowedOne.Key, 10, allowedOne.Alias),
            new ContentTypeSort(allowedTwo.Key, 20, allowedTwo.Alias),
        };
        var mediaType = (await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var updateModel = MediaTypeUpdateModel("Test", "test");
        updateModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort(allowedTwo.Key, 20, allowedTwo.Alias),
            new ContentTypeSort(allowedThree.Key, 30, allowedThree.Alias),
        };

        var result = await MediaTypeEditingService.UpdateAsync(mediaType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        mediaType = await MediaTypeService.GetAsync(result.Result.Key);
        Assert.IsNotNull(mediaType);

        var allowedContentTypes = mediaType.AllowedContentTypes?.ToArray();
        Assert.IsNotNull(allowedContentTypes);
        Assert.AreEqual(2, allowedContentTypes.Length);
        Assert.IsTrue(allowedContentTypes.Any(c => c.Key == allowedTwo.Key && c.SortOrder == 0 && c.Alias == allowedTwo.Alias));
        Assert.IsTrue(allowedContentTypes.Any(c => c.Key == allowedThree.Key && c.SortOrder == 1 && c.Alias == allowedThree.Alias));
    }

    [Test]
    public async Task Can_Edit_Properties()
    {
        var createModel = MediaTypeCreateModel("Test", "test");
        var propertyType = MediaTypePropertyTypeModel("Test Property", "testProperty");
        propertyType.Description = "The description";
        createModel.Properties = new[] { propertyType };

        var mediaType = (await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;
        var originalPropertyTypeKey = mediaType.PropertyTypes.First().Key;

        var updateModel = MediaTypeUpdateModel("Test", "test");
        propertyType = MediaTypePropertyTypeModel("Test Property Updated", "testProperty", key: originalPropertyTypeKey);
        propertyType.Description = "The updated description";
        propertyType.SortOrder = 10;
        var propertyType2 = MediaTypePropertyTypeModel("Test Property 2", "testProperty2");
        propertyType2.Description = "The description 2";
        propertyType2.SortOrder = 5;
        updateModel.Properties = new[] { propertyType, propertyType2 };

        var result = await MediaTypeEditingService.UpdateAsync(mediaType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);

        // Ensure it's actually persisted
        mediaType = await MediaTypeService.GetAsync(result.Result!.Key);

        Assert.IsNotNull(mediaType);
        Assert.AreEqual(2, mediaType.PropertyTypes.Count());

        var property1 = mediaType.PropertyTypes.First();
        Assert.AreEqual("Test Property 2", property1.Name);
        Assert.AreEqual("testProperty2", property1.Alias);
        Assert.AreEqual("The description 2", property1.Description);
        Assert.AreEqual(5, property1.SortOrder);
        var property2 = mediaType.PropertyTypes.Last();
        Assert.AreEqual("Test Property Updated", property2.Name);
        Assert.AreEqual("testProperty", property2.Alias);
        Assert.AreEqual("The updated description", property2.Description);
        Assert.AreEqual(originalPropertyTypeKey, property2.Key);
        Assert.AreEqual(10, property2.SortOrder);

        Assert.AreEqual(2, mediaType.NoGroupPropertyTypes.Count());
    }

    [TestCase(Constants.Conventions.MediaTypes.File)]
    [TestCase(Constants.Conventions.MediaTypes.Folder)]
    [TestCase(Constants.Conventions.MediaTypes.Image)]
    public async Task Cannot_Change_Alias_Of_System_Media_Type(string mediaTypeAlias)
    {
        var mediaType = MediaTypeService.Get(mediaTypeAlias);
        Assert.IsNotNull(mediaType);

        var updateModel = MediaTypeUpdateModel(mediaTypeAlias, $"{mediaTypeAlias}_updated");
        var result = await MediaTypeEditingService.UpdateAsync(mediaType, updateModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentTypeOperationStatus.SystemAliasChangeNotAllowed, result.Status);
        });
    }

    [Test]
    public async Task Can_Add_Composition_To_Media_Type_That_Has_Inheriting_Child()
    {
        var composition = (await MediaTypeEditingService.CreateAsync(MediaTypeCreateModel("Composition", "composition"), Constants.Security.SuperUserKey)).Result!;
        var parent = (await MediaTypeEditingService.CreateAsync(MediaTypeCreateModel("Parent", "parent"), Constants.Security.SuperUserKey)).Result!;
        await MediaTypeEditingService.CreateAsync(
            MediaTypeCreateModel(
                "Child",
                "child",
                compositions: [new Composition { CompositionType = CompositionType.Inheritance, Key = parent.Key }]),
            Constants.Security.SuperUserKey);

        var updateModel = MediaTypeUpdateModel(
            "Parent",
            "parent",
            compositions: [new() { CompositionType = CompositionType.Composition, Key = composition.Key }]);

        var result = await MediaTypeEditingService.UpdateAsync(parent, updateModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success, result.Status.ToString());

        parent = await MediaTypeService.GetAsync(parent.Key);
        Assert.IsNotNull(parent);
        Assert.AreEqual(1, parent.ContentTypeComposition.Count());
        Assert.AreEqual(composition.Key, parent.ContentTypeComposition.Single().Key);
    }

    [Test]
    public async Task Cannot_Add_Composition_Whose_Property_Alias_Collides_With_Descendant_Own_Property()
    {
        var compositionContainer = MediaTypePropertyContainerModel();
        var composition = (await MediaTypeEditingService.CreateAsync(
            MediaTypeCreateModel(
                "Composition",
                "composition",
                propertyTypes: [MediaTypePropertyTypeModel("sharedAlias", "sharedAlias", containerKey: compositionContainer.Key)],
                containers: [compositionContainer]),
            Constants.Security.SuperUserKey)).Result!;

        var parent = (await MediaTypeEditingService.CreateAsync(MediaTypeCreateModel("Parent", "parent"), Constants.Security.SuperUserKey)).Result!;

        var childContainer = MediaTypePropertyContainerModel();
        await MediaTypeEditingService.CreateAsync(
            MediaTypeCreateModel(
                "Child",
                "child",
                propertyTypes: [MediaTypePropertyTypeModel("sharedAlias", "sharedAlias", containerKey: childContainer.Key)],
                containers: [childContainer],
                compositions: [new Composition { CompositionType = CompositionType.Inheritance, Key = parent.Key }]),
            Constants.Security.SuperUserKey);

        var updateModel = MediaTypeUpdateModel(
            "Parent",
            "parent",
            compositions: [new() { CompositionType = CompositionType.Composition, Key = composition.Key }]);

        var result = await MediaTypeEditingService.UpdateAsync(parent, updateModel, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.DuplicatePropertyTypeAlias, result.Status);
    }
}
