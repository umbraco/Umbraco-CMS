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
            Assert.That(updatedParentKey, Is.Not.Null);
            Assert.That(updatedParentKey, Is.Not.EqualTo(originalParentKey));
            Assert.That(updatedParentKey, Is.EqualTo(targetParentKey));

            // Verifies that the parent's children have been updated
            Assert.That(afterMoveInitialParentDescendants, Has.Count.EqualTo(beforeMoveInitialParentDescendants.Count - (afterMoveDescendants.Count + 1)));
            Assert.That(afterMoveTargetParentChildren, Has.Count.EqualTo(beforeMoveTargetParentChildren.Count + 1));

            // Verifies that the descendants are the same before and after the move
            Assert.That(afterMoveDescendants, Has.Count.EqualTo(beforeMoveDescendants.Count));
            Assert.That(afterMoveDescendants, Is.EqualTo(beforeMoveDescendants));
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
            Assert.That(updatedParentKey, Is.Null);
            Assert.That(updatedParentKey, Is.Not.EqualTo(originalParentKey));
            Assert.That(updatedParentKey, Is.EqualTo(targetParentKey));

            // Verifies that the parent's children have been updated
            Assert.That(afterMoveInitialParentDescendants, Has.Count.EqualTo(beforeMoveInitialParentDescendants.Count - (afterMoveDescendants.Count + 1)));
            Assert.That(afterMoveRootSiblings, Has.Count.EqualTo(beforeMoveRootSiblings.Count + 1));

            // Verifies that the descendants are the same before and after the move
            Assert.That(afterMoveDescendants, Has.Count.EqualTo(beforeMoveDescendants.Count));
            Assert.That(afterMoveDescendants, Is.EqualTo(beforeMoveDescendants));
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
            Assert.That(rootKeys.Last(), Is.EqualTo(nodeToMove));
        }
        else
        {
            MediaNavigationQueryService.TryGetChildrenKeys(targetParentKey.Value, out IEnumerable<Guid> childrenKeys);
            Assert.That(childrenKeys.Last(), Is.EqualTo(nodeToMove));
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
            Assert.That(siblingsKeysAfterMovingList, Has.Count.EqualTo(1));
            Assert.That(siblingsKeysAfterMovingList[0], Is.EqualTo(SubAlbum1.Key));
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
            Assert.That(siblingsKeysAfterCreationList, Has.Count.EqualTo(2));
            Assert.That(siblingsKeysAfterMovingList[0], Is.EqualTo(SubAlbum1.Key));
            Assert.That(siblingsKeysAfterCreationList[1], Is.EqualTo(key));
        });
    }
}
