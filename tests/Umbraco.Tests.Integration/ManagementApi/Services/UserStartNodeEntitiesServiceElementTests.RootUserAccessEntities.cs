using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services;

public partial class UserStartNodeEntitiesServiceElementTests
{
    [Test]
    public async Task RootUserAccessEntities_FirstAndLastRootContainer_YieldsBoth_AsAllowed()
    {
        // Root containers "C1" and "C5" are used as start nodes
        var elementStartNodeIds = await CreateUserAndGetStartNodeIds(ItemsByName["C1"].Id, ItemsByName["C5"].Id);

        var roots = UserStartNodeEntitiesService
            .RootUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodeIds)
            .ToArray();

        // expected total is 2, because only two items at root ("C1" and "C5") are allowed
        Assert.AreEqual(2, roots.Length);
        Assert.Multiple(() =>
        {
            // first and last root containers are the ones allowed
            Assert.AreEqual(ItemsByName["C1"].Key, roots[0].Entity.Key);
            Assert.AreEqual(ItemsByName["C5"].Key, roots[1].Entity.Key);

            // both are allowed (they are the actual start nodes)
            Assert.IsTrue(roots[0].HasAccess);
            Assert.IsTrue(roots[1].HasAccess);
        });
    }

    [Test]
    public async Task RootUserAccessEntities_ChildContainersAsStartNode_YieldsChildRoots_AsNotAllowed()
    {
        // Child containers "C1-C3", "C3-C3", "C5-C3" are used as start nodes
        var elementStartNodeIds = await CreateUserAndGetStartNodeIds(ItemsByName["C1-C3"].Id, ItemsByName["C3-C3"].Id, ItemsByName["C5-C3"].Id);

        var roots = UserStartNodeEntitiesService
            .RootUserAccessEntities(
                [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer],
                elementStartNodeIds)
            .ToArray();

        Assert.AreEqual(3, roots.Length);
        Assert.Multiple(() =>
        {
            // the three start nodes are the children of the "C1", "C3" and "C5" roots, respectively, so these are expected as roots
            Assert.AreEqual(ItemsByName["C1"].Key, roots[0].Entity.Key);
            Assert.AreEqual(ItemsByName["C3"].Key, roots[1].Entity.Key);
            Assert.AreEqual(ItemsByName["C5"].Key, roots[2].Entity.Key);

            // all are disallowed - only the children (the actual start nodes) are allowed
            Assert.IsTrue(roots.All(r => r.HasAccess is false));
        });
    }
}
