using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services;

/// <summary>
/// Element tests use a folder/container structure with elements mixed at each level:
/// - Level 1 (Root): Containers "C1"-"C5" + Elements "E1"-"E3"
/// - Level 2: Child containers "C1-C1" through "C1-C10" + Elements "C1-E1", "C1-E2" under each root container
/// - Level 3: Elements "C1-C1-E1" through "C1-C1-E5" under each child container
/// </summary>
[TestFixture]
public partial class UserStartNodeEntitiesServiceElementTests : UserStartNodeEntitiesServiceTestsBase
{
    private IElementService ElementService => GetRequiredService<IElementService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Element;

    protected override string SectionAlias => Constants.Applications.Library;

    protected override async Task CreateContentTypeAndHierarchy()
    {
        // Create the element content type
        var contentType = new ContentTypeBuilder()
            .WithAlias("theElementType")
            .WithIsElement(true)
            .Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Create hierarchy with mixed containers and elements at each level
        foreach (var rootNumber in Enumerable.Range(1, 5))
        {
            // Level 1: Root containers (folders)
            var rootContainerResult = await ElementContainerService.CreateAsync(
                null,
                $"C{rootNumber}",
                null, // parent at root
                Constants.Security.SuperUserKey);
            Assert.IsTrue(rootContainerResult.Success);
            Assert.NotNull(rootContainerResult.Result);
            var rootContainer = rootContainerResult.Result;
            ItemsByName[rootContainer.Name!] = (rootContainer.Id, rootContainer.Key);

            foreach (var childNumber in Enumerable.Range(1, 10))
            {
                // Level 2: Child containers (folders)
                var childContainerResult = await ElementContainerService.CreateAsync(
                    null,
                    $"C{rootNumber}-C{childNumber}",
                    rootContainer.Key,
                    Constants.Security.SuperUserKey);
                Assert.IsTrue(childContainerResult.Success);
                Assert.NotNull(childContainerResult.Result);
                var childContainer = childContainerResult.Result;
                ItemsByName[childContainer.Name!] = (childContainer.Id, childContainer.Key);

                foreach (var grandChildNumber in Enumerable.Range(1, 5))
                {
                    // Level 3: Elements (leaf nodes)
                    var element = ElementService.Create($"C{rootNumber}-C{childNumber}-E{grandChildNumber}", contentType.Alias);
                    element.ParentId = childContainer.Id;
                    var saveElementResult = ElementService.Save([element]);
                    Assert.IsTrue(saveElementResult.Success);
                    ItemsByName[element.Name!] = (element.Id, element.Key);
                }
            }

            // Level 2: Elements alongside child containers (mixed level)
            foreach (var elementNumber in Enumerable.Range(1, 2))
            {
                var element = ElementService.Create($"C{rootNumber}-E{elementNumber}", contentType.Alias);
                element.ParentId = rootContainer.Id;
                var saveElementResult = ElementService.Save([element]);
                Assert.IsTrue(saveElementResult.Success);
                ItemsByName[element.Name!] = (element.Id, element.Key);
            }
        }

        // Level 1: Root elements alongside root containers (mixed level)
        foreach (var elementNumber in Enumerable.Range(1, 3))
        {
            var element = ElementService.Create($"E{elementNumber}", contentType.Alias);
            var saveElementResult = ElementService.Save([element]);
            Assert.IsTrue(saveElementResult.Success);
            ItemsByName[element.Name!] = (element.Id, element.Key);
        }
    }

    protected override void ClearUserGroupStartNode(IUserGroup userGroup)
        => userGroup.StartElementId = null;

    protected override Core.Models.Membership.User BuildUserWithStartNodes(int[] startNodeIds)
        => new UserBuilder()
            .WithName(Guid.NewGuid().ToString("N"))
            .WithStartElementIds(startNodeIds)
            .Build();

    protected override string[]? GetStartNodePaths(Cms.Core.Models.Membership.User user)
        => user.GetElementStartNodePaths(EntityService, AppCaches.NoCache);

    protected override int[]? CalculateStartNodeIds(Cms.Core.Models.Membership.User user)
        => user.CalculateElementStartNodeIds(EntityService, AppCaches.NoCache);
}
