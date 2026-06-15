using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
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
        Assert.That(result.Success, Is.True);

        // Ensure it's actually persisted
        mediaType = await MediaTypeService.GetAsync(result.Result!.Key);
        Assert.That(mediaType, Is.Not.Null);

        Assert.That(mediaType.Alias, Is.EqualTo("testUpdated"));
        Assert.That(mediaType.Name, Is.EqualTo("Test updated"));
        Assert.That(mediaType.Id, Is.EqualTo(result.Result.Id));
        Assert.That(mediaType.Key, Is.EqualTo(result.Result.Key));
        Assert.That(mediaType.Description, Is.EqualTo("This is the Test description updated"));
        Assert.That(mediaType.Icon, Is.EqualTo("icon icon-something-updated"));
        Assert.That(mediaType.AllowedAsRoot, Is.False);
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        mediaType = await MediaTypeService.GetAsync(result.Result.Key);
        Assert.That(mediaType, Is.Not.Null);

        var allowedContentTypes = mediaType.AllowedContentTypes?.ToArray();
        Assert.That(allowedContentTypes, Is.Not.Null);
        Assert.That(allowedContentTypes, Has.Length.EqualTo(2));
        Assert.That(allowedContentTypes.Any(c => c.Key == allowedTwo.Key && c.SortOrder == 0 && c.Alias == allowedTwo.Alias), Is.True);
        Assert.That(allowedContentTypes.Any(c => c.Key == allowedThree.Key && c.SortOrder == 1 && c.Alias == allowedThree.Alias), Is.True);
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);

        // Ensure it's actually persisted
        mediaType = await MediaTypeService.GetAsync(result.Result!.Key);

        Assert.That(mediaType, Is.Not.Null);
        Assert.That(mediaType.PropertyTypes.Count(), Is.EqualTo(2));

        var property1 = mediaType.PropertyTypes.First();
        Assert.That(property1.Name, Is.EqualTo("Test Property 2"));
        Assert.That(property1.Alias, Is.EqualTo("testProperty2"));
        Assert.That(property1.Description, Is.EqualTo("The description 2"));
        Assert.That(property1.SortOrder, Is.EqualTo(5));
        var property2 = mediaType.PropertyTypes.Last();
        Assert.That(property2.Name, Is.EqualTo("Test Property Updated"));
        Assert.That(property2.Alias, Is.EqualTo("testProperty"));
        Assert.That(property2.Description, Is.EqualTo("The updated description"));
        Assert.That(property2.Key, Is.EqualTo(originalPropertyTypeKey));
        Assert.That(property2.SortOrder, Is.EqualTo(10));

        Assert.That(mediaType.NoGroupPropertyTypes.Count(), Is.EqualTo(2));
    }

    [TestCase(Constants.Conventions.MediaTypes.File)]
    [TestCase(Constants.Conventions.MediaTypes.Folder)]
    [TestCase(Constants.Conventions.MediaTypes.Image)]
    public async Task Cannot_Change_Alias_Of_System_Media_Type(string mediaTypeAlias)
    {
        var mediaType = MediaTypeService.Get(mediaTypeAlias);
        Assert.That(mediaType, Is.Not.Null);

        var updateModel = MediaTypeUpdateModel(mediaTypeAlias, $"{mediaTypeAlias}_updated");
        var result = await MediaTypeEditingService.UpdateAsync(mediaType, updateModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentTypeOperationStatus.SystemAliasChangeNotAllowed));
        });
    }
}
