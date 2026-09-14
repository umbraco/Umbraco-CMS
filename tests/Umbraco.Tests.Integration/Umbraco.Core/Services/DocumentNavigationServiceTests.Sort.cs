using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class DocumentNavigationServiceTests
{
    [Test]
    public async Task Structure_Updates_When_Reversing_Children_Sort_Order()
    {
        // Arrange
        Guid nodeToSortItsChildren = Root.Key;
        DocumentNavigationQueryService.TryGetChildrenKeys(nodeToSortItsChildren, out IEnumerable<Guid> initialChildrenKeys);
        List<Guid> initialChildrenKeysList = initialChildrenKeys.ToList();

        Assert.Multiple(() =>
        {
            Assert.That(initialChildrenKeysList, Has.Count.EqualTo(3));

            // Assert initial order
            Assert.That(initialChildrenKeysList[0], Is.EqualTo(Child1.Key));
            Assert.That(initialChildrenKeysList[1], Is.EqualTo(Child2.Key));
            Assert.That(initialChildrenKeysList[2], Is.EqualTo(Child3.Key));
        });

        IEnumerable<SortingModel> sortingModels = initialChildrenKeys
            .Reverse()
            .Select((key, index) => new SortingModel { Key = key, SortOrder = index });

        // Act
        await ContentEditingService.SortAsync(nodeToSortItsChildren, sortingModels, Constants.Security.SuperUserKey);

        // Assert
        DocumentNavigationQueryService.TryGetChildrenKeys(nodeToSortItsChildren, out IEnumerable<Guid> sortedChildrenKeys);
        List<Guid> sortedChildrenKeysList = sortedChildrenKeys.ToList();

        Assert.Multiple(() =>
        {
            Assert.That(sortedChildrenKeysList[0], Is.EqualTo(Child3.Key));
            Assert.That(sortedChildrenKeysList[1], Is.EqualTo(Child2.Key));
            Assert.That(sortedChildrenKeysList[2], Is.EqualTo(Child1.Key));
        });

        var expectedChildrenKeysList = initialChildrenKeys.Reverse().ToList();

        // Check that the order matches what is expected
        Assert.That(expectedChildrenKeysList.SequenceEqual(sortedChildrenKeysList), Is.True);
    }

    [Test]
    public async Task Structure_Updates_When_Children_Have_Custom_Sort_Order()
    {
        // Arrange
        Guid node = Root.Key;
        var customSortingModels = new List<SortingModel>
        {
            new() { Key = Child2.Key, SortOrder = 0 }, // Move Child 2 to the position 1
            new() { Key = Child3.Key, SortOrder = 1 }, // Move Child 3 to the position 2
            new() { Key = Child1.Key, SortOrder = 2 }, // Move Child 1 to the position 3
        };

        // Act
        await ContentEditingService.SortAsync(node, customSortingModels, Constants.Security.SuperUserKey);

        // Assert
        DocumentNavigationQueryService.TryGetChildrenKeys(node, out IEnumerable<Guid> sortedChildrenKeys);
        List<Guid> sortedChildrenKeysList = sortedChildrenKeys.ToList();

        Assert.Multiple(() =>
        {
            Assert.That(sortedChildrenKeysList[0], Is.EqualTo(Child2.Key));
            Assert.That(sortedChildrenKeysList[1], Is.EqualTo(Child3.Key));
            Assert.That(sortedChildrenKeysList[2], Is.EqualTo(Child1.Key));
        });

        var expectedChildrenKeysList = customSortingModels
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Key)
            .ToList();

        // Check that the order matches what is expected
        Assert.That(expectedChildrenKeysList.SequenceEqual(sortedChildrenKeysList), Is.True);
    }

    [Test]
    public async Task Structure_Updates_When_Sorting_Items_At_Root()
    {
        // Arrange
        var anotherRootCreateModel = CreateContentCreateModel("Root 2", Guid.NewGuid(), parentKey: Constants.System.RootKey);
        await ContentEditingService.CreateAsync(anotherRootCreateModel, Constants.Security.SuperUserKey);
        DocumentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> initialRootKeys);

        var sortingModels = initialRootKeys
            .Reverse()
            .Select((rootKey, index) => new SortingModel { Key = rootKey, SortOrder = index });

        // Act
        await ContentEditingService.SortAsync(Constants.System.RootKey, sortingModels, Constants.Security.SuperUserKey);

        // Assert
        DocumentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> sortedRootKeys);

        var expectedRootKeysList = initialRootKeys.Reverse().ToList();

        // Check that the order matches what is expected
        Assert.That(expectedRootKeysList.SequenceEqual(sortedRootKeys), Is.True);
    }

    [Test]
    public async Task Descendants_Are_Returned_In_Correct_Order_After_Children_Are_Reordered()
    {
        // Arrange
        Guid node = Root.Key;
        DocumentNavigationQueryService.TryGetDescendantsKeys(node, out IEnumerable<Guid> initialDescendantsKeys);

        var customSortingModels = new List<SortingModel>
        {
            new() { Key = Child3.Key, SortOrder = 0 }, // Move Child 3 to the position 1
            new() { Key = Child1.Key, SortOrder = 1 }, // Move Child 1 to the position 2
            new() { Key = Child2.Key, SortOrder = 2 }, // Move Child 2 to the position 3
        };

        var expectedDescendantsOrder = new List<Guid>
        {
            Child3.Key, Grandchild4.Key, // Child 3 and its descendants
            Child1.Key, Grandchild1.Key, Grandchild2.Key, // Child 1 and its descendants
            Child2.Key, Grandchild3.Key, GreatGrandchild1.Key, // Child 2 and its descendants
        };

        // Act
        await ContentEditingService.SortAsync(node, customSortingModels, Constants.Security.SuperUserKey);

        // Assert
        DocumentNavigationQueryService.TryGetDescendantsKeys(node, out IEnumerable<Guid> updatedDescendantsKeys);
        List<Guid> updatedDescendantsKeysList = updatedDescendantsKeys.ToList();

        Assert.Multiple(() =>
        {
            Assert.That(initialDescendantsKeys.SequenceEqual(updatedDescendantsKeysList), Is.False);
            Assert.That(expectedDescendantsOrder.SequenceEqual(updatedDescendantsKeysList), Is.True);
        });
    }

    [Test]
    [TestCase(1, 2, 0, new[] { "B606E3FF-E070-4D46-8CB9-D31352029FDF", "C6173927-0C59-4778-825D-D7B9F45D8DDE" })] // Custom sort order: Child 3, Child 1, Child 2; Expected order: Child 3, Child 1
    [TestCase(0, 1, 2, new[] { "C6173927-0C59-4778-825D-D7B9F45D8DDE", "B606E3FF-E070-4D46-8CB9-D31352029FDF" })] // Custom sort order: Child 1, Child 2, Child 3; Expected order: Child 1, Child 3
    [TestCase(2, 0, 1, new[] { "B606E3FF-E070-4D46-8CB9-D31352029FDF", "C6173927-0C59-4778-825D-D7B9F45D8DDE" })] // Custom sort order: Child 2, Child 3, Child 1; Expected order: Child 3, Child 1
    public async Task Siblings_Are_Returned_In_Correct_Order_After_Sorting(int sortOrder1, int sortOrder2, int sortOrder3, string[] expectedSiblings)
    {
        // Arrange
        Guid node = Child2.Key;

        var customSortingModels = new List<SortingModel>
        {
            new() { Key = Child1.Key, SortOrder = sortOrder1 }, // Move Child 1 to the position sortOrder1
            new() { Key = Child2.Key, SortOrder = sortOrder2 }, // Move Child 2 to the position sortOrder2
            new() { Key = Child3.Key, SortOrder = sortOrder3 }, // Move Child 3 to the position sortOrder3
        };

        Guid[] expectedSiblingsOrder = Array.ConvertAll(expectedSiblings, Guid.Parse);

        // Act
        await ContentEditingService.SortAsync(Root.Key, customSortingModels, Constants.Security.SuperUserKey); // Using the parent key here

        // Assert
        DocumentNavigationQueryService.TryGetSiblingsKeys(node, out IEnumerable<Guid> sortedSiblingsKeys);
        var sortedSiblingsKeysList = sortedSiblingsKeys.ToList();

        Assert.That(expectedSiblingsOrder.SequenceEqual(sortedSiblingsKeysList), Is.True);
    }

    [Test]
    public async Task Siblings_Are_Returned_In_Correct_Order_After_Sorting_At_Root()
    {
        // Arrange
        Guid node = Root.Key;
        var anotherRootCreateModel1 = CreateContentCreateModel("Root 2", Guid.NewGuid(), parentKey: Constants.System.RootKey);
        await ContentEditingService.CreateAsync(anotherRootCreateModel1, Constants.Security.SuperUserKey);
        var anotherRootCreateModel2 = CreateContentCreateModel("Root 3", Guid.NewGuid(), parentKey: Constants.System.RootKey);
        await ContentEditingService.CreateAsync(anotherRootCreateModel2, Constants.Security.SuperUserKey);
        DocumentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> initialRootKeys);

        var sortingModels = initialRootKeys
            .Reverse()
            .Select((rootKey, index) => new SortingModel { Key = rootKey, SortOrder = index });

        var expectedSiblingsKeysList = initialRootKeys.Reverse().Where(k => k != node).ToList();

        // Act
        await ContentEditingService.SortAsync(Constants.System.RootKey, sortingModels, Constants.Security.SuperUserKey);

        // Assert
        DocumentNavigationQueryService.TryGetSiblingsKeys(node, out IEnumerable<Guid> sortedSiblingsKeys);
        var sortedSiblingsKeysList = sortedSiblingsKeys.ToList();

        Assert.That(expectedSiblingsKeysList.SequenceEqual(sortedSiblingsKeysList), Is.True);
    }
}
