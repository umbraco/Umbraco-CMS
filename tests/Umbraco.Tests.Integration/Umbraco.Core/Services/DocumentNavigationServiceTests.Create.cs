using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentNavigationServiceTests
{
    [Test]
    public async Task Structure_Updates_When_Creating_Content_At_Root()
    {
        // Arrange
        DocumentNavigationQueryService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> initialSiblingsKeys);
        var initialRootNodeSiblingsCount = initialSiblingsKeys.Count();
        var createModel = CreateContentCreateModel("Root 2", Guid.NewGuid(), parentKey: Constants.System.RootKey);

        // Act
        var createAttempt = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Guid createdItemKey = createAttempt.Result.Content!.Key;

        // Verify that the structure has updated by checking the siblings list of the Root once again
        DocumentNavigationQueryService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> updatedSiblingsKeys);
        List<Guid> siblingsList = updatedSiblingsKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsNotEmpty(siblingsList);
            Assert.AreEqual(initialRootNodeSiblingsCount + 1, siblingsList.Count);
            Assert.AreEqual(createdItemKey, siblingsList.First());
        });
    }

    [Test]
    public async Task Structure_Updates_When_Creating_Child_Content()
    {
        // Arrange
        DocumentNavigationQueryService.TryGetChildrenKeys(Child1.Key, out IEnumerable<Guid> initialChildrenKeys);
        var initialChild1ChildrenCount = initialChildrenKeys.Count();
        var createModel = CreateContentCreateModel("Grandchild 3", Guid.NewGuid(), parentKey: Child1.Key);

        // Act
        var createAttempt = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Guid createdItemKey = createAttempt.Result.Content!.Key;

        // Verify that the structure has updated by checking the children of the Child1 node once again
        DocumentNavigationQueryService.TryGetChildrenKeys(Child1.Key, out IEnumerable<Guid> updatedChildrenKeys);
        List<Guid> childrenList = updatedChildrenKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsNotEmpty(childrenList);
            Assert.AreEqual(initialChild1ChildrenCount + 1, childrenList.Count);
            Assert.IsTrue(childrenList.Contains(createdItemKey));
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
    public async Task Creating_Child_Content_Adds_It_As_The_Last_Child(Guid? parentKey)
    {
        // Arrange
        Guid newNodeKey = Guid.NewGuid();
        var createModel = CreateContentCreateModel("Child", newNodeKey, parentKey: parentKey);

        // Act
        await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // Assert
        if (parentKey is null)
        {
            DocumentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys);
            Assert.AreEqual(newNodeKey, rootKeys.Last());
        }
        else
        {
            DocumentNavigationQueryService.TryGetChildrenKeys(parentKey.Value, out IEnumerable<Guid> childrenKeys);
            Assert.AreEqual(newNodeKey, childrenKeys.Last());
        }
    }
}
