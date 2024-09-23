using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentNavigationServiceTests
{
    [Test]
    public async Task Structure_Updates_When_Creating_Content_At_Root()
    {
        // Arrange
        DocumentNavigationQueryService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> initialSiblingsKeys);
        var initialRootNodeSiblingsCount = initialSiblingsKeys.Count();
        var createModel = CreateContentCreateModel("Root 2", Guid.NewGuid(), Constants.System.RootKey);

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
        var createModel = CreateContentCreateModel("Child1Child", Guid.NewGuid(), Child1.Key);

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
}
