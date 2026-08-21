using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class MediaCollectionPresentationFactoryTests
{
    private Mock<IUmbracoMapper> _mapper = null!;
    private Mock<IEntityService> _entityService = null!;
    private Mock<IUserService> _userService = null!;
    private MediaCollectionPresentationFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _mapper = new Mock<IUmbracoMapper>();
        _entityService = new Mock<IEntityService>();
        _userService = new Mock<IUserService>();

        _userService.Setup(x => x.GetUsersById(It.IsAny<int[]>()))
            .Returns(Enumerable.Empty<IUser>());

        _entityService.Setup(x => x.GetKeysWithChildren(It.IsAny<UmbracoObjectTypes>(), It.IsAny<IEnumerable<Guid>>()))
            .Returns(new HashSet<Guid>());

        _factory = new MediaCollectionPresentationFactory(
            _mapper.Object,
            new FlagProviderCollection(() => Enumerable.Empty<IFlagProvider>()),
            _userService.Object,
            _entityService.Object);
    }

    [Test]
    public async Task Can_Flag_Collection_Items_That_Have_Children()
    {
        // Arrange - a mixed set, so that flagging all or none is visibly wrong.
        var mediaKey1 = Guid.NewGuid();
        var mediaKey2 = Guid.NewGuid();
        var mediaKey3 = Guid.NewGuid();

        ListViewPagedModel<IMedia> mediaCollection = SetupCollection(mediaKey1, mediaKey2, mediaKey3);

        _entityService.Setup(x => x.GetKeysWithChildren(It.IsAny<UmbracoObjectTypes>(), It.IsAny<IEnumerable<Guid>>()))
            .Returns(new HashSet<Guid> { mediaKey2 });

        // Act
        List<MediaCollectionResponseModel> result = await _factory.CreateCollectionModelAsync(mediaCollection);

        // Assert
        Assert.IsFalse(result[0].HasChildren);
        Assert.IsTrue(result[1].HasChildren);
        Assert.IsFalse(result[2].HasChildren);
    }

    [Test]
    public async Task Can_Query_Has_Children_With_The_Media_Object_Type()
    {
        ListViewPagedModel<IMedia> mediaCollection = SetupCollection(Guid.NewGuid());

        await _factory.CreateCollectionModelAsync(mediaCollection);

        _entityService.Verify(
            x => x.GetKeysWithChildren(UmbracoObjectTypes.Media, It.IsAny<IEnumerable<Guid>>()),
            Times.Once);
    }

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
