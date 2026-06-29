using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Navigation;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.Navigation;

/// <summary>
/// Tests for the per-<see cref="NavigationNode"/> ordered-children cache as exercised through
/// the service-level mutation methods on <see cref="ContentNavigationServiceBase{TContentType, TContentTypeService}"/>.
/// Verifies that service-level <c>Move</c>, <c>Add</c>, and <c>UpdateSortOrder</c> correctly
/// invalidate the per-parent ordered-children cache on every affected parent.
/// </summary>
[TestFixture]
public class ContentNavigationOrderedChildrenCacheTests
{
    private static DocumentNavigationService CreateService() =>
        new(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            Mock.Of<IContentTypeService>());

    [Test]
    public void TryGetChildrenKeys_AfterMove_InvalidatesSourceAndTargetParentOrderedChildrenCache()
    {
        // Move() drives RemoveChild on the source parent and AddChild on the target parent;
        // both calls auto-invalidate the per-parent ordered-children cache. This test targets that
        // behaviour at the service-level entry point so a future refactor of Move() that bypasses
        // AddChild/RemoveChild would fail loudly here rather than silently leaving stale order.
        DocumentNavigationService service = CreateService();
        Guid contentType = Guid.NewGuid();
        Guid sourceParent = Guid.NewGuid();
        Guid targetParent = Guid.NewGuid();
        Guid moveable = Guid.NewGuid();
        Guid targetSibling = Guid.NewGuid();
        service.Add(sourceParent, contentType);
        service.Add(targetParent, contentType);
        service.Add(moveable, contentType, sourceParent);
        service.Add(targetSibling, contentType, targetParent);

        // Prime both parents' ordered-children caches.
        Assume.That(service.TryGetChildrenKeys(sourceParent, out IEnumerable<Guid> sourcePrimed), Is.True);
        Assume.That(sourcePrimed, Is.EqualTo(new[] { moveable }));
        Assume.That(service.TryGetChildrenKeys(targetParent, out IEnumerable<Guid> targetPrimed), Is.True);
        Assume.That(targetPrimed, Is.EqualTo(new[] { targetSibling }));

        service.Move(moveable, targetParent);

        Assert.That(service.TryGetChildrenKeys(sourceParent, out IEnumerable<Guid> sourceAfter), Is.True);
        Assert.That(service.TryGetChildrenKeys(targetParent, out IEnumerable<Guid> targetAfter), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(sourceAfter, Is.Empty, "Source parent ordered-children cache must drop the moved child.");
            Assert.That(targetAfter, Does.Contain(moveable), "Target parent ordered-children cache must include the moved child.");
            Assert.That(targetAfter, Does.Contain(targetSibling), "Target parent must still see its pre-existing children.");
        });
    }

    [Test]
    public void TryGetChildrenKeys_AfterMoveToRoot_InvalidatesSourceParentOrderedChildrenCache()
    {
        // Move() with null target moves to root, so only the source parent's ordered-children
        // cache needs invalidation. Targets the asymmetric path: root has no NavigationNode, so
        // there's no "target parent" cache to worry about.
        DocumentNavigationService service = CreateService();
        Guid contentType = Guid.NewGuid();
        Guid sourceParent = Guid.NewGuid();
        Guid moveable = Guid.NewGuid();
        service.Add(sourceParent, contentType);
        service.Add(moveable, contentType, sourceParent);

        Assume.That(service.TryGetChildrenKeys(sourceParent, out IEnumerable<Guid> sourcePrimed), Is.True);
        Assume.That(sourcePrimed, Is.EqualTo(new[] { moveable }));

        service.Move(moveable, targetParentKey: null);

        Assert.That(service.TryGetChildrenKeys(sourceParent, out IEnumerable<Guid> sourceAfter), Is.True);
        Assert.That(sourceAfter, Is.Empty);
    }
}
