using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services;

public partial class UserStartNodeEntitiesServiceElementTests
{
    [Test]
    public async Task SiblingUserAccessEntities_WithStartNodeOfTargetParent_YieldsAllContainerSiblings_AsAllowed()
    {
        // Root container "C1" is used as start node
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C1"].Id);

        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C1-C5"].Key,
                2,
                2,
                BySortOrder,
                out long totalBefore,
                out long totalAfter)
            .ToArray();

        // C1 has 10 child containers (C1-C1 through C1-C10) and 2 elements (C1-E1, C1-E2) = 12 total
        // Target is C1-C5, requesting 2 before and 2 after
        // Before C1-C5: C1-C1, C1-C2, C1-C3, C1-C4 = 4 items, returning 2, so totalBefore = 4 - 2 = 2
        // After C1-C5: C1-C6 through C1-C10 (5) + C1-E1, C1-E2 (2) = 7 items, returning 2, so totalAfter = 7 - 2 = 5
        Assert.AreEqual(2, totalBefore);
        Assert.AreEqual(5, totalAfter);
        Assert.AreEqual(5, siblings.Length);
        Assert.Multiple(() =>
        {
            // Siblings returned: C1-C3, C1-C4, C1-C5, C1-C6, C1-C7
            Assert.AreEqual(ItemsByName["C1-C3"].Key, siblings[0].Entity.Key);
            Assert.AreEqual(ItemsByName["C1-C4"].Key, siblings[1].Entity.Key);
            Assert.AreEqual(ItemsByName["C1-C5"].Key, siblings[2].Entity.Key);
            Assert.AreEqual(ItemsByName["C1-C6"].Key, siblings[3].Entity.Key);
            Assert.AreEqual(ItemsByName["C1-C7"].Key, siblings[4].Entity.Key);
            Assert.IsTrue(siblings.All(s => s.HasAccess));
        });
    }

    [Test]
    public async Task SiblingUserAccessEntities_WithStartNodeOfTargetParentAndTarget_YieldsOnlyTarget_AsAllowed()
    {
        // See notes on ChildUserAccessEntities_ChildAndGrandchildAsStartNode_AllowsOnlyGrandchild.

        // Root container "C1" and child container "C1-C5" are used as start nodes
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C1"].Id, ItemsByName["C1-C5"].Id);

        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C1-C5"].Key,
                2,
                2,
                BySortOrder,
                out long totalBefore,
                out long totalAfter)
            .ToArray();

        Assert.AreEqual(0, totalBefore);
        Assert.AreEqual(0, totalAfter);
        Assert.AreEqual(1, siblings.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ItemsByName["C1-C5"].Key, siblings[0].Entity.Key);
            Assert.IsTrue(siblings[0].HasAccess);
        });
    }

    [Test]
    public async Task SiblingUserAccessEntities_WithStartNodeOfTarget_YieldsOnlyTarget_AsAllowed()
    {
        // Child container "C1-C5" is used as start node
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C1-C5"].Id);

        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C1-C5"].Key,
                2,
                2,
                BySortOrder,
                out long totalBefore,
                out long totalAfter)
            .ToArray();

        Assert.AreEqual(0, totalBefore);
        Assert.AreEqual(0, totalAfter);
        Assert.AreEqual(1, siblings.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ItemsByName["C1-C5"].Key, siblings[0].Entity.Key);
            Assert.IsTrue(siblings[0].HasAccess);
        });
    }

    [Test]
    public async Task SiblingUserAccessEntities_WithStartsNodesOfTargetAndSiblings_YieldsOnlyPermitted_AsAllowed()
    {
        // Multiple child containers are used as start nodes
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C1-C3"].Id, ItemsByName["C1-C5"].Id, ItemsByName["C1-C7"].Id, ItemsByName["C1-C10"].Id);

        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C1-C5"].Key,
                1,
                1,
                BySortOrder,
                out long totalBefore,
                out long totalAfter)
            .ToArray();

        Assert.AreEqual(0, totalBefore);
        Assert.AreEqual(1, totalAfter);
        Assert.AreEqual(3, siblings.Length);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(ItemsByName["C1-C3"].Key, siblings[0].Entity.Key);
            Assert.IsTrue(siblings[0].HasAccess);
            Assert.AreEqual(ItemsByName["C1-C5"].Key, siblings[1].Entity.Key);
            Assert.IsTrue(siblings[1].HasAccess);
            Assert.AreEqual(ItemsByName["C1-C7"].Key, siblings[2].Entity.Key);
            Assert.IsTrue(siblings[2].HasAccess);
        });
    }

    [Test]
    public async Task SiblingUserAccessEntities_WithStartNodeOfTargetDescendant_YieldsTarget_AsNotAllowed()
    {
        // Child container "C1-C5" is used as start node (descendant of root container "C1")
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C1-C5"].Id);

        // Query for siblings of root container "C1" - the parent of our start node
        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                UmbracoObjectTypes.ElementContainer,
                elementStartNodePaths,
                ItemsByName["C1"].Key,
                2,
                2,
                BySortOrder,
                out long totalBefore,
                out long totalAfter)
            .ToArray();

        // User can see "C1" (to navigate to their start node "C1-C5"), but doesn't have direct access
        Assert.AreEqual(0, totalBefore);
        Assert.AreEqual(0, totalAfter);
        Assert.AreEqual(1, siblings.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ItemsByName["C1"].Key, siblings[0].Entity.Key);
            Assert.IsFalse(siblings[0].HasAccess);
        });
    }

    [Test]
    public async Task SiblingUserAccessEntities_WithStartNodeOfTargetParent_YieldsMixedSiblings_AsAllowed()
    {
        // Root container "C1" is used as start node - its children include both containers and elements
        var elementStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["C1"].Id);

        // Query for siblings of element "C1-E1" (which has container siblings too)
        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodePaths,
                ItemsByName["C1-E1"].Key,
                2,
                2,
                BySortOrder,
                out long totalBefore,
                out long totalAfter)
            .ToArray();

        // C1-E1 is after all child containers (C1-C1 through C1-C10), so it has 10 items before it
        // and C1-E2 after it
        // Before C1-E1: C1-C1 through C1-C10 = 10 items, returning 2, so totalBefore = 10 - 2 = 8
        // After C1-E1: C1-E2 = 1 item, returning 1 (not 2 since only 1 exists), so totalAfter = 0
        Assert.AreEqual(8, totalBefore);
        Assert.AreEqual(0, totalAfter);
        Assert.AreEqual(4, siblings.Length); // 2 before + target + 1 after
        Assert.Multiple(() =>
        {
            // Siblings returned: C1-C9, C1-C10, C1-E1, C1-E2
            Assert.AreEqual(ItemsByName["C1-C9"].Key, siblings[0].Entity.Key);
            Assert.AreEqual(ItemsByName["C1-C10"].Key, siblings[1].Entity.Key);
            Assert.AreEqual(ItemsByName["C1-E1"].Key, siblings[2].Entity.Key);
            Assert.AreEqual(ItemsByName["C1-E2"].Key, siblings[3].Entity.Key);
            Assert.IsTrue(siblings.All(s => s.HasAccess));
        });
    }
}