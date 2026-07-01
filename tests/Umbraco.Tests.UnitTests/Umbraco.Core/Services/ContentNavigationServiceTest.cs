using System.Data;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class ContentNavigationServiceTest
{
    [Test]
    public async Task Root_Is_1_Indexed()
    {
        var rootKey = Guid.NewGuid();
        IEnumerable<INavigationModel> navigationNodes = [new NavigationDto {Key = rootKey, ParentId = -1, Id = 1, Trashed = false}];
        var navigationRepoMock = new Mock<INavigationRepository>();
        navigationRepoMock.Setup(x => x.GetContentNodesByObjectType(It.Is<IEnumerable<Guid>>(keys => keys.Count() == 1 && keys.Contains(Constants.ObjectTypes.Document))))
            .Returns(navigationNodes);

        var contentNavigationService = new DocumentNavigationService(GetScopeProvider(), navigationRepoMock.Object, Mock.Of<IContentTypeService>());
        await contentNavigationService.RebuildAsync();

        var success = contentNavigationService.TryGetLevel(rootKey, out var level);

        Assert.IsTrue(success);
        Assert.That(level, Is.EqualTo(1));
    }

    [Test]
    public async Task Can_Count_Child()
    {
        var rootKey = Guid.NewGuid();
        var childKey = Guid.NewGuid();
        var grandChildKey = Guid.NewGuid();
        IEnumerable<INavigationModel> navigationNodes = [
            new NavigationDto {Key = rootKey, ParentId = -1, Id = 1, Trashed = false},
            new NavigationDto {Key = childKey, ParentId = 1, Id = 2, Trashed = false},
            new NavigationDto {Key = grandChildKey, ParentId = 2, Id = 3, Trashed = false}];

        var navigationRepoMock = new Mock<INavigationRepository>();
        navigationRepoMock.Setup(x => x.GetContentNodesByObjectType(It.Is<IEnumerable<Guid>>(keys => keys.Count() == 1 && keys.Contains(Constants.ObjectTypes.Document))))
            .Returns(navigationNodes);

        var contentNavigationService = new DocumentNavigationService(GetScopeProvider(), navigationRepoMock.Object, Mock.Of<IContentTypeService>());
        await contentNavigationService.RebuildAsync();

        var success = contentNavigationService.TryGetLevel(childKey, out var level);

        Assert.IsTrue(success);
        Assert.That(level, Is.EqualTo(2));
    }

    [Test]
    public async Task Can_Count_Grandchild()
    {
        var rootKey = Guid.NewGuid();
        var childKey = Guid.NewGuid();
        var grandChildKey = Guid.NewGuid();
        IEnumerable<INavigationModel> navigationNodes = [
            new NavigationDto {Key = rootKey, ParentId = -1, Id = 1, Trashed = false},
            new NavigationDto {Key = childKey, ParentId = 1, Id = 2, Trashed = false},
            new NavigationDto {Key = grandChildKey, ParentId = 2, Id = 3, Trashed = false}];

        var navigationRepoMock = new Mock<INavigationRepository>();
        navigationRepoMock.Setup(x => x.GetContentNodesByObjectType(It.Is<IEnumerable<Guid>>(keys => keys.Count() == 1 && keys.Contains(Constants.ObjectTypes.Document))))
            .Returns(navigationNodes);

        var contentNavigationService = new DocumentNavigationService(GetScopeProvider(), navigationRepoMock.Object, Mock.Of<IContentTypeService>());
        await contentNavigationService.RebuildAsync();

        var success = contentNavigationService.TryGetLevel(grandChildKey, out var level);

        Assert.IsTrue(success);
        Assert.That(level, Is.EqualTo(3));
    }

    [Test]
    public async Task Can_Get_Children_In_Sort_Order_After_Rebuild()
    {
        // Regression test for #23156 / #23230: after a rebuild, TryGetChildrenKeys must return
        // children in their persisted SortOrder, not in the order the repository yields rows.
        // The repository orders by path (parent-first), which is effectively node-id/creation
        // order; here the children are created C, B, A (ascending id) but sorted A, B, C
        // (descending id).
        var rootKey = Guid.NewGuid();
        var childCKey = Guid.NewGuid();
        var childBKey = Guid.NewGuid();
        var childAKey = Guid.NewGuid();

        // Returned in path/id order (the order the repository produces), with SortOrder reversed.
        IEnumerable<INavigationModel> navigationNodes =
        [
            new NavigationDto { Key = rootKey, ParentId = -1, Id = 1, SortOrder = 0, Trashed = false },
            new NavigationDto { Key = childCKey, ParentId = 1, Id = 2, SortOrder = 2, Trashed = false },
            new NavigationDto { Key = childBKey, ParentId = 1, Id = 3, SortOrder = 1, Trashed = false },
            new NavigationDto { Key = childAKey, ParentId = 1, Id = 4, SortOrder = 0, Trashed = false },
        ];

        var navigationRepoMock = new Mock<INavigationRepository>();
        navigationRepoMock.Setup(x => x.GetContentNodesByObjectType(Constants.ObjectTypes.Document))
            .Returns(navigationNodes);

        var contentNavigationService = new DocumentNavigationService(GetScopeProvider(), navigationRepoMock.Object, Mock.Of<IContentTypeService>());
        await contentNavigationService.RebuildAsync();

        var success = contentNavigationService.TryGetChildrenKeys(rootKey, out IEnumerable<Guid> childrenKeys);

        Assert.IsTrue(success);
        Assert.That(childrenKeys, Is.EqualTo(new[] { childAKey, childBKey, childCKey }));
    }

    [Test]
    public async Task Can_Get_Descendants_In_Sort_Order_After_Rebuild()
    {
        // #23156 calls out Descendants() specifically: descendants are walked depth-first using
        // each parent's ordered children, so a rebuild that loses SortOrder corrupts the whole
        // walk. Here both levels have SortOrder reversed relative to id/path order, so the
        // depth-first result only matches if SortOrder survives the rebuild at every level.
        var rootKey = Guid.NewGuid();
        var childBKey = Guid.NewGuid();
        var childAKey = Guid.NewGuid();
        var grandB2Key = Guid.NewGuid();
        var grandB1Key = Guid.NewGuid();
        var grandA2Key = Guid.NewGuid();
        var grandA1Key = Guid.NewGuid();

        // Supplied parent-first (as the repository's path ordering guarantees), ids ascending,
        // SortOrder reversed at each level so A sorts before B and *2 sorts before *1.
        IEnumerable<INavigationModel> navigationNodes =
        [
            new NavigationDto { Key = rootKey, ParentId = -1, Id = 1, SortOrder = 0, Trashed = false },
            new NavigationDto { Key = childBKey, ParentId = 1, Id = 2, SortOrder = 1, Trashed = false },
            new NavigationDto { Key = childAKey, ParentId = 1, Id = 3, SortOrder = 0, Trashed = false },
            new NavigationDto { Key = grandB1Key, ParentId = 2, Id = 4, SortOrder = 1, Trashed = false },
            new NavigationDto { Key = grandB2Key, ParentId = 2, Id = 5, SortOrder = 0, Trashed = false },
            new NavigationDto { Key = grandA1Key, ParentId = 3, Id = 6, SortOrder = 1, Trashed = false },
            new NavigationDto { Key = grandA2Key, ParentId = 3, Id = 7, SortOrder = 0, Trashed = false },
        ];

        var navigationRepoMock = new Mock<INavigationRepository>();
        navigationRepoMock.Setup(x => x.GetContentNodesByObjectType(Constants.ObjectTypes.Document))
            .Returns(navigationNodes);

        var contentNavigationService = new DocumentNavigationService(GetScopeProvider(), navigationRepoMock.Object, Mock.Of<IContentTypeService>());
        await contentNavigationService.RebuildAsync();

        var success = contentNavigationService.TryGetDescendantsKeys(rootKey, out IEnumerable<Guid> descendantsKeys);

        Assert.IsTrue(success);
        Assert.That(
            descendantsKeys,
            Is.EqualTo(new[] { childAKey, grandA2Key, grandA1Key, childBKey, grandB2Key, grandB1Key }));
    }

    [Test]
    public void Can_Add_Children_With_Explicit_Sort_Order()
    {
        // The cache refreshers pass the real content.SortOrder to Add(). When children are added in an
        // order that differs from their sort order — as a RefreshBranch walking descendants in path
        // order can do — the supplied sort order must be honoured rather than overwritten with the
        // insertion position. Children are added C, A, B here but with sort orders that yield A, B, C.
        var contentType = Guid.NewGuid();
        var parentKey = Guid.NewGuid();
        var childCKey = Guid.NewGuid();
        var childAKey = Guid.NewGuid();
        var childBKey = Guid.NewGuid();

        var contentNavigationService = new DocumentNavigationService(GetScopeProvider(), Mock.Of<INavigationRepository>(), Mock.Of<IContentTypeService>());

        contentNavigationService.Add(parentKey, contentType);
        contentNavigationService.Add(childCKey, contentType, parentKey, sortOrder: 2);
        contentNavigationService.Add(childAKey, contentType, parentKey, sortOrder: 0);
        contentNavigationService.Add(childBKey, contentType, parentKey, sortOrder: 1);

        var success = contentNavigationService.TryGetChildrenKeys(parentKey, out IEnumerable<Guid> childrenKeys);

        Assert.IsTrue(success);
        Assert.That(childrenKeys, Is.EqualTo(new[] { childAKey, childBKey, childCKey }));
    }

    public ICoreScopeProvider GetScopeProvider()
    {
        var mockScope = new Mock<IScope>();
        var mockScopeProvider = new Mock<ICoreScopeProvider>();
        mockScopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(mockScope.Object);

        return mockScopeProvider.Object;
    }
}
