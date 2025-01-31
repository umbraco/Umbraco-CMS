using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class MediaNavigationServiceTests : MediaNavigationServiceTestsBase
{
    [SetUp]
    public async Task Setup()
    {
        // Album
        //    - Image 1
        //    - Sub-album 1
        //      - Image 2
        //      - Image 3
        //    - Sub-album 2
        //      - Sub-sub-album 1
        //        - Image 4

        // Media Types
        FolderMediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.Folder);
        ImageMediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.Image);
        ImageMediaType.PropertyTypes.First(x => x.Alias == "umbracoFile").Mandatory = false;
        MediaTypeService.Save(ImageMediaType);

        // Media
        var albumModel = CreateMediaCreateModel("Album", new Guid("1CD97C02-8534-4B72-AE9E-AE52EC94CF31"), FolderMediaType.Key);
        var albumCreateAttempt = await MediaEditingService.CreateAsync(albumModel, Constants.Security.SuperUserKey);
        Album = albumCreateAttempt.Result.Content!;

        var image1Model = CreateMediaCreateModel("Image 1", new Guid("03976EBE-A942-4F24-9885-9186E99AEF7C"), ImageMediaType.Key, Album.Key);
        var image1CreateAttempt = await MediaEditingService.CreateAsync(image1Model, Constants.Security.SuperUserKey);
        Image1 = image1CreateAttempt.Result.Content!;

        var subAlbum1Model = CreateMediaCreateModel("Sub-album 1", new Guid("139DC977-E50F-4382-9728-B278C4B7AC6A"), FolderMediaType.Key, Album.Key);
        var subAlbum1CreateAttempt = await MediaEditingService.CreateAsync(subAlbum1Model, Constants.Security.SuperUserKey);
        SubAlbum1 = subAlbum1CreateAttempt.Result.Content!;

        var image2Model = CreateMediaCreateModel("Image 2", new Guid("3E489C32-9315-42DA-95CE-823D154B09C8"), ImageMediaType.Key, SubAlbum1.Key);
        var image2CreateAttempt = await MediaEditingService.CreateAsync(image2Model, Constants.Security.SuperUserKey);
        Image2 = image2CreateAttempt.Result.Content!;

        var image3Model = CreateMediaCreateModel("Image 3", new Guid("6176BD70-2CD2-4AEE-A045-084C94E4AFF2"), ImageMediaType.Key, SubAlbum1.Key);
        var image3CreateAttempt = await MediaEditingService.CreateAsync(image3Model, Constants.Security.SuperUserKey);
        Image3 = image3CreateAttempt.Result.Content!;

        var subAlbum2Model = CreateMediaCreateModel("Sub-album 2", new Guid("DBCAFF2F-BFA4-4744-A948-C290C432D564"), FolderMediaType.Key, Album.Key);
        var subAlbum2CreateAttempt = await MediaEditingService.CreateAsync(subAlbum2Model, Constants.Security.SuperUserKey);
        SubAlbum2 = subAlbum2CreateAttempt.Result.Content!;

        var subSubAlbum1Model = CreateMediaCreateModel("Sub-sub-album 1", new Guid("E0B23D56-9A0E-4FC4-BD42-834B73B4C7AB"), FolderMediaType.Key, SubAlbum2.Key);
        var subSubAlbum1CreateAttempt = await MediaEditingService.CreateAsync(subSubAlbum1Model, Constants.Security.SuperUserKey);
        SubSubAlbum1 = subSubAlbum1CreateAttempt.Result.Content!;

        var image4Model = CreateMediaCreateModel("Image 4", new Guid("62BCE72F-8C18-420E-BCAC-112B5ECC95FD"), ImageMediaType.Key, SubSubAlbum1.Key);
        var image4CreateAttempt = await MediaEditingService.CreateAsync(image4Model, Constants.Security.SuperUserKey);
        Image4 = image4CreateAttempt.Result.Content!;
    }

    [Test]
    public async Task Structure_Does_Not_Update_When_Scope_Is_Not_Completed()
    {
        // Arrange
        Guid notCreatedAlbumKey = new Guid("860EE748-BC7E-4A13-A1D9-C9160B25AD6E");

        // Create node at media root
        var createModel = CreateMediaCreateModel("Album 2", notCreatedAlbumKey, FolderMediaType.Key);

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        }

        // Act
        var nodeExists = MediaNavigationQueryService.TryGetParentKey(notCreatedAlbumKey, out _);

        // Assert
        Assert.IsFalse(nodeExists);
    }

    [Test]
    public void Can_Filter_Children_By_Type()
    {
        // Arrange
        MediaNavigationQueryService.TryGetChildrenKeys(Album.Key, out IEnumerable<Guid> allChildrenKeys);
        List<Guid> allChildrenList = allChildrenKeys.ToList();

        // Act
        MediaNavigationQueryService.TryGetChildrenKeysOfType(Album.Key, ImageMediaType.Alias, out IEnumerable<Guid> childrenKeysOfTypeImage);
        List<Guid> imageChildrenList = childrenKeysOfTypeImage.ToList();

        MediaNavigationQueryService.TryGetChildrenKeysOfType(Album.Key, FolderMediaType.Alias, out IEnumerable<Guid> childrenKeysOfTypeFolder);
        List<Guid> folderChildrenList = childrenKeysOfTypeFolder.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, imageChildrenList.Count);
            Assert.AreEqual(2, folderChildrenList.Count);
            Assert.AreEqual(3, allChildrenList.Count);
        });
    }
}
