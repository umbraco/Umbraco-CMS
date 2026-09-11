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
        Assert.That(roots, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            // first and last root containers are the ones allowed
            Assert.That(roots[0].Entity.Key, Is.EqualTo(ItemsByName["C1"].Key));
            Assert.That(roots[1].Entity.Key, Is.EqualTo(ItemsByName["C5"].Key));

            // both are allowed (they are the actual start nodes)
            Assert.That(roots[0].HasAccess, Is.True);
            Assert.That(roots[1].HasAccess, Is.True);
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

        Assert.That(roots, Has.Length.EqualTo(3));
        Assert.Multiple(() =>
        {
            // the three start nodes are the children of the "C1", "C3" and "C5" roots, respectively, so these are expected as roots
            Assert.That(roots[0].Entity.Key, Is.EqualTo(ItemsByName["C1"].Key));
            Assert.That(roots[1].Entity.Key, Is.EqualTo(ItemsByName["C3"].Key));
            Assert.That(roots[2].Entity.Key, Is.EqualTo(ItemsByName["C5"].Key));

            // all are disallowed - only the children (the actual start nodes) are allowed
            Assert.That(roots.All(r => r.HasAccess is false), Is.True);
        });
    }
}
