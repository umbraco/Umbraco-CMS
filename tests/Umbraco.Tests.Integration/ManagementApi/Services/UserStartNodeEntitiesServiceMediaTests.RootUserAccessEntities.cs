using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services;

public partial class UserStartNodeEntitiesServiceMediaTests
{
    [Test]
    public async Task RootUserAccessEntities_FirstAndLastRoot_YieldsBoth_AsAllowed()
    {
        var contentStartNodeIds = await CreateUserAndGetStartNodeIds(ItemsByName["1"].Id, ItemsByName["5"].Id);

        var roots = UserStartNodeEntitiesService
            .RootUserAccessEntities(
                UmbracoObjectTypes.Media,
                contentStartNodeIds)
            .ToArray();

        // expected total is 2, because only two items at root ("1" amd "10") are allowed
        Assert.That(roots, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            // first and last content items are the ones allowed
            Assert.That(roots[0].Entity.Key, Is.EqualTo(ItemsByName["1"].Key));
            Assert.That(roots[1].Entity.Key, Is.EqualTo(ItemsByName["5"].Key));

            // explicitly verify the entity sort order, both so we know sorting works,
            // and so we know it's actually the first and last item at root
            Assert.That(roots[0].Entity.SortOrder, Is.EqualTo(0));
            Assert.That(roots[1].Entity.SortOrder, Is.EqualTo(4));

            // both are allowed (they are the actual start nodes)
            Assert.That(roots[0].HasAccess, Is.True);
            Assert.That(roots[1].HasAccess, Is.True);
        });
    }

    [Test]
    public async Task RootUserAccessEntities_ChildrenAsStartNode_YieldsChildRoots_AsNotAllowed()
    {
        var contentStartNodeIds = await CreateUserAndGetStartNodeIds(ItemsByName["1-3"].Id, ItemsByName["3-3"].Id, ItemsByName["5-3"].Id);

        var roots = UserStartNodeEntitiesService
            .RootUserAccessEntities(
                UmbracoObjectTypes.Media,
                contentStartNodeIds)
            .ToArray();

        Assert.That(roots, Has.Length.EqualTo(3));
        Assert.Multiple(() =>
        {
            // the three start nodes are the children of the "1", "3" and "5" roots, respectively, so these are expected as roots
            Assert.That(roots[0].Entity.Key, Is.EqualTo(ItemsByName["1"].Key));
            Assert.That(roots[1].Entity.Key, Is.EqualTo(ItemsByName["3"].Key));
            Assert.That(roots[2].Entity.Key, Is.EqualTo(ItemsByName["5"].Key));

            // all are disallowed - only the children (the actual start nodes) are allowed
            Assert.That(roots.All(r => r.HasAccess is false), Is.True);
        });
    }

    [Test]
    public async Task RootUserAccessEntities_GrandchildrenAsStartNode_YieldsGrandchildRoots_AsNotAllowed()
    {
        var contentStartNodeIds = await CreateUserAndGetStartNodeIds(ItemsByName["1-2-3"].Id, ItemsByName["2-3-4"].Id, ItemsByName["3-4-5"].Id);

        var roots = UserStartNodeEntitiesService
            .RootUserAccessEntities(
                UmbracoObjectTypes.Media,
                contentStartNodeIds)
            .ToArray();

        Assert.That(roots, Has.Length.EqualTo(3));
        Assert.Multiple(() =>
        {
            // the three start nodes are the grandchildren of the "1", "2" and "3" roots, respectively, so these are expected as roots
            Assert.That(roots[0].Entity.Key, Is.EqualTo(ItemsByName["1"].Key));
            Assert.That(roots[1].Entity.Key, Is.EqualTo(ItemsByName["2"].Key));
            Assert.That(roots[2].Entity.Key, Is.EqualTo(ItemsByName["3"].Key));

            // all are disallowed - only the grandchildren (the actual start nodes) are allowed
            Assert.That(roots.All(r => r.HasAccess is false), Is.True);
        });
    }
}
