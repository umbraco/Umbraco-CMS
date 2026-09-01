using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.TestHelpers;

namespace Umbraco.Cms.Tests.Integration.Testing;

public abstract class UmbracoIntegrationTestWithMediaEditing : UmbracoIntegrationTest
{
    protected IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    protected IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    protected IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    protected MediaCreateModel RootFolder { get; private set; }

    protected int RootFolderId { get; private set; }

    protected MediaCreateModel RootImage { get; private set; }

    protected int RootImageId { get; private set; }

    protected MediaCreateModel SubFolder1 { get; private set; }

    protected int SubFolder1Id { get; private set; }

    protected MediaCreateModel SubFolder2 { get; private set; }

    protected int SubFolder2Id { get; private set; }

    protected MediaCreateModel SubImage { get; private set; }

    protected int SubImageId { get; private set; }

    protected MediaCreateModel SubTestMedia { get; private set; }

    protected int SubTestMediaId { get; private set; }

    protected MediaCreateModel SubSubImage { get; private set; }

    protected int SubSubImageId { get; private set; }

    protected MediaCreateModel SubSubFile { get; private set; }

    protected int SubSubFileId { get; private set; }

    protected MediaTypeCreateModel CustomMediaTypeCreateModel { get; private set; }

    protected IMediaType CustomMediaType { get; private set; }

    [SetUp]
    public new async Task Setup() => await CreateTestData();

    protected async Task CreateTestData()
    {
        CustomMediaTypeCreateModel = MediaTypeEditingBuilder.CreateMediaTypeWithOneProperty();
        var mediaTypeTestAttempt = await MediaTypeEditingService.CreateAsync(CustomMediaTypeCreateModel, Constants.Security.SuperUserKey);
        Assert.That(mediaTypeTestAttempt.Success, Is.True);
        CustomMediaType = mediaTypeTestAttempt.Result;

        // Gets all media types
        var folderMediaTypes = await MediaTypeEditingService.GetFolderMediaTypes(0, 10);
        var folderMediaType = folderMediaTypes.Items.FirstOrDefault(x => x.Alias == "Folder");
        var mediaTypes = MediaTypeService.GetAll();
        var mediaTypesList = mediaTypes.ToList();
        var imageMediaType = mediaTypesList.FirstOrDefault(x => x.Alias == "Image");
        imageMediaType.PropertyTypes.First().Mandatory = false;
        await MediaTypeService.UpdateAsync(imageMediaType, Constants.Security.SuperUserKey);

        // Add CustomMediaType to FolderMediaType AllowedContentTypes
        var mediaTypeUpdateHelper = new MediaTypeUpdateHelper();
        var updateModel = mediaTypeUpdateHelper.CreateMediaTypeUpdateModel(folderMediaType);
        var folderMediaTypeAllowedContentTypes = updateModel.AllowedContentTypes.ToList();
        folderMediaTypeAllowedContentTypes.Add(new ContentTypeSort(key: mediaTypeTestAttempt.Result.Key, sortOrder: folderMediaTypeAllowedContentTypes.Count, alias: CustomMediaTypeCreateModel.Alias));
        updateModel.AllowedContentTypes = folderMediaTypeAllowedContentTypes;
        var updatedFolderMediaTypeAttempt = MediaTypeEditingService.UpdateAsync(folderMediaType, updateModel, Constants.Security.SuperUserKey);
        Assert.That(updatedFolderMediaTypeAttempt.Result.Success, Is.True);

        // Create and Save RootFolder
        RootFolder = MediaEditingBuilder.CreateSimpleMedia(folderMediaType.Key, "RootFolder", null);
        var rootFolderAttempt = await MediaEditingService.CreateAsync(RootFolder, Constants.Security.SuperUserKey);
        Assert.That(rootFolderAttempt.Success, Is.True);
        if (rootFolderAttempt.Result.Content != null)
        {
            RootFolderId = rootFolderAttempt.Result.Content.Id;
        }

        // Create and Save RootImage
        RootImage = MediaEditingBuilder.CreateSimpleMedia(imageMediaType.Key, "RootImage", null);
        var rootImageAttempt = await MediaEditingService.CreateAsync(RootImage, Constants.Security.SuperUserKey);
        Assert.That(rootImageAttempt.Success, Is.True);
        if (rootImageAttempt.Result.Content != null)
        {
            RootImageId = rootImageAttempt.Result.Content.Id;
        }

        // Create and Save SubFolder1
        SubFolder1 = MediaEditingBuilder.CreateSimpleMedia(folderMediaType.Key, "SubFolder1", RootFolder.Key);
        var subFolder1Attempt = await MediaEditingService.CreateAsync(SubFolder1, Constants.Security.SuperUserKey);
        Assert.That(subFolder1Attempt.Success, Is.True);
        if (subFolder1Attempt.Result.Content != null)
        {
            SubFolder1Id = subFolder1Attempt.Result.Content.Id;
        }

        // Create and Save SubFolder2
        SubFolder2 = MediaEditingBuilder.CreateSimpleMedia(folderMediaType.Key, "SubFolder2", RootFolder.Key);
        var subFolder2Attempt = await MediaEditingService.CreateAsync(SubFolder2, Constants.Security.SuperUserKey);
        Assert.That(subFolder2Attempt.Success, Is.True);
        if (subFolder2Attempt.Result.Content != null)
        {
            SubFolder2Id = subFolder2Attempt.Result.Content.Id;
        }

        // Create and Save SubImage
        SubImage = MediaEditingBuilder.CreateSimpleMedia(imageMediaType.Key, "SubImage", RootFolder.Key);
        var subImageAttempt = await MediaEditingService.CreateAsync(SubImage, Constants.Security.SuperUserKey);
        Assert.That(subImageAttempt.Success, Is.True);
        if (subImageAttempt.Result.Content != null)
        {
            SubImageId = subImageAttempt.Result.Content.Id;
        }

        // Create and Save SubTestMedia
        SubTestMedia = MediaEditingBuilder.CreateMediaWithAProperty(mediaTypeTestAttempt.Result.Key, "SubTestMedia", RootFolder.Key, "testProperty", "This is a test");
        var subTestMediaAttempt = await MediaEditingService.CreateAsync(SubTestMedia, Constants.Security.SuperUserKey);
        Assert.That(subTestMediaAttempt.Success, Is.True);
        if (subTestMediaAttempt.Result.Content != null)
        {
            SubTestMediaId = subTestMediaAttempt.Result.Content.Id;
        }

        // Create and Save SubSubImage
        SubSubImage = MediaEditingBuilder.CreateSimpleMedia(imageMediaType.Key, "SubSubImage", SubFolder1.Key);
        var subSubImageAttempt = await MediaEditingService.CreateAsync(SubSubImage, Constants.Security.SuperUserKey);
        Assert.That(subSubImageAttempt.Success, Is.True);
        if (subSubImageAttempt.Result.Content != null)
        {
            SubSubImageId = subSubImageAttempt.Result.Content.Id;
        }

        // Create and Save SubSubFile
        SubSubFile = MediaEditingBuilder.CreateSimpleMedia(imageMediaType.Key, "SubSubFile", SubFolder1.Key);
        var subSubFileAttempt = await MediaEditingService.CreateAsync(SubSubFile, Constants.Security.SuperUserKey);
        Assert.That(subSubFileAttempt.Success, Is.True);
        if (subSubFileAttempt.Result.Content != null)
        {
            SubSubFileId = subSubFileAttempt.Result.Content.Id;
        }
    }
}
