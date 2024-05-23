using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class MediaTypeEditingServiceTests
{
    [Test]
    public async Task Can_Get_Default_Folder_Media_Type()
    {
        var folderMediaTypes = await MediaTypeEditingService.GetFolderMediaTypes( 0, 100);
        Assert.AreEqual(1, folderMediaTypes.Total);
        Assert.AreEqual(Constants.Conventions.MediaTypes.Folder, folderMediaTypes.Items.First().Alias);
    }

    [Test]
    public async Task Can_Yield_Multiple_Folder_Media_Types()
    {
        var imageMediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.Image);

        var createModel = MediaTypeCreateModel("Test Media Type", "testMediaType");
        createModel.Description = "This is the Test description";
        createModel.Icon = "icon icon-something";
        createModel.AllowedAsRoot = true;
        createModel.Properties = [];
        createModel.AllowedContentTypes = new[]
        {
            new ContentTypeSort { Alias = imageMediaType.Alias, Key = imageMediaType.Key }
        };

        await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        var folderMediaTypes = await MediaTypeEditingService.GetFolderMediaTypes( 0, 100);
        Assert.AreEqual(2, folderMediaTypes.Total);
        Assert.Multiple(() =>
        {
            var aliases = folderMediaTypes.Items.Select(i => i.Alias).ToArray();
            Assert.IsTrue(aliases.Contains(Constants.Conventions.MediaTypes.Folder));
            Assert.IsTrue(aliases.Contains("testMediaType"));
        });
    }

    [Test]
    public async Task System_Folder_Media_Type_Is_Always_Included()
    {
        // update the system "Folder" media type so it does not pass the conventions for a "folder" media type
        // - remove all allowed child content types
        // - add an "umbracoFile" property
        var systemFolderMediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.Folder)!;
        var updateModel = MediaTypeUpdateModel(Constants.Conventions.MediaTypes.Folder, Constants.Conventions.MediaTypes.Folder);
        updateModel.Properties = new[]
        {
            MediaTypePropertyTypeModel("Test Property", Constants.Conventions.Media.File)
        };
        updateModel.AllowedContentTypes = [];

        var updateResult = await MediaTypeEditingService.UpdateAsync(systemFolderMediaType, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(updateResult.Success);

        // despite the system "Folder" media type no longer living up to the "folder" media type requirements,
        // it should still be considered a "folder"
        var folderMediaTypes = await MediaTypeEditingService.GetFolderMediaTypes( 0, 100);
        Assert.AreEqual(1, folderMediaTypes.Total);
        Assert.AreEqual(Constants.Conventions.MediaTypes.Folder, folderMediaTypes.Items.First().Alias);
    }

    [Test]
    public async Task Folder_Media_Types_Must_Have_Allowed_Content_Types()
    {
        var createModel = MediaTypeCreateModel("Test Media Type", "testMediaType");
        createModel.Description = "This is the Test description";
        createModel.Icon = "icon icon-something";
        createModel.AllowedAsRoot = true;
        createModel.Properties = [];
        createModel.AllowedContentTypes = [];

        await MediaTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        var folderMediaTypes = await MediaTypeEditingService.GetFolderMediaTypes( 0, 100);
        Assert.AreEqual(1, folderMediaTypes.Total);
        Assert.AreEqual(Constants.Conventions.MediaTypes.Folder, folderMediaTypes.Items.First().Alias);
    }
}
