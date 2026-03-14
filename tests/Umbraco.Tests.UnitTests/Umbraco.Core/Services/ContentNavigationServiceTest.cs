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

/// <summary>
/// Provides unit tests for the <see cref="ContentNavigationService"/> in the <c>Umbraco.Cms.Core.Services</c> namespace.
/// </summary>
[TestFixture]
public class ContentNavigationServiceTest
{
    /// <summary>
    /// Tests that the root content node is indexed as level 1.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Root_Is_1_Indexed()
    {
        var rootKey = Guid.NewGuid();
        IEnumerable<INavigationModel> navigationNodes = [new NavigationDto {Key = rootKey, ParentId = -1, Id = 1, Trashed = false}];
        var navigationRepoMock = new Mock<INavigationRepository>();
        navigationRepoMock.Setup(x => x.GetContentNodesByObjectType(Constants.ObjectTypes.Document))
            .Returns(navigationNodes);

        var contentNavigationService = new DocumentNavigationService(GetScopeProvider(), navigationRepoMock.Object, Mock.Of<IContentTypeService>());
        await contentNavigationService.RebuildAsync();

        var success = contentNavigationService.TryGetLevel(rootKey, out var level);

        Assert.IsTrue(success);
        Assert.That(level, Is.EqualTo(1));
    }

    /// <summary>
    /// Tests that the content navigation service can correctly determine the hierarchical level of a child node within a content tree.
    /// </summary>
    /// <remarks>
    /// This test sets up a root, child, and grandchild node, then verifies that the service returns the correct level for the child node.
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
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
        navigationRepoMock.Setup(x => x.GetContentNodesByObjectType(Constants.ObjectTypes.Document))
            .Returns(navigationNodes);

        var contentNavigationService = new DocumentNavigationService(GetScopeProvider(), navigationRepoMock.Object, Mock.Of<IContentTypeService>());
        await contentNavigationService.RebuildAsync();

        var success = contentNavigationService.TryGetLevel(childKey, out var level);

        Assert.IsTrue(success);
        Assert.That(level, Is.EqualTo(2));
    }

    /// <summary>
    /// Unit test that verifies the content navigation service can correctly determine the hierarchical level of a grandchild node within a content tree.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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
        navigationRepoMock.Setup(x => x.GetContentNodesByObjectType(Constants.ObjectTypes.Document))
            .Returns(navigationNodes);

        var contentNavigationService = new DocumentNavigationService(GetScopeProvider(), navigationRepoMock.Object, Mock.Of<IContentTypeService>());
        await contentNavigationService.RebuildAsync();

        var success = contentNavigationService.TryGetLevel(grandChildKey, out var level);

        Assert.IsTrue(success);
        Assert.That(level, Is.EqualTo(3));
    }

    /// <summary>
    /// Creates and returns a mocked ICoreScopeProvider for unit testing.
    /// </summary>
    /// <returns>A mocked ICoreScopeProvider instance.</returns>
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
