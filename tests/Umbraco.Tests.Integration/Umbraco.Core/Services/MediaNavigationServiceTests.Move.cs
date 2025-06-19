using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class MediaNavigationServiceTests
{
    [Test]
    [TestCase("62BCE72F-8C18-420E-BCAC-112B5ECC95FD", "139DC977-E50F-4382-9728-B278C4B7AC6A")] // Image 4 to Sub-album 1
    [TestCase("E0B23D56-9A0E-4FC4-BD42-834B73B4C7AB", "1CD97C02-8534-4B72-AE9E-AE52EC94CF31")] // Sub-sub-album 1 to Album
    public async Task Structure_Updates_When_Moving_Media(Guid nodeToMove, Guid targetParentKey)
    {
        // Arrange
        MediaNavigationQueryService.TryGetParentKey(nodeToMove, out Guid? originalParentKey);
        MediaNavigationQueryService.TryGetDescendantsKeys(nodeToMove, out IEnumerable<Guid> initialDescendantsKeys);
        var beforeMoveDescendants = initialDescendantsKeys.ToList();
        MediaNavigationQueryService.TryGetDescendantsKeys(originalParentKey.Value, out IEnumerable<Guid> beforeMoveInitialParentDescendantsKeys);
        var beforeMoveInitialParentDescendants = beforeMoveInitialParentDescendantsKeys.ToList();
        MediaNavigationQueryService.TryGetChildrenKeys(targetParentKey, out IEnumerable<Guid> beforeMoveTargetParentChildrenKeys);
        var beforeMoveTargetParentChildren = beforeMoveTargetParentChildrenKeys.ToList();

        // Act
        var moveAttempt = await MediaEditingService.MoveAsync(nodeToMove, targetParentKey, Constants.Security.SuperUserKey);

        // Verify the node's new parent is updated
        MediaNavigationQueryService.TryGetParentKey(moveAttempt.Result!.Key, out Guid? updatedParentKey);

        // Assert
        MediaNavigationQueryService.TryGetDescendantsKeys(nodeToMove, out IEnumerable<Guid> afterMoveDescendantsKeys);
        var afterMoveDescendants = afterMoveDescendantsKeys.ToList();
        MediaNavigationQueryService.TryGetDescendantsKeys(originalParentKey.Value, out IEnumerable<Guid> afterMoveInitialParentDescendantsKeys);
        var afterMoveInitialParentDescendants = afterMoveInitialParentDescendantsKeys.ToList();
        MediaNavigationQueryService.TryGetChildrenKeys(targetParentKey, out IEnumerable<Guid> afterMoveTargetParentChildrenKeys);
        var afterMoveTargetParentChildren = afterMoveTargetParentChildrenKeys.ToList();

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(updatedParentKey);
            Assert.AreNotEqual(originalParentKey, updatedParentKey);
            Assert.AreEqual(targetParentKey, updatedParentKey);

            // Verifies that the parent's children have been updated
            Assert.AreEqual(beforeMoveInitialParentDescendants.Count - (afterMoveDescendants.Count + 1), afterMoveInitialParentDescendants.Count);
            Assert.AreEqual(beforeMoveTargetParentChildren.Count + 1, afterMoveTargetParentChildren.Count);

            // Verifies that the descendants are the same before and after the move
            Assert.AreEqual(beforeMoveDescendants.Count, afterMoveDescendants.Count);
            Assert.AreEqual(beforeMoveDescendants, afterMoveDescendants);
        });
    }

    [Test]
    public async Task Structure_Updates_When_Moving_Media_To_Root()
    {
        // Arrange
        Guid nodeToMove = SubAlbum2.Key;
        Guid? targetParentKey = Constants.System.RootKey;
        MediaNavigationQueryService.TryGetParentKey(nodeToMove, out Guid? originalParentKey);
        MediaNavigationQueryService.TryGetDescendantsKeys(nodeToMove, out IEnumerable<Guid> initialDescendantsKeys);
        var beforeMoveDescendants = initialDescendantsKeys.ToList();
        MediaNavigationQueryService.TryGetDescendantsKeys(originalParentKey.Value, out IEnumerable<Guid> beforeMoveInitialParentDescendantsKeys);
        var beforeMoveInitialParentDescendants = beforeMoveInitialParentDescendantsKeys.ToList();

        // The Root node is the only node at the root
        MediaNavigationQueryService.TryGetSiblingsKeys(Album.Key, out IEnumerable<Guid> beforeMoveSiblingsKeys);
        var beforeMoveRootSiblings = beforeMoveSiblingsKeys.ToList();

        // Act
        var moveAttempt = await MediaEditingService.MoveAsync(nodeToMove, targetParentKey, Constants.Security.SuperUserKey);

        // Verify the node's new parent is updated
        MediaNavigationQueryService.TryGetParentKey(moveAttempt.Result!.Key, out Guid? updatedParentKey);

        // Assert
        MediaNavigationQueryService.TryGetDescendantsKeys(nodeToMove, out IEnumerable<Guid> afterMoveDescendantsKeys);
        var afterMoveDescendants = afterMoveDescendantsKeys.ToList();
        MediaNavigationQueryService.TryGetDescendantsKeys((Guid)originalParentKey, out IEnumerable<Guid> afterMoveInitialParentDescendantsKeys);
        var afterMoveInitialParentDescendants = afterMoveInitialParentDescendantsKeys.ToList();
        MediaNavigationQueryService.TryGetSiblingsKeys(Album.Key, out IEnumerable<Guid> afterMoveSiblingsKeys);
        var afterMoveRootSiblings = afterMoveSiblingsKeys.ToList();

        Assert.Multiple(() =>
        {
            Assert.IsNull(updatedParentKey);
            Assert.AreNotEqual(originalParentKey, updatedParentKey);
            Assert.AreEqual(targetParentKey, updatedParentKey);

            // Verifies that the parent's children have been updated
            Assert.AreEqual(beforeMoveInitialParentDescendants.Count - (afterMoveDescendants.Count + 1), afterMoveInitialParentDescendants.Count);
            Assert.AreEqual(beforeMoveRootSiblings.Count + 1, afterMoveRootSiblings.Count);

            // Verifies that the descendants are the same before and after the move
            Assert.AreEqual(beforeMoveDescendants.Count, afterMoveDescendants.Count);
            Assert.AreEqual(beforeMoveDescendants, afterMoveDescendants);
        });
    }

    [Test]
    [TestCase(null)] // Media root
    [TestCase("1CD97C02-8534-4B72-AE9E-AE52EC94CF31")] // Album
    [TestCase("DBCAFF2F-BFA4-4744-A948-C290C432D564")] // Sub-album 2
    [TestCase("E0B23D56-9A0E-4FC4-BD42-834B73B4C7AB")] // Sub-sub-album 1
    public async Task Moving_Media_Adds_It_Last(Guid? targetParentKey)
    {
        // Arrange
        Guid nodeToMove = Image2.Key;

        // Act
        await MediaEditingService.MoveAsync(nodeToMove, targetParentKey, Constants.Security.SuperUserKey);

        // Assert
        if (targetParentKey is null)
        {
            MediaNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys);
            Assert.AreEqual(nodeToMove, rootKeys.Last());
        }
        else
        {
            MediaNavigationQueryService.TryGetChildrenKeys(targetParentKey.Value, out IEnumerable<Guid> childrenKeys);
            Assert.AreEqual(nodeToMove, childrenKeys.Last());
        }
    }

    // TODO: Add more test cases
    [Test]
    public async Task Sort_Order_Of_Siblings_Updates_When_Moving_Media_And_Adding_New_One()
    {
        // Arrange
        Guid nodeToMove = SubAlbum2.Key;
        Guid node = Image1.Key;

        // Act
        await MediaEditingService.MoveAsync(nodeToMove, null, Constants.Security.SuperUserKey);

        // Assert
        MediaNavigationQueryService.TryGetSiblingsKeys(node, out IEnumerable<Guid> siblingsKeysAfterMoving);
        var siblingsKeysAfterMovingList = siblingsKeysAfterMoving.ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, siblingsKeysAfterMovingList.Count);
            Assert.AreEqual(SubAlbum1.Key, siblingsKeysAfterMovingList[0]);
        });

        // Create a new sibling under the same parent
        var key = Guid.NewGuid();
        var createModel = CreateMediaCreateModel("Child Image", key, ImageMediaType.Key, Album.Key);
        await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        MediaNavigationQueryService.TryGetSiblingsKeys(node, out IEnumerable<Guid> siblingsKeysAfterCreation);
        var siblingsKeysAfterCreationList = siblingsKeysAfterCreation.ToList();

        // Verify sibling order after creating the new media
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, siblingsKeysAfterCreationList.Count);
            Assert.AreEqual(SubAlbum1.Key, siblingsKeysAfterMovingList[0]);
            Assert.AreEqual(key, siblingsKeysAfterCreationList[1]);
        });
    }
}
