using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services;

public partial class UserStartNodeEntitiesServiceTests
{
    [Test]
    public async Task SiblingUserAccessEntities_WithStartNodeOfTargetParent_YieldsAll_AsAllowed()
    {
        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(_contentByName["1"].Id);

        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["1-5"].Key,
                2,
                2,
                BySortOrder)
            .ToArray();

        Assert.AreEqual(5, siblings.Length);
        Assert.Multiple(() =>
        {
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(_contentByName[$"1-{i + 3}"].Key, siblings[i].Entity.Key);
                Assert.IsTrue(siblings[i].HasAccess);
            }
        });
    }

    [Test]
    public async Task SiblingUserAccessEntities_WithStartsNodesOfTargetAndSiblings_YieldsOnlyPermitted_AsAllowed()
    {
        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(_contentByName["1-3"].Id, _contentByName["1-5"].Id, _contentByName["1-7"].Id);

        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["1-5"].Key,
                2,
                2,
                BySortOrder)
            .ToArray();

        Assert.AreEqual(3, siblings.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(_contentByName[$"1-3"].Key, siblings[0].Entity.Key);
            Assert.IsTrue(siblings[0].HasAccess);
            Assert.AreEqual(_contentByName[$"1-5"].Key, siblings[1].Entity.Key);
            Assert.IsTrue(siblings[1].HasAccess);
            Assert.AreEqual(_contentByName[$"1-7"].Key, siblings[2].Entity.Key);
            Assert.IsTrue(siblings[2].HasAccess);
        });
    }

    [Test]
    public async Task SiblingUserAccessEntities_WithStartNodesOfTargetChild_YieldsTarget_AsNotAllowed()
    {
        var contentStartNodePaths = await CreateUserAndGetStartNodePaths(_contentByName["1-5-1"].Id);

        var siblings = UserStartNodeEntitiesService
            .SiblingUserAccessEntities(
                UmbracoObjectTypes.Document,
                contentStartNodePaths,
                _contentByName["1-5"].Key,
                2,
                2,
                BySortOrder)
            .ToArray();

        Assert.AreEqual(1, siblings.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(_contentByName[$"1-5"].Key, siblings[0].Entity.Key);
            Assert.IsFalse(siblings[0].HasAccess);
        });
    }
}
