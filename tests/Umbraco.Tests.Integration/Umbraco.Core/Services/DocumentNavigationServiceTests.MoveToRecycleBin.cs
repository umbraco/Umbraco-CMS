using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentNavigationServiceTests
{
    [Test]
    public async Task Structure_Updates_When_Moving_Content_To_Recycle_Bin()
    {
        // Arrange
        Guid nodeToMoveToRecycleBin = Child3.Key;
        Guid nodeInRecycleBin = Grandchild4.Key;
        await ContentEditingService.MoveToRecycleBinAsync(nodeInRecycleBin, Constants.Security.SuperUserKey);
        DocumentNavigationQueryService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> initialSiblingsKeys);
        var beforeMoveRecycleBinSiblingsCount = initialSiblingsKeys.Count();
        Assert.AreEqual(beforeMoveRecycleBinSiblingsCount, 0);
        DocumentNavigationQueryService.TryGetParentKey(nodeToMoveToRecycleBin, out Guid? originalParentKey);
        DocumentNavigationQueryService.TryGetDescendantsKeys(nodeToMoveToRecycleBin, out IEnumerable<Guid> initialDescendantsKeys);
        var beforeMoveDescendants = initialDescendantsKeys.ToList();
        DocumentNavigationQueryService.TryGetChildrenKeys(originalParentKey.Value, out IEnumerable<Guid> initialParentChildrenKeys);
        var beforeMoveParentSiblingsCount = initialParentChildrenKeys.Count();

        // Act
        await ContentEditingService.MoveToRecycleBinAsync(nodeToMoveToRecycleBin, Constants.Security.SuperUserKey);

        // Assert
        var nodeExists = DocumentNavigationQueryService.TryGetParentKey(nodeToMoveToRecycleBin, out _); // Verify that the item is no longer in the document structure
        var nodeExistsInRecycleBin = DocumentNavigationQueryService.TryGetParentKeyInBin(nodeToMoveToRecycleBin, out Guid? updatedParentKeyInRecycleBin);
        DocumentNavigationQueryService.TryGetDescendantsKeysInBin(nodeToMoveToRecycleBin, out IEnumerable<Guid> afterMoveDescendantsKeys);
        var afterMoveDescendants = afterMoveDescendantsKeys.ToList();
        DocumentNavigationQueryService.TryGetChildrenKeys((Guid)originalParentKey, out IEnumerable<Guid> afterMoveParentChildrenKeys);
        var afterMoveParentSiblingsCount = afterMoveParentChildrenKeys.Count();
        DocumentNavigationQueryService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> afterMoveRecycleBinSiblingsKeys);
        var afterMoveRecycleBinSiblingsCount = afterMoveRecycleBinSiblingsKeys.Count();

        Assert.Multiple(() =>
        {
            Assert.IsFalse(nodeExists);
            Assert.IsTrue(nodeExistsInRecycleBin);
            Assert.AreNotEqual(originalParentKey, updatedParentKeyInRecycleBin);

            Assert.IsNull(updatedParentKeyInRecycleBin); // Verify the node's parent is now located at the root of the recycle bin (null)
            Assert.AreEqual(beforeMoveDescendants, afterMoveDescendants);
            Assert.AreEqual(beforeMoveParentSiblingsCount - 1, afterMoveParentSiblingsCount);
            Assert.AreEqual(beforeMoveRecycleBinSiblingsCount + 1, afterMoveRecycleBinSiblingsCount);
        });
    }
}
