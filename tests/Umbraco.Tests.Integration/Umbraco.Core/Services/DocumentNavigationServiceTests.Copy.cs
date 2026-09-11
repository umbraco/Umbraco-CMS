using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class DocumentNavigationServiceTests
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
        Assert.That(copiedItemKey, Is.Not.EqualTo(nodeToCopy));

        DocumentNavigationQueryService.TryGetParentKey(copiedItemKey, out Guid? copiedItemParentKey);

        Assert.Multiple(() =>
        {
            Assert.That(copiedItemParentKey, Is.Not.Null);
            Assert.That(copiedItemParentKey, Is.EqualTo(targetParentKey));
            Assert.That(copiedItemParentKey, Is.Not.EqualTo(sourceParentKey));
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
        Assert.That(copiedItemKey, Is.Not.EqualTo(Grandchild2.Key));

        DocumentNavigationQueryService.TryGetParentKey(copiedItemKey, out Guid? copiedItemParentKey);
        DocumentNavigationQueryService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> afterCopyRootSiblingsKeys);
        DocumentNavigationQueryService.TryGetChildrenKeys(sourceParentKey.Value, out IEnumerable<Guid> sourceParentChildrenKeys);
        List<Guid> rootSiblingsList = afterCopyRootSiblingsKeys.ToList();

        Assert.Multiple(() =>
        {
            // Verifies that the node actually has been copied
            Assert.That(copiedItemParentKey, Is.Not.EqualTo(sourceParentKey));
            Assert.That(copiedItemParentKey, Is.Null);

            // Verifies that the siblings amount has been updated after copying
            Assert.That(rootSiblingsList, Has.Count.EqualTo(initialRootSiblingsCount + 1));
            Assert.That(rootSiblingsList, Does.Contain(copiedItemKey));

            // Verifies that the node was copied and not moved
            Assert.That(sourceParentChildrenKeys, Does.Contain(Grandchild2.Key));
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
        Assert.That(copiedItemKey, Is.Not.EqualTo(Grandchild3.Key));

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
            Assert.That(copiedItemParentKey, Is.Not.EqualTo(sourceParentKey));
            Assert.That(copiedItemParentKey, Is.EqualTo(Child3.Key));
            Assert.That(child3ChildrenList, Has.Count.EqualTo(initialChild3ChildrenCount + 1));

            // Verifies that the descendant amount is the same for the original and the moved GrandChild1 node
            Assert.That(grandChild1DescendantsList, Has.Count.EqualTo(initialGrandChild1DescendentsCount));

            // Verifies that the keys are not the same
            Assert.That(copiedGreatGrandChild1.Name, Is.EqualTo(GreatGrandchild1.Name));
            Assert.That(copiedGreatGrandChild1.Key, Is.Not.EqualTo(GreatGrandchild1.Key));
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
            Assert.That(rootKeys.Last(), Is.EqualTo(copiedItemKey));
        }
        else
        {
            DocumentNavigationQueryService.TryGetChildrenKeys(parentKey.Value, out IEnumerable<Guid> childrenKeys);
            Assert.That(childrenKeys.Last(), Is.EqualTo(copiedItemKey));
        }
    }
}
