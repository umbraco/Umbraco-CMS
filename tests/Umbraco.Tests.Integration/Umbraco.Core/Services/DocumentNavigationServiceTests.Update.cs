using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentNavigationServiceTests
{
    [Test]
    public async Task Structure_Does_Not_Update_When_Updating_Content()
    {
        // Arrange
        Guid nodeToUpdate = Root.Key;

        // Capture initial state
        DocumentNavigationService.TryGetParentKey(nodeToUpdate, out Guid? initialParentKey);
        DocumentNavigationService.TryGetChildrenKeys(nodeToUpdate, out IEnumerable<Guid> initialChildrenKeys);
        DocumentNavigationService.TryGetDescendantsKeys(nodeToUpdate, out IEnumerable<Guid> initialDescendantsKeys);
        DocumentNavigationService.TryGetAncestorsKeys(nodeToUpdate, out IEnumerable<Guid> initialAncestorsKeys);
        DocumentNavigationService.TryGetSiblingsKeys(nodeToUpdate, out IEnumerable<Guid> initialSiblingsKeys);

        var updateModel = new ContentUpdateModel
        {
            InvariantName = "Updated Root",
        };

        // Act
        var updateAttempt = await ContentEditingService.UpdateAsync(nodeToUpdate, updateModel, Constants.Security.SuperUserKey);
        Guid updatedItemKey = updateAttempt.Result.Content!.Key;

        // Capture updated state
        var nodeExists = DocumentNavigationService.TryGetParentKey(updatedItemKey, out Guid? updatedParentKey);
        DocumentNavigationService.TryGetChildrenKeys(updatedItemKey, out IEnumerable<Guid> childrenKeysAfterUpdate);
        DocumentNavigationService.TryGetDescendantsKeys(updatedItemKey, out IEnumerable<Guid> descendantsKeysAfterUpdate);
        DocumentNavigationService.TryGetAncestorsKeys(updatedItemKey, out IEnumerable<Guid> ancestorsKeysAfterUpdate);
        DocumentNavigationService.TryGetSiblingsKeys(updatedItemKey, out IEnumerable<Guid> siblingsKeysAfterUpdate);

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
