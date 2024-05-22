using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

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
    public async Task Folder_Media_Types_Must_Have_Allowed_Content_Types()
    {
        var imageMediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.Image);

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
