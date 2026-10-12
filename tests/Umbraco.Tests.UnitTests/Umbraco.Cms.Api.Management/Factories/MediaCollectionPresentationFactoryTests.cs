using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class MediaCollectionPresentationFactoryTests
{
    private Mock<IUmbracoMapper> _mapper = null!;
    private Mock<IUserService> _userService = null!;
    private Mock<IMediaNavigationQueryService> _navigationQueryService = null!;
    private MediaCollectionPresentationFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _mapper = new Mock<IUmbracoMapper>();
        _userService = new Mock<IUserService>();
        _navigationQueryService = new Mock<IMediaNavigationQueryService>();

        _userService.Setup(x => x.GetUsersById(It.IsAny<int[]>()))
            .Returns(Enumerable.Empty<IUser>());

        _factory = new MediaCollectionPresentationFactory(
            _mapper.Object,
            new FlagProviderCollection(() => Enumerable.Empty<IFlagProvider>()),
            _userService.Object,
            _navigationQueryService.Object);
    }

    [Test]
    public async Task PopulateHasChildren_Flags_Items_That_Have_Children()
    {
        // Arrange - a mixed set, so that flagging all or none is visibly wrong.
        var mediaKey1 = Guid.NewGuid();
        var mediaKey2 = Guid.NewGuid();
        var mediaKey3 = Guid.NewGuid();

        ListViewPagedModel<IMedia> mediaCollection = SetupCollection(mediaKey1, mediaKey2, mediaKey3);

        SetupHasChildren(mediaKey1, false);
        SetupHasChildren(mediaKey2, true);
        SetupHasChildren(mediaKey3, false);

        // Act
        List<MediaCollectionResponseModel> result = await _factory.CreateCollectionModelAsync(mediaCollection);

        // Assert
        Assert.IsFalse(result[0].HasChildren);
        Assert.IsTrue(result[1].HasChildren);
        Assert.IsFalse(result[2].HasChildren);
    }

    [Test]
    public async Task PopulateHasChildren_Flags_Trashed_Items_From_Recycle_Bin_Structure()
    {
        var mediaKey = Guid.NewGuid();

        ListViewPagedModel<IMedia> mediaCollection = SetupCollection(mediaKey);

        SetupHasChildren(mediaKey, hasChildren: false, inStructure: false);
        _navigationQueryService
            .Setup(x => x.TryGetHasChildrenInBin(mediaKey, out It.Ref<bool>.IsAny))
            .Returns((Guid _, out bool hasChildren) =>
            {
                hasChildren = true;
                return true;
            });

        List<MediaCollectionResponseModel> result = await _factory.CreateCollectionModelAsync(mediaCollection);

        Assert.IsTrue(result[0].HasChildren);
    }

    private void SetupHasChildren(Guid key, bool hasChildren, bool inStructure = true)
        => _navigationQueryService
            .Setup(x => x.TryGetHasChildren(key, out It.Ref<bool>.IsAny))
            .Returns((Guid _, out bool result) =>
            {
                result = hasChildren;
                return inStructure;
            });

    private ListViewPagedModel<IMedia> SetupCollection(params Guid[] keys)
    {
        IMedia[] items = keys.Select(CreateMediaMock).ToArray();

        _mapper.Setup(m => m.MapEnumerable<IMedia, MediaCollectionResponseModel>(
                It.IsAny<IEnumerable<IMedia>>(),
                It.IsAny<Action<MapperContext>>()))
            .Returns(keys.Select(key => new MediaCollectionResponseModel { Id = key }).ToList());

        return new ListViewPagedModel<IMedia>
        {
            Items = new PagedModel<IMedia>(items.Length, items),
            ListViewConfiguration = new ListViewConfiguration(),
        };
    }

    private static IMedia CreateMediaMock(Guid key)
    {
        var mock = new Mock<IMedia>();
        mock.Setup(m => m.Key).Returns(key);
        return mock.Object;
    }
}
