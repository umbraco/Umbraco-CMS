using NUnit.Framework;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class MediaNavigationServiceTests
{
    [Test]
    public async Task Structure_Can_Rebuild()
    {
        // Arrange
        Guid nodeKey = Album.Key;

        // Capture original built state of MediaNavigationService
        MediaNavigationService.TryGetParentKey(nodeKey, out Guid? originalParentKey);
        MediaNavigationService.TryGetChildrenKeys(nodeKey, out IEnumerable<Guid> originalChildrenKeys);
        MediaNavigationService.TryGetDescendantsKeys(nodeKey, out IEnumerable<Guid> originalDescendantsKeys);
        MediaNavigationService.TryGetAncestorsKeys(nodeKey, out IEnumerable<Guid> originalAncestorsKeys);
        MediaNavigationService.TryGetSiblingsKeys(nodeKey, out IEnumerable<Guid> originalSiblingsKeys);

        // Im-memory navigation structure is empty here
        var newMediaNavigationService = new MediaNavigationService(GetRequiredService<ICoreScopeProvider>(), GetRequiredService<INavigationRepository>());
        var initialNodeExists = newMediaNavigationService.TryGetParentKey(nodeKey, out _);

        // Act
        await newMediaNavigationService.RebuildAsync();

        // Capture rebuilt state
        var nodeExists = newMediaNavigationService.TryGetParentKey(nodeKey, out Guid? parentKeyFromRebuild);
        newMediaNavigationService.TryGetChildrenKeys(nodeKey, out IEnumerable<Guid> childrenKeysFromRebuild);
        newMediaNavigationService.TryGetDescendantsKeys(nodeKey, out IEnumerable<Guid> descendantsKeysFromRebuild);
        newMediaNavigationService.TryGetAncestorsKeys(nodeKey, out IEnumerable<Guid> ancestorsKeysFromRebuild);
        newMediaNavigationService.TryGetSiblingsKeys(nodeKey, out IEnumerable<Guid> siblingsKeysFromRebuild);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(initialNodeExists);

            // Verify that the item is present in the navigation structure after a rebuild
            Assert.IsTrue(nodeExists);

            // Verify that we have the same items as in the original built state of MediaNavigationService
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
