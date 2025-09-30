using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class DocumentNavigationServiceTests
{
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF")] // Child 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    public async Task Structure_Updates_When_Deleting_Content(Guid nodeToDelete)
    {
        // Arrange
        DocumentNavigationQueryService.TryGetDescendantsKeys(nodeToDelete, out IEnumerable<Guid> initialDescendantsKeys);

        // Act
        // Deletes the item whether it is in the recycle bin or not
        var deleteAttempt = await ContentEditingService.DeleteAsync(nodeToDelete, Constants.Security.SuperUserKey);
        Guid deletedItemKey = deleteAttempt.Result.Key;

        // Assert
        var nodeExists = DocumentNavigationQueryService.TryGetDescendantsKeys(deletedItemKey, out _);
        var nodeExistsInRecycleBin = DocumentNavigationQueryService.TryGetDescendantsKeysInBin(nodeToDelete, out _);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(nodeToDelete, deletedItemKey);
            Assert.IsFalse(nodeExists);
            Assert.IsFalse(nodeExistsInRecycleBin);

            foreach (Guid descendant in initialDescendantsKeys)
            {
                var descendantExists = DocumentNavigationQueryService.TryGetParentKey(descendant, out _);
                Assert.IsFalse(descendantExists);

                var descendantExistsInRecycleBin = DocumentNavigationQueryService.TryGetParentKeyInBin(descendant, out _);
                Assert.IsFalse(descendantExistsInRecycleBin);
            }
        });
    }

    // TODO: Add more test cases
    [Test]
    public async Task Sort_Order_Of_Siblings_Updates_When_Deleting_Content_And_Adding_New_One()
    {
        // Arrange
        Guid nodeToDelete = Child3.Key;
        Guid node = Child1.Key;

        // Act
        await ContentEditingService.DeleteAsync(nodeToDelete, Constants.Security.SuperUserKey);

        // Assert
        DocumentNavigationQueryService.TryGetSiblingsKeys(node, out IEnumerable<Guid> siblingsKeysAfterDeletion);
        var siblingsKeysAfterDeletionList = siblingsKeysAfterDeletion.ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, siblingsKeysAfterDeletionList.Count);
            Assert.AreEqual(Child2.Key, siblingsKeysAfterDeletionList[0]);
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
