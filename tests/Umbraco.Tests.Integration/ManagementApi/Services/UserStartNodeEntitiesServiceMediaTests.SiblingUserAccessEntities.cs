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

        Assert.That(totalBefore, Is.EqualTo(2));
        Assert.That(totalAfter, Is.EqualTo(3));
        Assert.That(siblings, Has.Length.EqualTo(5));
        Assert.Multiple(() =>
        {
            for (int i = 0; i < 4; i++)
            {
                Assert.That(siblings[i].Entity.Key, Is.EqualTo(ItemsByName[$"1-{i + 3}"].Key));
                Assert.That(siblings[i].HasAccess, Is.True);
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

        Assert.That(totalBefore, Is.EqualTo(0));
        Assert.That(totalAfter, Is.EqualTo(0));
        Assert.That(siblings, Has.Length.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(siblings[0].Entity.Key, Is.EqualTo(ItemsByName[$"1-5"].Key));
            Assert.That(siblings[0].HasAccess, Is.True);
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

        Assert.That(totalBefore, Is.EqualTo(0));
        Assert.That(totalAfter, Is.EqualTo(0));
        Assert.That(siblings, Has.Length.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(siblings[0].Entity.Key, Is.EqualTo(ItemsByName[$"1-5"].Key));
            Assert.That(siblings[0].HasAccess, Is.True);
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

        Assert.That(totalBefore, Is.EqualTo(0));
        Assert.That(totalAfter, Is.EqualTo(1));
        Assert.That(siblings, Has.Length.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(siblings[0].Entity.Key, Is.EqualTo(ItemsByName[$"1-3"].Key));
            Assert.That(siblings[0].HasAccess, Is.True);
            Assert.That(siblings[1].Entity.Key, Is.EqualTo(ItemsByName[$"1-5"].Key));
            Assert.That(siblings[1].HasAccess, Is.True);
            Assert.That(siblings[2].Entity.Key, Is.EqualTo(ItemsByName[$"1-7"].Key));
            Assert.That(siblings[2].HasAccess, Is.True);
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

        Assert.That(totalBefore, Is.EqualTo(0));
        Assert.That(totalAfter, Is.EqualTo(0));
        Assert.That(siblings, Has.Length.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(siblings[0].Entity.Key, Is.EqualTo(ItemsByName[$"1-5"].Key));
            Assert.That(siblings[0].HasAccess, Is.False);
        });
    }
}
