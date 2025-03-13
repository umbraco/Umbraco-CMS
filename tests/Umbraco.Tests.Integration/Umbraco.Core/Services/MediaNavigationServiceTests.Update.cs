using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class MediaNavigationServiceTests
{
    [Test]
    public async Task Structure_Does_Not_Update_When_Updating_Media()
    {
        // Arrange
        Guid nodeToUpdate = Album.Key;

        // Capture initial state
        MediaNavigationQueryService.TryGetParentKey(nodeToUpdate, out Guid? initialParentKey);
        MediaNavigationQueryService.TryGetChildrenKeys(nodeToUpdate, out IEnumerable<Guid> initialChildrenKeys);
        MediaNavigationQueryService.TryGetDescendantsKeys(nodeToUpdate, out IEnumerable<Guid> initialDescendantsKeys);
        MediaNavigationQueryService.TryGetAncestorsKeys(nodeToUpdate, out IEnumerable<Guid> initialAncestorsKeys);
        MediaNavigationQueryService.TryGetSiblingsKeys(nodeToUpdate, out IEnumerable<Guid> initialSiblingsKeys);

        var updateModel = new MediaUpdateModel
        {
            InvariantName = "Updated Album",
        };

        // Act
        var updateAttempt = await MediaEditingService.UpdateAsync(nodeToUpdate, updateModel, Constants.Security.SuperUserKey);
        Guid updatedItemKey = updateAttempt.Result.Content!.Key;

        // Capture updated state
        var nodeExists = MediaNavigationQueryService.TryGetParentKey(updatedItemKey, out Guid? updatedParentKey);
        MediaNavigationQueryService.TryGetChildrenKeys(updatedItemKey, out IEnumerable<Guid> childrenKeysAfterUpdate);
        MediaNavigationQueryService.TryGetDescendantsKeys(updatedItemKey, out IEnumerable<Guid> descendantsKeysAfterUpdate);
        MediaNavigationQueryService.TryGetAncestorsKeys(updatedItemKey, out IEnumerable<Guid> ancestorsKeysAfterUpdate);
        MediaNavigationQueryService.TryGetSiblingsKeys(updatedItemKey, out IEnumerable<Guid> siblingsKeysAfterUpdate);

        // Assert
        Assert.Multiple(() =>
        {
            // Verify that the item is still present in the navigation structure
            Assert.IsTrue(nodeExists);
            Assert.AreEqual(nodeToUpdate, updatedItemKey);

            // Verify that nothing's changed
            Assert.AreEqual(initialParentKey, updatedParentKey);
            CollectionAssert.AreEquivalent(initialChildrenKeys, childrenKeysAfterUpdate);
            CollectionAssert.AreEquivalent(initialDescendantsKeys, descendantsKeysAfterUpdate);
            CollectionAssert.AreEquivalent(initialAncestorsKeys, ancestorsKeysAfterUpdate);
            CollectionAssert.AreEquivalent(initialSiblingsKeys, siblingsKeysAfterUpdate);
        });
    }
}
