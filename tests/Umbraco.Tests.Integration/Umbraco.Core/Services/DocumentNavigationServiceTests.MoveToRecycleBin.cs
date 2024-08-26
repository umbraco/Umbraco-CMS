using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

// TODO: also check that initial siblings count's decreased
// TODO: and that descendants are still the same (i.e. they've also been moved to recycle bin)
public partial class DocumentNavigationServiceTests
{
    [Test]
    public async Task Structure_Updates_When_Moving_Content_To_Recycle_Bin()
    {
        // Arrange
        Guid nodeToMoveToRecycleBin = Child3.Key;
        DocumentNavigationQueryService.TryGetParentKey(nodeToMoveToRecycleBin, out Guid? originalParentKey);

        // Act
        await ContentEditingService.MoveToRecycleBinAsync(nodeToMoveToRecycleBin, Constants.Security.SuperUserKey);

        // Assert
        var nodeExists = DocumentNavigationQueryService.TryGetParentKey(nodeToMoveToRecycleBin, out _); // Verify that the item is no longer in the document structure
        var nodeExistsInRecycleBin = DocumentNavigationQueryService.TryGetParentKeyInBin(nodeToMoveToRecycleBin, out Guid? updatedParentKeyInRecycleBin);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(nodeExists);
            Assert.IsTrue(nodeExistsInRecycleBin);
            Assert.AreNotEqual(originalParentKey, updatedParentKeyInRecycleBin);
            Assert.IsNull(updatedParentKeyInRecycleBin); // Verify the node's parent is now located at the root of the recycle bin (null)
        });
    }
}
