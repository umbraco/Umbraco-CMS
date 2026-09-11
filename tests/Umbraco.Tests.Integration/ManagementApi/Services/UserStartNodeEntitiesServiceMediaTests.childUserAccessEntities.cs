using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services;

public partial class UserStartNodeEntitiesServiceMediaTests
{
    [Test]
    public async Task ChildUserAccessEntities_FirstAndLastChildOfRoot_YieldsBothInFirstPage_AsAllowed()
    {
        var mediaStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["1-1"].Id, ItemsByName["1-10"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["1"].Key,
                0,
                3,
                BySortOrder,
                out var totalItems)
            .ToArray();

        // expected total is 2, because only two items under "1" are allowed (note the page size is 3 for good measure)
        Assert.That(totalItems, Is.EqualTo(2));
        Assert.That(children, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            // first and last media items are the ones allowed
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["1-1"].Key));
            Assert.That(children[1].Entity.Key, Is.EqualTo(ItemsByName["1-10"].Key));

            // explicitly verify the entity sort order, both so we know sorting works,
            // and so we know it's actually the first and last item below "1"
            Assert.That(children[0].Entity.SortOrder, Is.EqualTo(0));
            Assert.That(children[1].Entity.SortOrder, Is.EqualTo(9));

            // both are allowed (they are the actual start nodes)
            Assert.That(children[0].HasAccess, Is.True);
            Assert.That(children[1].HasAccess, Is.True);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_InAndOutOfScope_YieldsOnlyChildrenInScope()
    {
        var mediaStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["1-5"].Id, ItemsByName["2-10"].Id);
        Assert.That(mediaStartNodePaths, Has.Length.EqualTo(2));

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["2"].Key,
                0,
                10,
                BySortOrder,
                out var totalItems)
            .ToArray();

        Assert.That(totalItems, Is.EqualTo(1));
        Assert.That(children, Has.Length.EqualTo(1));
        Assert.Multiple(() =>
        {
            // only the "2-10" media item is returned, as "1-5" is out of scope
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["2-10"].Key));
            Assert.That(children[0].HasAccess, Is.True);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_OutOfScope_YieldsNothing()
    {
        var mediaStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["1-5"].Id, ItemsByName["2-10"].Id);
        Assert.That(mediaStartNodePaths, Has.Length.EqualTo(2));

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["3"].Key,
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
        var mediaStartNodePaths = await CreateUserAndGetStartNodePaths(
            ItemsByName["1-1"].Id,
            ItemsByName["1-3"].Id,
            ItemsByName["1-5"].Id,
            ItemsByName["1-7"].Id,
            ItemsByName["1-9"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["1"].Key,
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
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["1-1"].Key));
            Assert.That(children[1].Entity.Key, Is.EqualTo(ItemsByName["1-3"].Key));
            Assert.That(children[0].HasAccess, Is.True);
            Assert.That(children[1].HasAccess, Is.True);
        });

        // next result page
        children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["1"].Key,
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
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["1-5"].Key));
            Assert.That(children[1].Entity.Key, Is.EqualTo(ItemsByName["1-7"].Key));
            Assert.That(children[0].HasAccess, Is.True);
            Assert.That(children[1].HasAccess, Is.True);
        });

        // next result page
        children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["1"].Key,
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
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["1-9"].Key));
            Assert.That(children[0].HasAccess, Is.True);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_ChildrenAsStartNode_YieldsAllGrandchildren_AsAllowed()
    {
        var mediaStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["3-3"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["3-3"].Key,
                0,
                100,
                BySortOrder,
                out var totalItems)
            .ToArray();

        Assert.That(totalItems, Is.EqualTo(5));
        Assert.That(children, Has.Length.EqualTo(5));
        Assert.Multiple(() =>
        {
            // all children of "3-3" should be allowed because "3-3" is a start node
            foreach (var childNumber in Enumerable.Range(1, 5))
            {
                var child = children[childNumber - 1];
                Assert.That(child.Entity.Key, Is.EqualTo(ItemsByName[$"3-3-{childNumber}"].Key));
                Assert.That(child.HasAccess, Is.True);
            }
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_GrandchildrenAsStartNode_YieldsGrandchildren_AsAllowed()
    {
        var mediaStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["3-3-3"].Id, ItemsByName["3-3-4"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["3-3"].Key,
                0,
                3,
                BySortOrder,
                out var totalItems)
            .ToArray();

        Assert.That(totalItems, Is.EqualTo(2));
        Assert.That(children, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            // the two items are the children of "3-3" - that is, the actual start nodes
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["3-3-3"].Key));
            Assert.That(children[1].Entity.Key, Is.EqualTo(ItemsByName["3-3-4"].Key));

            // both are allowed (they are the actual start nodes)
            Assert.That(children[0].HasAccess, Is.True);
            Assert.That(children[1].HasAccess, Is.True);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_GrandchildrenAsStartNode_YieldsChildren_AsNotAllowed()
    {
        var mediaStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["3-3-3"].Id, ItemsByName["3-5-3"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["3"].Key,
                0,
                3,
                BySortOrder,
                out var totalItems)
            .ToArray();

        Assert.That(totalItems, Is.EqualTo(2));
        Assert.That(children, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            // the two items are the children of "3" - that is, the parents of the actual start nodes
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["3-3"].Key));
            Assert.That(children[1].Entity.Key, Is.EqualTo(ItemsByName["3-5"].Key));

            // both are disallowed - only the two children (the actual start nodes) are allowed
            Assert.That(children[0].HasAccess, Is.False);
            Assert.That(children[1].HasAccess, Is.False);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_ChildAndGrandchildAsStartNode_AllowsOnlyGrandchild()
    {
        // NOTE: this test proves that start node paths are *not* additive when structural inheritance is in play.
        //       if one has a start node that is a descendant to another start node, the descendant start node "wins"
        //       and the ancestor start node is ignored. this differs somewhat from the norm; we normally consider
        //       permissions additive (which in this case would mean ignoring the descendant rather than the ancestor).

        var mediaStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["3-3"].Id, ItemsByName["3-3-1"].Id, ItemsByName["3-3-5"].Id);
        Assert.That(mediaStartNodePaths, Has.Length.EqualTo(2));

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["3"].Key,
                0,
                10,
                BySortOrder,
                out var totalItems)
            .ToArray();
        Assert.That(totalItems, Is.EqualTo(1));
        Assert.That(children, Has.Length.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["3-3"].Key));
            Assert.That(children[0].HasAccess, Is.False);
        });

        children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["3-3"].Key,
                0,
                10,
                BySortOrder,
                out totalItems)
            .ToArray();

        Assert.That(totalItems, Is.EqualTo(2));
        Assert.That(children, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            // the two items are the children of "3-3" - that is, the actual start nodes
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["3-3-1"].Key));
            Assert.That(children[1].Entity.Key, Is.EqualTo(ItemsByName["3-3-5"].Key));

            Assert.That(children[0].HasAccess, Is.True);
            Assert.That(children[1].HasAccess, Is.True);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_ReverseStartNodeOrder_DoesNotAffectResultOrder()
    {
        var mediaStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["3-3"].Id, ItemsByName["3-2"].Id, ItemsByName["3-1"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["3"].Key,
                0,
                10,
                BySortOrder,
                out var totalItems)
            .ToArray();
        Assert.That(totalItems, Is.EqualTo(3));
        Assert.That(children, Has.Length.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(children[0].Entity.Key, Is.EqualTo(ItemsByName["3-1"].Key));
            Assert.That(children[1].Entity.Key, Is.EqualTo(ItemsByName["3-2"].Key));
            Assert.That(children[2].Entity.Key, Is.EqualTo(ItemsByName["3-3"].Key));
        });
    }
}
