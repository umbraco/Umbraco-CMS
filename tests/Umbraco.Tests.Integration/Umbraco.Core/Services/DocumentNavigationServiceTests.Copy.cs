using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentNavigationServiceTests
{
    [Test]
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", "A1B1B217-B02F-4307-862C-A5E22DB729EB")] // Grandchild 2 to itself
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 3 to Child 1
    public async Task Structure_Updates_When_Copying_Content(Guid nodeToCopy, Guid targetParentKey)
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
            Assert.IsNotNull(copiedItemParentKey);
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

    [Test]
    [TestCase(null)] // Content root
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573")] // Grandchild 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB")] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732")] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF")] // Child 3
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8")] // Grandchild 4
    public async Task Copying_Content_Adds_It_Last(Guid? parentKey)
    {
        // Act
        var copyAttempt = await ContentEditingService.CopyAsync(Grandchild1.Key, parentKey, false, true, Constants.Security.SuperUserKey);
        Guid copiedItemKey = copyAttempt.Result.Key;

        // Assert
        if (parentKey is null)
        {
            DocumentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys);
            Assert.AreEqual(copiedItemKey, rootKeys.Last());
        }
        else
        {
            DocumentNavigationQueryService.TryGetChildrenKeys(parentKey.Value, out IEnumerable<Guid> childrenKeys);
            Assert.AreEqual(copiedItemKey, childrenKeys.Last());
        }
    }
}
