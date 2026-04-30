using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class DocumentCollectionPresentationFactoryTests
{
    private Mock<IUmbracoMapper> _mapper = null!;
    private Mock<IPublicAccessService> _publicAccessService = null!;
    private Mock<IEntityService> _entityService = null!;
    private Mock<IUserService> _userService = null!;
    private DocumentCollectionPresentationFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _mapper = new Mock<IUmbracoMapper>();
        _publicAccessService = new Mock<IPublicAccessService>();
        _entityService = new Mock<IEntityService>();
        _userService = new Mock<IUserService>();

        // Default: return empty user list for any batch call
        _userService.Setup(x => x.GetUsersById(It.IsAny<int[]>()))
            .Returns(Enumerable.Empty<IUser>());

        _factory = new DocumentCollectionPresentationFactory(
            _mapper.Object,
            new FlagProviderCollection(() => Enumerable.Empty<IFlagProvider>()),
            _publicAccessService.Object,
            _entityService.Object,
            _userService.Object);
    }

    [Test]
    public async Task SetUnmappedProperties_Sets_IsProtected_Via_Batched_GetAll()
    {
        // Arrange
        var contentKey1 = Guid.NewGuid();
        var contentKey2 = Guid.NewGuid();
        var contentKey3 = Guid.NewGuid();
        var ancestorKey = Guid.NewGuid();

        // content1 path includes protected node 100, content2 path includes protected node 200
        var content1 = CreateContentMock(contentKey1, id: 10, path: "-1,100,10");
        var content2 = CreateContentMock(contentKey2, id: 20, path: "-1,200,20");
        var content3 = CreateContentMock(contentKey3, id: 30, path: "-1,300,30");

        var contentCollection = new ListViewPagedModel<IContent>
        {
            Items = new PagedModel<IContent>(3, new[] { content1, content2, content3 }),
            ListViewConfiguration = new ListViewConfiguration(),
        };

        var responseModel1 = new DocumentCollectionResponseModel { Id = contentKey1 };
        var responseModel2 = new DocumentCollectionResponseModel { Id = contentKey2 };
        var responseModel3 = new DocumentCollectionResponseModel { Id = contentKey3 };

        _mapper.Setup(m => m.MapEnumerable<IContent, DocumentCollectionResponseModel>(
                It.IsAny<IEnumerable<IContent>>(),
                It.IsAny<Action<MapperContext>>()))
            .Returns([responseModel1, responseModel2, responseModel3]);

        // Public access entries protect nodes 100 and 200 (ancestors of content1 and content2)
        _publicAccessService.Setup(x => x.GetAll())
            .Returns(new[]
            {
                CreatePublicAccessEntry(protectedNodeId: 100),
                CreatePublicAccessEntry(protectedNodeId: 200),
            });

        _entityService.Setup(x => x.GetPathKeys(It.IsAny<ITreeEntity>(), true))
            .Returns([ancestorKey]);

        // Act
        List<DocumentCollectionResponseModel> result = await _factory.CreateCollectionModelAsync(contentCollection);

        // Assert
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result[0].IsProtected, "Content 1 should be protected (ancestor 100 is protected)");
        Assert.IsTrue(result[1].IsProtected, "Content 2 should be protected (ancestor 200 is protected)");
        Assert.IsFalse(result[2].IsProtected, "Content 3 should not be protected (no protected ancestor)");

        // Verify GetAll was called once (batched) and IsProtected was never called per-item
        _publicAccessService.Verify(x => x.GetAll(), Times.Once);
        _publicAccessService.Verify(x => x.IsProtected(It.IsAny<IContent>()), Times.Never);

        // Verify ancestors are set for all items
        Assert.AreEqual(ancestorKey, result[0].Ancestors.First().Id);
        Assert.AreEqual(ancestorKey, result[1].Ancestors.First().Id);
        Assert.AreEqual(ancestorKey, result[2].Ancestors.First().Id);
    }

    [Test]
    public async Task SetUnmappedProperties_Handles_No_Public_Access_Entries()
    {
        // Arrange
        var contentKey1 = Guid.NewGuid();
        var contentKey2 = Guid.NewGuid();

        var content1 = CreateContentMock(contentKey1, id: 10, path: "-1,100,10");
        var content2 = CreateContentMock(contentKey2, id: 20, path: "-1,200,20");

        var contentCollection = new ListViewPagedModel<IContent>
        {
            Items = new PagedModel<IContent>(2, new[] { content1, content2 }),
            ListViewConfiguration = new ListViewConfiguration(),
        };

        var responseModel1 = new DocumentCollectionResponseModel { Id = contentKey1 };
        var responseModel2 = new DocumentCollectionResponseModel { Id = contentKey2 };

        _mapper.Setup(m => m.MapEnumerable<IContent, DocumentCollectionResponseModel>(
                It.IsAny<IEnumerable<IContent>>(),
                It.IsAny<Action<MapperContext>>()))
            .Returns([responseModel1, responseModel2]);

        // No public access entries exist
        _publicAccessService.Setup(x => x.GetAll())
            .Returns(Enumerable.Empty<PublicAccessEntry>());

        _entityService.Setup(x => x.GetPathKeys(It.IsAny<ITreeEntity>(), true))
            .Returns(Array.Empty<Guid>());

        // Act
        List<DocumentCollectionResponseModel> result = await _factory.CreateCollectionModelAsync(contentCollection);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsFalse(result[0].IsProtected);
        Assert.IsFalse(result[1].IsProtected);
    }

    [Test]
    public async Task SetUnmappedProperties_Skips_Items_With_No_Matching_Content()
    {
        // Arrange - response model has a key that doesn't match any content item
        var contentKey = Guid.NewGuid();
        var orphanKey = Guid.NewGuid();

        var content = CreateContentMock(contentKey, id: 10, path: "-1,100,10");

        var contentCollection = new ListViewPagedModel<IContent>
        {
            Items = new PagedModel<IContent>(1, new[] { content }),
            ListViewConfiguration = new ListViewConfiguration(),
        };

        var matchingResponseModel = new DocumentCollectionResponseModel { Id = contentKey };
        var orphanResponseModel = new DocumentCollectionResponseModel { Id = orphanKey };

        _mapper.Setup(m => m.MapEnumerable<IContent, DocumentCollectionResponseModel>(
                It.IsAny<IEnumerable<IContent>>(),
                It.IsAny<Action<MapperContext>>()))
            .Returns([matchingResponseModel, orphanResponseModel]);

        // Node 100 is protected (ancestor of content)
        _publicAccessService.Setup(x => x.GetAll())
            .Returns(new[] { CreatePublicAccessEntry(protectedNodeId: 100) });

        _entityService.Setup(x => x.GetPathKeys(It.IsAny<ITreeEntity>(), true))
            .Returns(Array.Empty<Guid>());

        // Act
        List<DocumentCollectionResponseModel> result = await _factory.CreateCollectionModelAsync(contentCollection);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result[0].IsProtected, "Matching content should be protected");
        Assert.IsFalse(result[1].IsProtected, "Orphan item should remain default (not protected)");
    }

    [Test]
    public async Task SetUnmappedProperties_Detects_Protection_On_Content_Item_Itself()
    {
        // Arrange - the content item's own ID is the protected node
        var contentKey = Guid.NewGuid();

        var content = CreateContentMock(contentKey, id: 42, path: "-1,100,42");

        var contentCollection = new ListViewPagedModel<IContent>
        {
            Items = new PagedModel<IContent>(1, new[] { content }),
            ListViewConfiguration = new ListViewConfiguration(),
        };

        var responseModel = new DocumentCollectionResponseModel { Id = contentKey };

        _mapper.Setup(m => m.MapEnumerable<IContent, DocumentCollectionResponseModel>(
                It.IsAny<IEnumerable<IContent>>(),
                It.IsAny<Action<MapperContext>>()))
            .Returns([responseModel]);

        // The item itself (node 42) is protected
        _publicAccessService.Setup(x => x.GetAll())
            .Returns(new[] { CreatePublicAccessEntry(protectedNodeId: 42) });

        _entityService.Setup(x => x.GetPathKeys(It.IsAny<ITreeEntity>(), true))
            .Returns(Array.Empty<Guid>());

        // Act
        List<DocumentCollectionResponseModel> result = await _factory.CreateCollectionModelAsync(contentCollection);

        // Assert
        Assert.IsTrue(result[0].IsProtected, "Content should be protected when its own node is the protected node");
    }

    [Test]
    public async Task SetUnmappedProperties_Computes_Ancestors_Once_For_Siblings()
    {
        // Arrange - 3 siblings under the same parent
        var contentKey1 = Guid.NewGuid();
        var contentKey2 = Guid.NewGuid();
        var contentKey3 = Guid.NewGuid();
        var ancestorKey = Guid.NewGuid();

        var content1 = CreateContentMock(contentKey1, id: 10, path: "-1,100,10");
        var content2 = CreateContentMock(contentKey2, id: 20, path: "-1,100,20");
        var content3 = CreateContentMock(contentKey3, id: 30, path: "-1,100,30");

        var contentCollection = new ListViewPagedModel<IContent>
        {
            Items = new PagedModel<IContent>(3, new[] { content1, content2, content3 }),
            ListViewConfiguration = new ListViewConfiguration(),
        };

        var responseModel1 = new DocumentCollectionResponseModel { Id = contentKey1 };
        var responseModel2 = new DocumentCollectionResponseModel { Id = contentKey2 };
        var responseModel3 = new DocumentCollectionResponseModel { Id = contentKey3 };

        _mapper.Setup(m => m.MapEnumerable<IContent, DocumentCollectionResponseModel>(
                It.IsAny<IEnumerable<IContent>>(),
                It.IsAny<Action<MapperContext>>()))
            .Returns([responseModel1, responseModel2, responseModel3]);

        _publicAccessService.Setup(x => x.GetAll())
            .Returns(Enumerable.Empty<PublicAccessEntry>());

        _entityService.Setup(x => x.GetPathKeys(It.IsAny<ITreeEntity>(), true))
            .Returns([ancestorKey]);

        // Act
        List<DocumentCollectionResponseModel> result = await _factory.CreateCollectionModelAsync(contentCollection);

        // Assert - GetPathKeys called only once, not 3 times
        _entityService.Verify(x => x.GetPathKeys(It.IsAny<ITreeEntity>(), true), Times.Once);

        // All items share the same ancestors
        Assert.AreEqual(ancestorKey, result[0].Ancestors.First().Id);
        Assert.AreEqual(ancestorKey, result[1].Ancestors.First().Id);
        Assert.AreEqual(ancestorKey, result[2].Ancestors.First().Id);
    }

    [Test]
    public async Task CreateCollectionModelAsync_Batch_Resolves_User_Names()
    {
        // Arrange - 3 content items with 2 unique creator/writer IDs
        var contentKey1 = Guid.NewGuid();
        var contentKey2 = Guid.NewGuid();
        var contentKey3 = Guid.NewGuid();

        var content1 = CreateContentMock(contentKey1, id: 10, path: "-1,100,10", creatorId: 1, writerId: 2);
        var content2 = CreateContentMock(contentKey2, id: 20, path: "-1,100,20", creatorId: 1, writerId: 3);
        var content3 = CreateContentMock(contentKey3, id: 30, path: "-1,100,30", creatorId: 2, writerId: 3);

        var contentCollection = new ListViewPagedModel<IContent>
        {
            Items = new PagedModel<IContent>(3, new[] { content1, content2, content3 }),
            ListViewConfiguration = new ListViewConfiguration(),
        };

        _mapper.Setup(m => m.MapEnumerable<IContent, DocumentCollectionResponseModel>(
                It.IsAny<IEnumerable<IContent>>(),
                It.IsAny<Action<MapperContext>>()))
            .Returns([
                new DocumentCollectionResponseModel { Id = contentKey1 },
                new DocumentCollectionResponseModel { Id = contentKey2 },
                new DocumentCollectionResponseModel { Id = contentKey3 },
            ]);

        _publicAccessService.Setup(x => x.GetAll())
            .Returns(Enumerable.Empty<PublicAccessEntry>());

        _entityService.Setup(x => x.GetPathKeys(It.IsAny<ITreeEntity>(), true))
            .Returns(Array.Empty<Guid>());

        var user1 = new Mock<IUser>();
        user1.Setup(u => u.Id).Returns(1);
        user1.Setup(u => u.Name).Returns("Alice");

        var user2 = new Mock<IUser>();
        user2.Setup(u => u.Id).Returns(2);
        user2.Setup(u => u.Name).Returns("Bob");

        var user3 = new Mock<IUser>();
        user3.Setup(u => u.Id).Returns(3);
        user3.Setup(u => u.Name).Returns("Charlie");

        _userService.Setup(x => x.GetUsersById(It.IsAny<int[]>()))
            .Returns(new[] { user1.Object, user2.Object, user3.Object });

        // Act
        await _factory.CreateCollectionModelAsync(contentCollection);

        // Assert - GetUsersById called exactly once with the 3 unique user IDs
        _userService.Verify(
            x => x.GetUsersById(It.Is<int[]>(ids =>
                ids.Length == 3 && ids.Contains(1) && ids.Contains(2) && ids.Contains(3))),
            Times.Once);

        // Per-item profile resolution should never be called
        _userService.Verify(x => x.GetProfileById(It.IsAny<int>()), Times.Never);
    }

    private static IContent CreateContentMock(Guid key, int id, string path, int creatorId = 0, int writerId = 0)
    {
        var mock = new Mock<IContent>();
        mock.Setup(c => c.Key).Returns(key);
        mock.Setup(c => c.Id).Returns(id);
        mock.Setup(c => c.Path).Returns(path);
        mock.Setup(c => c.CreatorId).Returns(creatorId);
        mock.Setup(c => c.WriterId).Returns(writerId);
        return mock.Object;
    }

    private static PublicAccessEntry CreatePublicAccessEntry(int protectedNodeId) =>
        new(Guid.NewGuid(), protectedNodeId, 0, 0, Enumerable.Empty<PublicAccessRule>());
}
