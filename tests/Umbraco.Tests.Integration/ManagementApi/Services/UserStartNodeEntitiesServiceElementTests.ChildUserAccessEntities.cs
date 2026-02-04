using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services;

public partial class UserStartNodeEntitiesServiceElementTests
{
    [Test]
    public async Task ChildUserAccessEntities_FirstAndLastChildOfRoot_YieldsBothInFirstPage_AsAllowed()
    {
        // Child containers "C1-C1" and "C1-C10" are used as start nodes
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C1-C1"].Id, ItemsByName["C1-C10"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C1"].Key,
                0,
                3,
                BySortOrder,
                out var totalItems)
            .ToArray();

        // expected total is 2, because only two items under "C1" are allowed (note the page size is 3 for good measure)
        Assert.AreEqual(2, totalItems);
        Assert.AreEqual(2, children.Length);
        Assert.Multiple(() =>
        {
            // first and last child containers are the ones allowed
            Assert.AreEqual(ItemsByName["C1-C1"].Key, children[0].Entity.Key);
            Assert.AreEqual(ItemsByName["C1-C10"].Key, children[1].Entity.Key);

            // both are allowed (they are the actual start nodes)
            Assert.IsTrue(children[0].HasAccess);
            Assert.IsTrue(children[1].HasAccess);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_InAndOutOfScope_YieldsOnlyChildrenInScope()
    {
        // Child containers "C1-C5" and "C2-C10" are used as start nodes
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C1-C5"].Id, ItemsByName["C2-C10"].Id);
        Assert.AreEqual(2, elementStartNodePaths.Length);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C2"].Key,
                0,
                10,
                BySortOrder,
                out var totalItems)
            .ToArray();

        Assert.AreEqual(1, totalItems);
        Assert.AreEqual(1, children.Length);
        Assert.Multiple(() =>
        {
            // only the "C2-C10" container is returned, as "C1-C5" is out of scope
            Assert.AreEqual(ItemsByName["C2-C10"].Key, children[0].Entity.Key);
            Assert.IsTrue(children[0].HasAccess);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_OutOfScope_YieldsNothing()
    {
        // Child containers "C1-C5" and "C2-C10" are used as start nodes
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C1-C5"].Id, ItemsByName["C2-C10"].Id);
        Assert.AreEqual(2, elementStartNodePaths.Length);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C3"].Key,
                0,
                10,
                BySortOrder,
                out var totalItems)
            .ToArray();

        Assert.AreEqual(0, totalItems);
        Assert.AreEqual(0, children.Length);
    }

    [Test]
    public async Task ChildUserAccessEntities_SpanningMultipleResultPages_CanPaginate()
    {
        // Child containers used as start nodes
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(
            ItemsByName["C1-C1"].Id,
            ItemsByName["C1-C3"].Id,
            ItemsByName["C1-C5"].Id,
            ItemsByName["C1-C7"].Id,
            ItemsByName["C1-C9"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C1"].Key,
                0,
                2,
                BySortOrder,
                out var totalItems)
            .ToArray();

        Assert.AreEqual(5, totalItems);
        // page size is 2
        Assert.AreEqual(2, children.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ItemsByName["C1-C1"].Key, children[0].Entity.Key);
            Assert.AreEqual(ItemsByName["C1-C3"].Key, children[1].Entity.Key);
            Assert.IsTrue(children[0].HasAccess);
            Assert.IsTrue(children[1].HasAccess);
        });

        // next result page
        children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C1"].Key,
                2,
                2,
                BySortOrder,
                out totalItems)
            .ToArray();

        Assert.AreEqual(5, totalItems);
        // page size is still 2
        Assert.AreEqual(2, children.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ItemsByName["C1-C5"].Key, children[0].Entity.Key);
            Assert.AreEqual(ItemsByName["C1-C7"].Key, children[1].Entity.Key);
            Assert.IsTrue(children[0].HasAccess);
            Assert.IsTrue(children[1].HasAccess);
        });

        // next result page
        children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C1"].Key,
                4,
                2,
                BySortOrder,
                out totalItems)
            .ToArray();

        Assert.AreEqual(5, totalItems);
        // page size is still 2, but this is the last result page
        Assert.AreEqual(1, children.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ItemsByName["C1-C9"].Key, children[0].Entity.Key);
            Assert.IsTrue(children[0].HasAccess);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_ChildContainerAsStartNode_YieldsAllGrandchildren_AsAllowed()
    {
        // Child container "C3-C3" is used as start node
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C3-C3"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C3-C3"].Key,
                0,
                100,
                BySortOrder,
                out var totalItems)
            .ToArray();

        Assert.AreEqual(5, totalItems);
        Assert.AreEqual(5, children.Length);
        Assert.Multiple(() =>
        {
            // all children of "C3-C3" (which are elements) should be allowed because "C3-C3" is a start node
            foreach (var childNumber in Enumerable.Range(1, 5))
            {
                var child = children[childNumber - 1];
                Assert.AreEqual(ItemsByName[$"C3-C3-E{childNumber}"].Key, child.Entity.Key);
                Assert.IsTrue(child.HasAccess);
            }
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_ReverseStartNodeOrder_DoesNotAffectResultOrder()
    {
        // Child containers "C3-C3", "C3-C2", "C3-C1" are used as start nodes (in reverse order)
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C3-C3"].Id, ItemsByName["C3-C2"].Id, ItemsByName["C3-C1"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C3"].Key,
                0,
                10,
                BySortOrder,
                out var totalItems)
            .ToArray();
        Assert.AreEqual(3, totalItems);
        Assert.AreEqual(3, children.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ItemsByName["C3-C1"].Key, children[0].Entity.Key);
            Assert.AreEqual(ItemsByName["C3-C2"].Key, children[1].Entity.Key);
            Assert.AreEqual(ItemsByName["C3-C3"].Key, children[2].Entity.Key);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_RootContainerAsStartNode_YieldsMixedChildrenContainersAndElements_AsAllowed()
    {
        // Root container "C1" is used as start node - its children include both child containers and elements
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C1"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C1"].Key,
                0,
                100,
                BySortOrder,
                out var totalItems)
            .ToArray();

        // C1 has 10 child containers (C1-C1 through C1-C10) and 2 elements (C1-E1, C1-E2)
        Assert.AreEqual(12, totalItems);
        Assert.AreEqual(12, children.Length);
        Assert.Multiple(() =>
        {
            // All children should be allowed because "C1" is a start node
            Assert.IsTrue(children.All(c => c.HasAccess));

            // Verify we have both containers and elements in the results
            var containerChildren = children.Where(c => c.Entity.Name!.Contains("-C")).ToArray();
            var elementChildren = children.Where(c => c.Entity.Name!.Contains("-E")).ToArray();
            Assert.AreEqual(10, containerChildren.Length);
            Assert.AreEqual(2, elementChildren.Length);
        });
    }
}
