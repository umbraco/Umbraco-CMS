using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentNavigationServiceTests
{
    [Test]
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", "60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Grandchild 1 to Child 2
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", null)] // Child 3 to content root
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 2 to Child 1
    public async Task Structure_Updates_When_Moving_Content(Guid nodeToMove, Guid? targetParentKey)
    {
        // Arrange
        DocumentNavigationService.TryGetParentKey(nodeToMove, out Guid? originalParentKey);

        // Act
        var moveAttempt = await ContentEditingService.MoveAsync(nodeToMove, targetParentKey, Constants.Security.SuperUserKey);

        // Verify the node's new parent is updated
        DocumentNavigationService.TryGetParentKey(moveAttempt.Result!.Key, out Guid? updatedParentKey);

        // Assert
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
        });
    }
}
