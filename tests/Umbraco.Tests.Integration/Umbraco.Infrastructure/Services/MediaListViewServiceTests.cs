using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
/// Tests for the media list view service. Please notice that a lot of functional test is covered by the content list
/// view service tests, since these services share the same base implementation.
/// </summary>
public class MediaListViewServiceTests : ContentListViewServiceTestsBase
{
    private IMediaListViewService MediaListViewService => GetRequiredService<IMediaListViewService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    private IUser SuperUser { get; set; }

    [SetUp]
    public async Task Setup()
        => SuperUser = await GetSuperUser();

    [Test]
    public async Task Can_Get_List_View_Items_At_Root()
    {
        // Arrange
        CreateTenMediaItemsFromTwoMediaTypesAtRoot();
        var descendants = MediaService.GetPagedDescendants(Constants.System.Root, 0, int.MaxValue, out _);

        // Act
        var result = await MediaListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            null,
            null,
            "updateDate",
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);

            PagedModel<IMedia> collectionItemsResult = result.Result.Items;

            Assert.AreEqual(10, collectionItemsResult.Total);
            CollectionAssert.AreEquivalent(descendants, collectionItemsResult.Items);
        });
    }

    [Test]
    public async Task Can_Get_Items_With_Default_List_View_Configuration()
    {
        // Arrange
        CreateTenMediaItemsFromTwoMediaTypesAtRoot();

        // Act
        var result = await MediaListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            null,
            null,
            "updateDate",
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);

            await AssertListViewConfiguration(result.Result.ListViewConfiguration, Constants.DataTypes.Guids.ListViewMediaGuid);
        });
    }

    [Test]
    public async Task Can_Get_List_View_Items_By_Key()
    {
        // Arrange
        var root = await CreateRootMediaWithFiveChildrenAsListViewItems();
        var descendants = MediaService.GetPagedDescendants(root.Id, 0, int.MaxValue, out _);

        // Act
        var result = await MediaListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            root.Key,
            null,
            "updateDate",
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            // Assert the content type is configured as list view
            Assert.IsNotNull(root.ContentType.ListView);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);

            PagedModel<IMedia> collectionItemsResult = result.Result.Items;

            Assert.AreEqual(5, collectionItemsResult.Total);
            CollectionAssert.AreEquivalent(descendants, collectionItemsResult.Items);
        });
    }

    [Test]
    public async Task Can_Only_Get_List_View_Items_That_The_User_Has_Access_To()
    {
        // Arrange
        // Media item that the user doesn't have access to
        var imageMediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.Image);
        var image = MediaBuilder.CreateMediaImage(imageMediaType, -1);
        MediaService.Save(image);

        // Media item that serves as a start node
        var album = await CreateRootMediaWithFiveChildrenAsListViewItems();

        MediaService.GetPagedChildren(Constants.System.Root, 0, int.MaxValue, out var totalChildren);

        // New user and user group
        var userGroup = new UserGroupBuilder()
            .WithAlias("test")
            .WithName("Test")
            .WithAllowedSections(new[] { "packages" })
            .WithStartMediaId(album.Id)
            .Build();
        var userGroupCreateResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        var userCreateModel = new UserCreateModel
        {
            UserName = "testUser@mail.com",
            Email = "testUser@mail.com",
            Name = "Test user",
            UserGroupKeys = new HashSet<Guid> { userGroupCreateResult.Result.Key },
        };

        var userCreateResult =
            await UserService.CreateAsync(Constants.Security.SuperUserKey, userCreateModel, true);

        // Act
        var result = await MediaListViewService.GetListViewItemsByKeyAsync(
            userCreateResult.Result.CreatedUser,
            null,
            null,
            "updateDate",
            Direction.Ascending,
            null,
            0,
            10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(2, totalChildren);

            Assert.AreEqual(1, result.Result.Items.Items.Count());
        });

    }

    private void CreateTenMediaItemsFromTwoMediaTypesAtRoot()
    {
        var mediaType1 = MediaTypeBuilder.CreateImageMediaType("Image2");
        MediaTypeService.Save(mediaType1);
        var mediaType2 = MediaTypeBuilder.CreateImageMediaType("Image3");
        MediaTypeService.Save(mediaType2);

        for (var i = 0; i < 5; i++)
        {
            var m1 = MediaBuilder.CreateMediaImage(mediaType1, -1);
            MediaService.Save(m1);
            var m2 = MediaBuilder.CreateMediaImage(mediaType2, -1);
            MediaService.Save(m2);
        }
    }

    private async Task<IMedia> CreateRootMediaWithFiveChildrenAsListViewItems(Guid? listViewDataTypeKey = null)
    {
        var childImageMediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.Image);
        childImageMediaType.PropertyTypes.First(x => x.Alias == "umbracoFile").Mandatory = false;
        MediaTypeService.Save(childImageMediaType);

        var mediaTypeWithListView = new MediaTypeBuilder()
            .WithAlias("album")
            .WithName("Album")
            .WithIsContainer(listViewDataTypeKey ?? Constants.DataTypes.Guids.ListViewMediaGuid)
            .Build();

        mediaTypeWithListView.AllowedAsRoot = true;
        mediaTypeWithListView.AllowedContentTypes = new[]
        {
            new ContentTypeSort(childImageMediaType.Key, 1, childImageMediaType.Alias),
        };
        MediaTypeService.Save(mediaTypeWithListView);

        var rootContentCreateModel = new MediaCreateModel
        {
            ContentTypeKey = mediaTypeWithListView.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Album",
        };

        var result = await MediaEditingService.CreateAsync(rootContentCreateModel, Constants.Security.SuperUserKey);
        var root = result.Result.Content;

        for (var i = 1; i < 6; i++)
        {
            var createModel = new MediaCreateModel
            {
                ContentTypeKey = childImageMediaType.Key,
                ParentKey = root.Key,
                InvariantName = $"Image {i}",
                Key = i.ToGuid(),
            };

            await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        }

        return root;
    }
}
