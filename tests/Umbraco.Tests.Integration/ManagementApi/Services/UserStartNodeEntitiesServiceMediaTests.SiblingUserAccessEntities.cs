using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services;

public partial class UserStartNodeEntitiesServiceMediaTests
{
    [Test]
    public async Task SiblingUserAccessEntities_WithStartNodeOfTargetParent_YieldsAll_AsAllowed()
    {
        var mediaStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["1"].Id);

        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["1-5"].Key,
                2,
                2,
                BySortOrder,
                out long totalBefore,
                out long totalAfter)
            .ToArray();

        Assert.AreEqual(2, totalBefore);
        Assert.AreEqual(3, totalAfter);
        Assert.AreEqual(5, siblings.Length);
        Assert.Multiple(() =>
        {
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(ItemsByName[$"1-{i + 3}"].Key, siblings[i].Entity.Key);
                Assert.IsTrue(siblings[i].HasAccess);
            }
        });
    }

    [Test]
    public async Task SiblingUserAccessEntities_WithStartNodeOfTargetParentAndTarget_YieldsOnlyTarget_AsAllowed()
    {
        // See notes on ChildUserAccessEntities_ChildAndGrandchildAsStartNode_AllowsOnlyGrandchild.

        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["1"].Id, ItemsByName["1-5"].Id);

        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                UmbracoObjectTypes.Media,
                contentStartNodePaths,
                ItemsByName["1-5"].Key,
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
            Assert.AreEqual(ItemsByName[$"1-5"].Key, siblings[0].Entity.Key);
            Assert.IsTrue(siblings[0].HasAccess);
        });
    }

    [Test]
    public async Task SiblingUserAccessEntities_WithStartNodeOfTarget_YieldsOnlyTarget_AsAllowed()
    {
        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["1-5"].Id);

        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                UmbracoObjectTypes.Media,
                contentStartNodePaths,
                ItemsByName["1-5"].Key,
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
            Assert.AreEqual(ItemsByName[$"1-5"].Key, siblings[0].Entity.Key);
            Assert.IsTrue(siblings[0].HasAccess);
        });
    }

    [Test]
    public async Task SiblingUserAccessEntities_WithStartsNodesOfTargetAndSiblings_YieldsOnlyPermitted_AsAllowed()
    {
        var mediaStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["1-3"].Id, ItemsByName["1-5"].Id, ItemsByName["1-7"].Id, ItemsByName["1-10"].Id);

        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["1-5"].Key,
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
            Assert.AreEqual(ItemsByName[$"1-3"].Key, siblings[0].Entity.Key);
            Assert.IsTrue(siblings[0].HasAccess);
            Assert.AreEqual(ItemsByName[$"1-5"].Key, siblings[1].Entity.Key);
            Assert.IsTrue(siblings[1].HasAccess);
            Assert.AreEqual(ItemsByName[$"1-7"].Key, siblings[2].Entity.Key);
            Assert.IsTrue(siblings[2].HasAccess);
        });
    }

    [Test]
    public async Task SiblingUserAccessEntities_WithStartsNodesOfTargetGrandchild_YieldsTarget_AsNotAllowed()
    {
        var mediaStartNodePaths = await CreateUserAndGetStartNodePaths(ItemsByName["1-5-1"].Id);

        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                UmbracoObjectTypes.Media,
                mediaStartNodePaths,
                ItemsByName["1-5"].Key,
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
            Assert.AreEqual(ItemsByName[$"1-5"].Key, siblings[0].Entity.Key);
            Assert.IsFalse(siblings[0].HasAccess);
        });
    }
}
