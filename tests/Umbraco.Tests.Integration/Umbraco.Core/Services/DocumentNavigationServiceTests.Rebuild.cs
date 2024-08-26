using NUnit.Framework;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentNavigationServiceTests
{
    [Test]
    public async Task Structure_Can_Rebuild()
    {
        // Arrange
        Guid nodeKey = Root.Key;

        // Capture original built state of DocumentNavigationService
        DocumentNavigationQueryService.TryGetParentKey(nodeKey, out Guid? originalParentKey);
        DocumentNavigationQueryService.TryGetChildrenKeys(nodeKey, out IEnumerable<Guid> originalChildrenKeys);
        DocumentNavigationQueryService.TryGetDescendantsKeys(nodeKey, out IEnumerable<Guid> originalDescendantsKeys);
        DocumentNavigationQueryService.TryGetAncestorsKeys(nodeKey, out IEnumerable<Guid> originalAncestorsKeys);
        DocumentNavigationQueryService.TryGetSiblingsKeys(nodeKey, out IEnumerable<Guid> originalSiblingsKeys);

        // Im-memory navigation structure is empty here
        var newDocumentNavigationService = new DocumentNavigationService(GetRequiredService<ICoreScopeProvider>(), GetRequiredService<INavigationRepository>());
        var initialNodeExists = newDocumentNavigationService.TryGetParentKey(nodeKey, out _);

        // Act
        await newDocumentNavigationService.RebuildAsync();

        // Capture rebuilt state
        var nodeExists = newDocumentNavigationService.TryGetParentKey(nodeKey, out Guid? parentKeyFromRebuild);
        newDocumentNavigationService.TryGetChildrenKeys(nodeKey, out IEnumerable<Guid> childrenKeysFromRebuild);
        newDocumentNavigationService.TryGetDescendantsKeys(nodeKey, out IEnumerable<Guid> descendantsKeysFromRebuild);
        newDocumentNavigationService.TryGetAncestorsKeys(nodeKey, out IEnumerable<Guid> ancestorsKeysFromRebuild);
        newDocumentNavigationService.TryGetSiblingsKeys(nodeKey, out IEnumerable<Guid> siblingsKeysFromRebuild);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(initialNodeExists);

            // Verify that the item is present in the navigation structure after a rebuild
            Assert.IsTrue(nodeExists);

            // Verify that we have the same items as in the original built state of DocumentNavigationService
            Assert.AreEqual(originalParentKey, parentKeyFromRebuild);
            CollectionAssert.AreEquivalent(originalChildrenKeys, childrenKeysFromRebuild);
            CollectionAssert.AreEquivalent(originalDescendantsKeys, descendantsKeysFromRebuild);
            CollectionAssert.AreEquivalent(originalAncestorsKeys, ancestorsKeysFromRebuild);
            CollectionAssert.AreEquivalent(originalSiblingsKeys, siblingsKeysFromRebuild);
        });
    }

    [Test]
    // TODO: Test that you can rebuild bin structure as well
    public async Task Bin_Structure_Can_Rebuild()
    {
    }
}
