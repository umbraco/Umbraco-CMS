using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

// TODO: test that it is added to its new parent - check parent's children
// TODO: test that it has the same amount of descendants - depending on value of includeDescendants param
// TODO: test that the number of target parent descendants updates when copying node with descendants
// TODO: test that copied node descendants have different keys than source node descendants
public partial class DocumentNavigationServiceTests
{
    [Test]
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", "A1B1B217-B02F-4307-862C-A5E22DB729EB")] // Grandchild 2 to itself
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", null)] // Child 2 to content root
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 3 to Child 1
    public async Task Structure_Updates_When_Copying_Content(Guid nodeToCopy, Guid? targetParentKey)
    {
        // Arrange
        DocumentNavigationQueryService.TryGetParentKey(nodeToCopy, out Guid? sourceParentKey);

        // Act
        var copyAttempt = await ContentEditingService.CopyAsync(nodeToCopy, targetParentKey, false, false, Constants.Security.SuperUserKey);
        Guid copiedItemKey = copyAttempt.Result.Key;

        // Assert
        Assert.AreNotEqual(nodeToCopy, copiedItemKey);

        DocumentNavigationQueryService.TryGetParentKey(copiedItemKey, out Guid? copiedItemParentKey);

        Assert.Multiple(() =>
        {
            if (targetParentKey is null)
            {
                // Verify the copied node's parent is null (it's been copied to content root)
                Assert.IsNull(copiedItemParentKey);
            }
            else
            {
                Assert.IsNotNull(copiedItemParentKey);
            }

            Assert.AreEqual(targetParentKey, copiedItemParentKey);
            Assert.AreNotEqual(sourceParentKey, copiedItemParentKey);
        });
    }
}
