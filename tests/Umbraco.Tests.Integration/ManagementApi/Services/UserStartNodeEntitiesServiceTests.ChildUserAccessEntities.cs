using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services;

public partial class UserStartNodeEntitiesServiceTests
{
    [Test]
    public async Task ChildUserAccessEntities_FirstAndLastChildOfRoot_YieldsBothInFirstPage_AsAllowed()
    {
        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(_contentByName["1-1"].Id, _contentByName["1-10"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["1"].Key,
                0,
                3,
                BySortOrder,
                out var totalItems)
            .ToArray();

        // expected total is 2, because only two items under "1" are allowed (note the page size is 3 for good measure)
        Assert.AreEqual(2, totalItems);
        Assert.AreEqual(2, children.Length);
        Assert.Multiple(() =>
        {
            // first and last content items are the ones allowed
            Assert.AreEqual(_contentByName["1-1"].Key, children[0].Entity.Key);
            Assert.AreEqual(_contentByName["1-10"].Key, children[1].Entity.Key);

            // explicitly verify the entity sort order, both so we know sorting works,
            // and so we know it's actually the first and last item below "1"
            Assert.AreEqual(0, children[0].Entity.SortOrder);
            Assert.AreEqual(9, children[1].Entity.SortOrder);

            // both are allowed (they are the actual start nodes)
            Assert.IsTrue(children[0].HasAccess);
            Assert.IsTrue(children[1].HasAccess);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_InAndOutOfScope_YieldsOnlyChildrenInScope()
    {
        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(_contentByName["1-5"].Id, _contentByName["2-10"].Id);
        Assert.AreEqual(2, contentStartNodePaths.Length);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["2"].Key,
                0,
                10,
                BySortOrder,
                out var totalItems)
            .ToArray();

        Assert.AreEqual(1, totalItems);
        Assert.AreEqual(1, children.Length);
        Assert.Multiple(() =>
        {
            // only the "2-10" content item is returned, as "1-5" is out of scope
            Assert.AreEqual(_contentByName["2-10"].Key, children[0].Entity.Key);
            Assert.IsTrue(children[0].HasAccess);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_OutOfScope_YieldsNothing()
    {
        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(_contentByName["1-5"].Id, _contentByName["2-10"].Id);
        Assert.AreEqual(2, contentStartNodePaths.Length);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["3"].Key,
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
        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(
            _contentByName["1-1"].Id,
            _contentByName["1-3"].Id,
            _contentByName["1-5"].Id,
            _contentByName["1-7"].Id,
            _contentByName["1-9"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["1"].Key,
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
            Assert.AreEqual(_contentByName["1-1"].Key, children[0].Entity.Key);
            Assert.AreEqual(_contentByName["1-3"].Key, children[1].Entity.Key);
            Assert.IsTrue(children[0].HasAccess);
            Assert.IsTrue(children[1].HasAccess);
        });

        // next result page
        children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["1"].Key,
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
            Assert.AreEqual(_contentByName["1-5"].Key, children[0].Entity.Key);
            Assert.AreEqual(_contentByName["1-7"].Key, children[1].Entity.Key);
            Assert.IsTrue(children[0].HasAccess);
            Assert.IsTrue(children[1].HasAccess);
        });

        // next result page
        children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["1"].Key,
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
            Assert.AreEqual(_contentByName["1-9"].Key, children[0].Entity.Key);
            Assert.IsTrue(children[0].HasAccess);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_ChildrenAsStartNode_YieldsAllGrandchildren_AsAllowed()
    {
        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(_contentByName["3-3"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["3-3"].Key,
                0,
                100,
                BySortOrder,
                out var totalItems)
            .ToArray();

        Assert.AreEqual(5, totalItems);
        Assert.AreEqual(5, children.Length);
        Assert.Multiple(() =>
        {
            // all children of "3-3" should be allowed because "3-3" is a start node
            foreach (var childNumber in Enumerable.Range(1, 5))
            {
                var child = children[childNumber - 1];
                Assert.AreEqual(_contentByName[$"3-3-{childNumber}"].Key, child.Entity.Key);
                Assert.IsTrue(child.HasAccess);
            }
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_GrandchildrenAsStartNode_YieldsGrandchildren_AsAllowed()
    {
        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(_contentByName["3-3-3"].Id, _contentByName["3-3-4"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["3-3"].Key,
                0,
                3,
                BySortOrder,
                out var totalItems)
            .ToArray();

        Assert.AreEqual(2, totalItems);
        Assert.AreEqual(2, children.Length);
        Assert.Multiple(() =>
        {
            // the two items are the children of "3-3" - that is, the actual start nodes
            Assert.AreEqual(_contentByName["3-3-3"].Key, children[0].Entity.Key);
            Assert.AreEqual(_contentByName["3-3-4"].Key, children[1].Entity.Key);

            // both are allowed (they are the actual start nodes)
            Assert.IsTrue(children[0].HasAccess);
            Assert.IsTrue(children[1].HasAccess);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_GrandchildrenAsStartNode_YieldsChildren_AsNotAllowed()
    {
        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(_contentByName["3-3-3"].Id, _contentByName["3-5-3"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["3"].Key,
                0,
                3,
                BySortOrder,
                out var totalItems)
            .ToArray();

        Assert.AreEqual(2, totalItems);
        Assert.AreEqual(2, children.Length);
        Assert.Multiple(() =>
        {
            // the two items are the children of "3" - that is, the parents of the actual start nodes
            Assert.AreEqual(_contentByName["3-3"].Key, children[0].Entity.Key);
            Assert.AreEqual(_contentByName["3-5"].Key, children[1].Entity.Key);

            // both are disallowed - only the two children (the actual start nodes) are allowed
            Assert.IsFalse(children[0].HasAccess);
            Assert.IsFalse(children[1].HasAccess);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_ChildAndGrandchildAsStartNode_AllowsOnlyGrandchild()
    {
        // NOTE: this test proves that start node paths are *not* additive when structural inheritance is in play.
        //       if one has a start node that is a descendant to another start node, the descendant start node "wins"
        //       and the ancestor start node is ignored. this differs somewhat from the norm; we normally consider
        //       permissions additive (which in this case would mean ignoring the descendant rather than the ancestor).

        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(_contentByName["3-3"].Id, _contentByName["3-3-1"].Id, _contentByName["3-3-5"].Id);
        Assert.AreEqual(2, contentStartNodePaths.Length);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["3"].Key,
                0,
                10,
                BySortOrder,
                out var totalItems)
            .ToArray();
        Assert.AreEqual(1, totalItems);
        Assert.AreEqual(1, children.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(_contentByName["3-3"].Key, children[0].Entity.Key);
            Assert.IsFalse(children[0].HasAccess);
        });

        children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["3-3"].Key,
                0,
                10,
                BySortOrder,
                out totalItems)
            .ToArray();

        Assert.AreEqual(2, totalItems);
        Assert.AreEqual(2, children.Length);
        Assert.Multiple(() =>
        {
            // the two items are the children of "3-3" - that is, the actual start nodes
            Assert.AreEqual(_contentByName["3-3-1"].Key, children[0].Entity.Key);
            Assert.AreEqual(_contentByName["3-3-5"].Key, children[1].Entity.Key);

            Assert.IsTrue(children[0].HasAccess);
            Assert.IsTrue(children[1].HasAccess);
        });
    }

    [Test]
    public async Task ChildUserAccessEntities_ReverseStartNodeOrder_DoesNotAffectResultOrder()
    {
        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(_contentByName["3-3"].Id, _contentByName["3-2"].Id, _contentByName["3-1"].Id);

        var children = UserStartNodeEntitiesService
            .ChildUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["3"].Key,
                0,
                10,
                BySortOrder,
                out var totalItems)
            .ToArray();
        Assert.AreEqual(3, totalItems);
        Assert.AreEqual(3, children.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(_contentByName["3-1"].Key, children[0].Entity.Key);
            Assert.AreEqual(_contentByName["3-2"].Key, children[1].Entity.Key);
            Assert.AreEqual(_contentByName["3-3"].Key, children[2].Entity.Key);
        });
    }
}
