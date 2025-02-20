using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentNavigationServiceTests
{
    [Test]
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", "60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Grandchild 1 to Child 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 2 to Child 1
    public async Task Structure_Updates_When_Moving_Content(Guid nodeToMove, Guid targetParentKey)
    {
        // Arrange
        DocumentNavigationQueryService.TryGetParentKey(nodeToMove, out Guid? originalParentKey);
        DocumentNavigationQueryService.TryGetDescendantsKeys(nodeToMove, out IEnumerable<Guid> initialDescendantsKeys);
        var beforeMoveDescendants = initialDescendantsKeys.ToList();
        DocumentNavigationQueryService.TryGetChildrenKeys(originalParentKey.Value, out IEnumerable<Guid> beforeMoveInitialParentChildrenKeys);
        var beforeMoveInitialParentChildren = beforeMoveInitialParentChildrenKeys.ToList();
        DocumentNavigationQueryService.TryGetChildrenKeys(targetParentKey, out IEnumerable<Guid> beforeMoveTargetParentChildrenKeys);
        var beforeMoveTargetParentChildren = beforeMoveTargetParentChildrenKeys.ToList();

        // Act
        var moveAttempt = await ContentEditingService.MoveAsync(nodeToMove, targetParentKey, Constants.Security.SuperUserKey);

        // Verify the node's new parent is updated
        DocumentNavigationQueryService.TryGetParentKey(moveAttempt.Result!.Key, out Guid? updatedParentKey);

        // Assert
        DocumentNavigationQueryService.TryGetDescendantsKeys(nodeToMove, out IEnumerable<Guid> afterMoveDescendantsKeys);
        var afterMoveDescendants = afterMoveDescendantsKeys.ToList();
        DocumentNavigationQueryService.TryGetChildrenKeys(originalParentKey.Value, out IEnumerable<Guid> afterMoveInitialParentChildrenKeys);
        var afterMoveInitialParentChildren = afterMoveInitialParentChildrenKeys.ToList();
        DocumentNavigationQueryService.TryGetChildrenKeys(targetParentKey, out IEnumerable<Guid> afterMoveTargetParentChildrenKeys);
        var afterMoveTargetParentChildren = afterMoveTargetParentChildrenKeys.ToList();

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(updatedParentKey);
            Assert.AreNotEqual(originalParentKey, updatedParentKey);
            Assert.AreEqual(targetParentKey, updatedParentKey);

            // Verifies that the parent's children have been updated
            Assert.AreEqual(beforeMoveInitialParentChildren.Count - 1, afterMoveInitialParentChildren.Count);
            Assert.AreEqual(beforeMoveTargetParentChildren.Count + 1, afterMoveTargetParentChildren.Count);

            // Verifies that the descendants are the same before and after the move
            Assert.AreEqual(beforeMoveDescendants.Count, afterMoveDescendants.Count);
            Assert.AreEqual(beforeMoveDescendants, afterMoveDescendants);
        });
    }

    [Test]
    public async Task Structure_Updates_When_Moving_Content_To_Root()
    {
        // Arrange
        Guid nodeToMove = Child3.Key;
        Guid? targetParentKey = Constants.System.RootKey; // Root
        DocumentNavigationQueryService.TryGetParentKey(nodeToMove, out Guid? originalParentKey);
        DocumentNavigationQueryService.TryGetDescendantsKeys(nodeToMove, out IEnumerable<Guid> initialDescendantsKeys);
        var beforeMoveDescendants = initialDescendantsKeys.ToList();
        DocumentNavigationQueryService.TryGetDescendantsKeys(originalParentKey.Value, out IEnumerable<Guid> beforeMoveInitialParentDescendantsKeys);
        var beforeMoveInitialParentDescendants = beforeMoveInitialParentDescendantsKeys.ToList();

        // The Root node is the only node at the root
        DocumentNavigationQueryService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> beforeMoveSiblingsKeys);
        var beforeMoveRootSiblings = beforeMoveSiblingsKeys.ToList();

        // Act
        var moveAttempt = await ContentEditingService.MoveAsync(nodeToMove, targetParentKey, Constants.Security.SuperUserKey);

        // Verify the node's new parent is updated
        DocumentNavigationQueryService.TryGetParentKey(moveAttempt.Result!.Key, out Guid? updatedParentKey);

        // Assert
        DocumentNavigationQueryService.TryGetDescendantsKeys(nodeToMove, out IEnumerable<Guid> afterMoveDescendantsKeys);
        var afterMoveDescendants = afterMoveDescendantsKeys.ToList();
        DocumentNavigationQueryService.TryGetDescendantsKeys((Guid)originalParentKey, out IEnumerable<Guid> afterMoveInitialParentDescendantsKeys);
        var afterMoveInitialParentDescendants = afterMoveInitialParentDescendantsKeys.ToList();
        DocumentNavigationQueryService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> afterMoveSiblingsKeys);
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
    [TestCase(null)] // Content root
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB")] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732")] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF")] // Child 3
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8")] // Grandchild 4
    public async Task Moving_Content_Adds_It_Last(Guid? targetParentKey)
    {
        // Arrange
        Guid nodeToMove = Grandchild1.Key;

        // Act
        await ContentEditingService.MoveAsync(nodeToMove, targetParentKey, Constants.Security.SuperUserKey);

        // Assert
        if (targetParentKey is null)
        {
            DocumentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys);
            Assert.AreEqual(nodeToMove, rootKeys.Last());
        }
        else
        {
            DocumentNavigationQueryService.TryGetChildrenKeys(targetParentKey.Value, out IEnumerable<Guid> childrenKeys);
            Assert.AreEqual(nodeToMove, childrenKeys.Last());
        }
    }

    // TODO: Add more test cases
    [Test]
    public async Task Sort_Order_Of_Siblings_Updates_When_Moving_Content_And_Adding_New_One()
    {
        // Arrange
        Guid nodeToMove = Child3.Key;
        Guid node = Child1.Key;

        // Act
        await ContentEditingService.MoveAsync(nodeToMove, null, Constants.Security.SuperUserKey);

        // Assert
        DocumentNavigationQueryService.TryGetSiblingsKeys(node, out IEnumerable<Guid> siblingsKeysAfterMoving);
        var siblingsKeysAfterMovingList = siblingsKeysAfterMoving.ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, siblingsKeysAfterMovingList.Count);
            Assert.AreEqual(Child2.Key, siblingsKeysAfterMovingList[0]);
        });

        // Create a new sibling under the same parent
        var key = Guid.NewGuid();
        var createModel = CreateContentCreateModel("Child 4", key, parentKey: Root.Key);
        await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        DocumentNavigationQueryService.TryGetSiblingsKeys(node, out IEnumerable<Guid> siblingsKeysAfterCreation);
        var siblingsKeysAfterCreationList = siblingsKeysAfterCreation.ToList();

        // Verify sibling order after creating the new content
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, siblingsKeysAfterCreationList.Count);
            Assert.AreEqual(Child2.Key, siblingsKeysAfterCreationList[0]);
            Assert.AreEqual(key, siblingsKeysAfterCreationList[1]);
        });
    }
}
