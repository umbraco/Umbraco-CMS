using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

// TODO: Test that the descendants of the node have also been removed from both structures
public partial class DocumentNavigationServiceTests
{
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF")] // Child 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    public async Task Structure_Updates_When_Deleting_Content(Guid nodeToDelete)
    {
        // Arrange
        DocumentNavigationService.TryGetDescendantsKeys(nodeToDelete, out IEnumerable<Guid> initialDescendantsKeys);

        // Act
        // Deletes the item whether it is in the recycle bin or not
        var deleteAttempt = await ContentEditingService.DeleteAsync(nodeToDelete, Constants.Security.SuperUserKey);
        Guid deletedItemKey = deleteAttempt.Result.Key;

        // Assert
        var nodeExists = DocumentNavigationService.TryGetDescendantsKeys(deletedItemKey, out _);
        var nodeExistsInRecycleBin = DocumentNavigationService.TryGetDescendantsKeysInBin(nodeToDelete, out _);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(nodeToDelete, deletedItemKey);
            Assert.IsFalse(nodeExists);
            Assert.IsFalse(nodeExistsInRecycleBin);

            foreach (Guid descendant in initialDescendantsKeys)
            {
                var descendantExists = DocumentNavigationService.TryGetParentKey(descendant, out _);
                Assert.IsFalse(descendantExists);

                var descendantExistsInRecycleBin = DocumentNavigationService.TryGetParentKeyInBin(descendant, out _);
                Assert.IsFalse(descendantExistsInRecycleBin);
            }
        });
    }
}
