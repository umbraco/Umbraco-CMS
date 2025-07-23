using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services;

public partial class UserStartNodeEntitiesServiceMediaTests
{
    [Test]
    public async Task RootUserAccessEntities_FirstAndLastRoot_YieldsBoth_AsAllowed()
    {
        var contentStartNodeIds = await CreateUserAndGetStartNodeIds(_mediaByName["1"].Id, _mediaByName["5"].Id);

        var roots = UserStartNodeEntitiesService
            .RootUserAccessEntities(
                UmbracoObjectTypes.Media,
                contentStartNodeIds)
            .ToArray();

        // expected total is 2, because only two items at root ("1" amd "10") are allowed
        Assert.AreEqual(2, roots.Length);
        Assert.Multiple(() =>
        {
            // first and last content items are the ones allowed
            Assert.AreEqual(_mediaByName["1"].Key, roots[0].Entity.Key);
            Assert.AreEqual(_mediaByName["5"].Key, roots[1].Entity.Key);

            // explicitly verify the entity sort order, both so we know sorting works,
            // and so we know it's actually the first and last item at root
            Assert.AreEqual(0, roots[0].Entity.SortOrder);
            Assert.AreEqual(4, roots[1].Entity.SortOrder);

            // both are allowed (they are the actual start nodes)
            Assert.IsTrue(roots[0].HasAccess);
            Assert.IsTrue(roots[1].HasAccess);
        });
    }

    [Test]
    public async Task RootUserAccessEntities_ChildrenAsStartNode_YieldsChildRoots_AsNotAllowed()
    {
        var contentStartNodeIds = await CreateUserAndGetStartNodeIds(_mediaByName["1-3"].Id, _mediaByName["3-3"].Id, _mediaByName["5-3"].Id);

        var roots = UserStartNodeEntitiesService
            .RootUserAccessEntities(
                UmbracoObjectTypes.Media,
                contentStartNodeIds)
            .ToArray();

        Assert.AreEqual(3, roots.Length);
        Assert.Multiple(() =>
        {
            // the three start nodes are the children of the "1", "3" and "5" roots, respectively, so these are expected as roots
            Assert.AreEqual(_mediaByName["1"].Key, roots[0].Entity.Key);
            Assert.AreEqual(_mediaByName["3"].Key, roots[1].Entity.Key);
            Assert.AreEqual(_mediaByName["5"].Key, roots[2].Entity.Key);

            // all are disallowed - only the children (the actual start nodes) are allowed
            Assert.IsTrue(roots.All(r => r.HasAccess is false));
        });
    }

    [Test]
    public async Task RootUserAccessEntities_GrandchildrenAsStartNode_YieldsGrandchildRoots_AsNotAllowed()
    {
        var contentStartNodeIds = await CreateUserAndGetStartNodeIds(_mediaByName["1-2-3"].Id, _mediaByName["2-3-4"].Id, _mediaByName["3-4-5"].Id);

        var roots = UserStartNodeEntitiesService
            .RootUserAccessEntities(
                UmbracoObjectTypes.Media,
                contentStartNodeIds)
            .ToArray();

        Assert.AreEqual(3, roots.Length);
        Assert.Multiple(() =>
        {
            // the three start nodes are the grandchildren of the "1", "2" and "3" roots, respectively, so these are expected as roots
            Assert.AreEqual(_mediaByName["1"].Key, roots[0].Entity.Key);
            Assert.AreEqual(_mediaByName["2"].Key, roots[1].Entity.Key);
            Assert.AreEqual(_mediaByName["3"].Key, roots[2].Entity.Key);

            // all are disallowed - only the grandchildren (the actual start nodes) are allowed
            Assert.IsTrue(roots.All(r => r.HasAccess is false));
        });
    }
}
