using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class DocumentNavigationServiceTests
{
    [TestCase(1, TestName = "Structure_Can_Rebuild")]
    [TestCase(2, TestName = "Structure_Can_Rebuild_MultipleTimes")]
    public async Task Structure_Can_Rebuild(int numberOfRebuilds)
    {
        // Arrange
        Guid nodeKey = Root.Key;

        // Capture original built state of DocumentNavigationQueryService
        DocumentNavigationQueryService.TryGetParentKey(nodeKey, out Guid? originalParentKey);
        DocumentNavigationQueryService.TryGetChildrenKeys(nodeKey, out IEnumerable<Guid> originalChildrenKeys);
        DocumentNavigationQueryService.TryGetDescendantsKeys(nodeKey, out IEnumerable<Guid> originalDescendantsKeys);
        DocumentNavigationQueryService.TryGetAncestorsKeys(nodeKey, out IEnumerable<Guid> originalAncestorsKeys);
        DocumentNavigationQueryService.TryGetSiblingsKeys(nodeKey, out IEnumerable<Guid> originalSiblingsKeys);
        DocumentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> originalRouteKeys);

        // In-memory navigation structure is empty here
        var newDocumentNavigationService = new DocumentNavigationService(
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<INavigationRepository>(),
            GetRequiredService<IContentTypeService>());
        var initialNodeExists = newDocumentNavigationService.TryGetParentKey(nodeKey, out _);

        // Act
        for (int i = 0; i < numberOfRebuilds; i++)
        {
            await newDocumentNavigationService.RebuildAsync();
        }

        // Capture rebuilt state
        var nodeExists = newDocumentNavigationService.TryGetParentKey(nodeKey, out Guid? parentKeyFromRebuild);
        newDocumentNavigationService.TryGetChildrenKeys(nodeKey, out IEnumerable<Guid> childrenKeysFromRebuild);
        newDocumentNavigationService.TryGetDescendantsKeys(nodeKey, out IEnumerable<Guid> descendantsKeysFromRebuild);
        newDocumentNavigationService.TryGetAncestorsKeys(nodeKey, out IEnumerable<Guid> ancestorsKeysFromRebuild);
        newDocumentNavigationService.TryGetSiblingsKeys(nodeKey, out IEnumerable<Guid> siblingsKeysFromRebuild);
        newDocumentNavigationService.TryGetRootKeys(out IEnumerable<Guid> routeKeysFromRebuild);

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
            CollectionAssert.AreEquivalent(originalRouteKeys, routeKeysFromRebuild);
        });
    }

    [TestCase(1, TestName = "Bin_Structure_Can_Rebuild")]
    [TestCase(2, TestName = "Bin_Structure_Can_Rebuild_MultipleTimes")]
    public async Task Bin_Structure_Can_Rebuild(int numberOfRebuilds)
    {
        // Arrange
        Guid nodeKey = Root.Key;
        await ContentEditingService.MoveToRecycleBinAsync(nodeKey, Constants.Security.SuperUserKey);

        // Capture original built state of DocumentNavigationQueryService
        DocumentNavigationQueryService.TryGetParentKeyInBin(nodeKey, out Guid? originalParentKey);
        DocumentNavigationQueryService.TryGetChildrenKeysInBin(nodeKey, out IEnumerable<Guid> originalChildrenKeys);
        DocumentNavigationQueryService.TryGetDescendantsKeysInBin(nodeKey, out IEnumerable<Guid> originalDescendantsKeys);
        DocumentNavigationQueryService.TryGetAncestorsKeysInBin(nodeKey, out IEnumerable<Guid> originalAncestorsKeys);
        DocumentNavigationQueryService.TryGetSiblingsKeysInBin(nodeKey, out IEnumerable<Guid> originalSiblingsKeys);
        DocumentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> originalRouteKeys);

        // In-memory navigation structure is empty here
        var newDocumentNavigationService = new DocumentNavigationService(
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<INavigationRepository>(),
            GetRequiredService<IContentTypeService>());
        var initialNodeExists = newDocumentNavigationService.TryGetParentKeyInBin(nodeKey, out _);

        // Act
        for (int i = 0; i < numberOfRebuilds; i++)
        {
            await newDocumentNavigationService.RebuildBinAsync();
        }

        // Capture rebuilt state
        var nodeExists = newDocumentNavigationService.TryGetParentKeyInBin(nodeKey, out Guid? parentKeyFromRebuild);
        newDocumentNavigationService.TryGetChildrenKeysInBin(nodeKey, out IEnumerable<Guid> childrenKeysFromRebuild);
        newDocumentNavigationService.TryGetDescendantsKeysInBin(nodeKey, out IEnumerable<Guid> descendantsKeysFromRebuild);
        newDocumentNavigationService.TryGetAncestorsKeysInBin(nodeKey, out IEnumerable<Guid> ancestorsKeysFromRebuild);
        newDocumentNavigationService.TryGetSiblingsKeysInBin(nodeKey, out IEnumerable<Guid> siblingsKeysFromRebuild);
        newDocumentNavigationService.TryGetRootKeys(out IEnumerable<Guid> routeKeysFromRebuild);

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
            CollectionAssert.AreEquivalent(originalRouteKeys, routeKeysFromRebuild);
        });
    }
}
