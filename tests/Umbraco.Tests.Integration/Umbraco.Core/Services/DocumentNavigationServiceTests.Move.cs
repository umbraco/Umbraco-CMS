using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentNavigationServiceTests
{
    [Test]
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573",
        "60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Grandchild 1 to Child 2
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", null)] // Child 3 to content root
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 2 to Child 1
    public async Task Structure_Updates_When_Moving_Content(Guid nodeToMove, Guid? targetParentKey)
    {
        // Arrange

        DocumentNavigationQueryService.TryGetParentKey(nodeToMove, out Guid? originalParentKey);
        DocumentNavigationQueryService.TryGetDescendantsKeys(nodeToMove, out IEnumerable<Guid> initialDescendantsKeys);
        var beforeMoveDescendants = initialDescendantsKeys.ToList();
        DocumentNavigationQueryService.TryGetChildrenKeys((Guid)originalParentKey, out IEnumerable<Guid> beforeMoveInitialParentChildrenKeys);
        var beforeMoveInitialParentChildren = beforeMoveInitialParentChildrenKeys.ToList();
        DocumentNavigationQueryService.TryGetChildrenKeys((Guid)targetParentKey, out IEnumerable<Guid> beforeMoveTargetParentChildrenKeys);
        var beforeMoveTargetParentChildren = beforeMoveTargetParentChildrenKeys.ToList();

        // Act
        var moveAttempt = await ContentEditingService.MoveAsync(nodeToMove, targetParentKey, Constants.Security.SuperUserKey);

        // Verify the node's new parent is updated
        DocumentNavigationQueryService.TryGetParentKey(moveAttempt.Result!.Key, out Guid? updatedParentKey);

        // Assert
        DocumentNavigationQueryService.TryGetDescendantsKeys(nodeToMove, out IEnumerable<Guid> afterMoveDescendantsKeys);
        DocumentNavigationQueryService.TryGetChildrenKeys((Guid)originalParentKey, out IEnumerable<Guid> afterMoveInitialParentChildrenKeys);
        var afterMoveInitialParentChildren = afterMoveInitialParentChildrenKeys.ToList();
        DocumentNavigationQueryService.TryGetChildrenKeys((Guid)targetParentKey, out IEnumerable<Guid> afterMoveTargetParentChildrenKeys);
        var afterMoveTargetParentChildren = afterMoveTargetParentChildrenKeys.ToList();

        var afterMoveDescendants = afterMoveDescendantsKeys.ToList();

        Assert.Multiple(() =>
        {
            if (targetParentKey is null)
            {
                Assert.IsNull(updatedParentKey);
            }
            else
            {
                Assert.IsNotNull(updatedParentKey);
            }

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

    public async Task Structure_Updates_When_Moving_Content_To_Root()

    {
}

}
