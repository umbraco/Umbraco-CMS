using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

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

    [Test]
    public async Task Structure_Updates_When_Copying_Content_To_Root()
    {
        // Arrange
        DocumentNavigationQueryService.TryGetParentKey(Grandchild2.Key, out Guid? sourceParentKey);
        DocumentNavigationQueryService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> beforeCopyRootSiblingsKeys);
        var initialRootSiblingsCount = beforeCopyRootSiblingsKeys.Count();

        // Act
        var copyAttempt = await ContentEditingService.CopyAsync(Grandchild2.Key, null, false, false, Constants.Security.SuperUserKey);
        Guid copiedItemKey = copyAttempt.Result.Key;

        // Assert
        Assert.AreNotEqual(Grandchild2.Key, copiedItemKey);
        DocumentNavigationQueryService.TryGetParentKey(copiedItemKey, out Guid? copiedItemParentKey);
        DocumentNavigationQueryService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> afterCopyRootSiblingsKeys);
        DocumentNavigationQueryService.TryGetChildrenKeys(sourceParentKey.Value, out IEnumerable<Guid> sourceParentChildrenKeys);
        List<Guid> rootSiblingsList = afterCopyRootSiblingsKeys.ToList();

        Assert.Multiple(() =>
        {
            // Verifies that the node actually has been copied
            Assert.AreNotEqual(sourceParentKey, copiedItemParentKey);
            Assert.IsNull(copiedItemParentKey);

            // Verifies that the siblings amount has been updated after copying
            Assert.AreEqual(initialRootSiblingsCount + 1, rootSiblingsList.Count);
            Assert.IsTrue(rootSiblingsList.Contains(copiedItemKey));

            // Verifies that the node was copied and not moved
            Assert.IsTrue(sourceParentChildrenKeys.Contains(Grandchild2.Key));
        });
    }

    [Test]
    public async Task Structure_Updates_When_Copying_Content_With_Descendants()
    {
        // Arrange
        DocumentNavigationQueryService.TryGetParentKey(Grandchild3.Key, out Guid? sourceParentKey);
        DocumentNavigationQueryService.TryGetDescendantsKeys(Grandchild3.Key, out IEnumerable<Guid> beforeCopyGrandChild1Descendents);
        DocumentNavigationQueryService.TryGetChildrenKeys(Child3.Key, out IEnumerable<Guid> beforeCopyChild3ChildrenKeys);
        var initialChild3ChildrenCount = beforeCopyChild3ChildrenKeys.Count();
        var initialGrandChild1DescendentsCount = beforeCopyGrandChild1Descendents.Count();

        // Act
        var copyAttempt = await ContentEditingService.CopyAsync(Grandchild3.Key, Child3.Key, false, true, Constants.Security.SuperUserKey);
        Guid copiedItemKey = copyAttempt.Result.Key;

        // Assert
        Assert.AreNotEqual(Grandchild3.Key, copiedItemKey);
        DocumentNavigationQueryService.TryGetParentKey(copiedItemKey, out Guid? copiedItemParentKey);
        DocumentNavigationQueryService.TryGetChildrenKeys(Child3.Key, out IEnumerable<Guid> afterCopyChild3ChildrenKeys);
        DocumentNavigationQueryService.TryGetChildrenKeys(copiedItemKey, out IEnumerable<Guid> afterCopyGrandChild1Descendents);
        List<Guid> child3ChildrenList = afterCopyChild3ChildrenKeys.ToList();
        List<Guid> grandChild1DescendantsList = afterCopyGrandChild1Descendents.ToList();

        // Retrieves the child of the copied item to check its content
        var copiedGreatGrandChild1 = await ContentEditingService.GetAsync(grandChild1DescendantsList.First());

        Assert.Multiple(() =>
        {
            // Verifies that the node actually has been copied
            Assert.AreNotEqual(sourceParentKey, copiedItemParentKey);
            Assert.AreEqual(Child3.Key, copiedItemParentKey);
            Assert.AreEqual(initialChild3ChildrenCount + 1, child3ChildrenList.Count);

            // Verifies that the descendant amount is the same for the original and the moved GrandChild1 node
            Assert.AreEqual(initialGrandChild1DescendentsCount, grandChild1DescendantsList.Count);

            // Verifies that the keys are not the same
            Assert.AreEqual(GreatGrandchild1.Name, copiedGreatGrandChild1.Name);
            Assert.AreNotEqual(GreatGrandchild1.Key, copiedGreatGrandChild1.Key);
        });
    }
}
