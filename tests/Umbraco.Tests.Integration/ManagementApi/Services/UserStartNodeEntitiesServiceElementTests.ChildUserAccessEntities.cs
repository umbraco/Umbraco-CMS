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
        Assert.That(totalItems, Is.EqualTo(2));
        Assert.That(children, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            // first and last child containers are the ones allowed
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["C1-C1"].Key));
            Assert.That(children[1].Entity.Key, Is.EqualTo(ItemsByName["C1-C10"].Key));

            // both are allowed (they are the actual start nodes)
            Assert.That(children[0].HasAccess, Is.True);
            Assert.That(children[1].HasAccess, Is.True);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_InAndOutOfScope_YieldsOnlyChildrenInScope()
    {
        // Child containers "C1-C5" and "C2-C10" are used as start nodes
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C1-C5"].Id, ItemsByName["C2-C10"].Id);
        Assert.That(elementStartNodePaths, Has.Length.EqualTo(2));

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

        Assert.That(totalItems, Is.EqualTo(1));
        Assert.That(children, Has.Length.EqualTo(1));
        Assert.Multiple(() =>
        {
            // only the "C2-C10" container is returned, as "C1-C5" is out of scope
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["C2-C10"].Key));
            Assert.That(children[0].HasAccess, Is.True);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_OutOfScope_YieldsNothing()
    {
        // Child containers "C1-C5" and "C2-C10" are used as start nodes
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C1-C5"].Id, ItemsByName["C2-C10"].Id);
        Assert.That(elementStartNodePaths, Has.Length.EqualTo(2));

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

        Assert.That(totalItems, Is.EqualTo(0));
        Assert.That(children, Is.Empty);
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

        Assert.That(totalItems, Is.EqualTo(5));
        // page size is 2
        Assert.That(children, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["C1-C1"].Key));
            Assert.That(children[1].Entity.Key, Is.EqualTo(ItemsByName["C1-C3"].Key));
            Assert.That(children[0].HasAccess, Is.True);
            Assert.That(children[1].HasAccess, Is.True);
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

        Assert.That(totalItems, Is.EqualTo(5));
        // page size is still 2
        Assert.That(children, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["C1-C5"].Key));
            Assert.That(children[1].Entity.Key, Is.EqualTo(ItemsByName["C1-C7"].Key));
            Assert.That(children[0].HasAccess, Is.True);
            Assert.That(children[1].HasAccess, Is.True);
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

        Assert.That(totalItems, Is.EqualTo(5));
        // page size is still 2, but this is the last result page
        Assert.That(children, Has.Length.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["C1-C9"].Key));
            Assert.That(children[0].HasAccess, Is.True);
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

        Assert.That(totalItems, Is.EqualTo(5));
        Assert.That(children, Has.Length.EqualTo(5));
        Assert.Multiple(() =>
        {
            // all children of "C3-C3" (which are elements) should be allowed because "C3-C3" is a start node
            foreach (var childNumber in Enumerable.Range(1, 5))
            {
                var child = children[childNumber - 1];
                Assert.That(child.Entity.Key, Is.EqualTo(ItemsByName[$"C3-C3-E{childNumber}"].Key));
                Assert.That(child.HasAccess, Is.True);
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
        Assert.That(totalItems, Is.EqualTo(3));
        Assert.That(children, Has.Length.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["C3-C1"].Key));
            Assert.That(children[1].Entity.Key, Is.EqualTo(ItemsByName["C3-C2"].Key));
            Assert.That(children[2].Entity.Key, Is.EqualTo(ItemsByName["C3-C3"].Key));
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
        Assert.That(totalItems, Is.EqualTo(12));
        Assert.That(children, Has.Length.EqualTo(12));
        Assert.Multiple(() =>
        {
            // All children should be allowed because "C1" is a start node
            Assert.That(children.All(c => c.HasAccess), Is.True);

            // Verify we have both containers and elements in the results
            var containerChildren = children.Where(c => c.Entity.Name!.Contains("-C")).ToArray();
            var elementChildren = children.Where(c => c.Entity.Name!.Contains("-E")).ToArray();
            Assert.That(containerChildren, Has.Length.EqualTo(10));
            Assert.That(elementChildren, Has.Length.EqualTo(2));
        });
    }
}
