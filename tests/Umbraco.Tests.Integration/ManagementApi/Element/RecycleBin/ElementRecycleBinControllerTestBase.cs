using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.RecycleBin;

public abstract class ElementRecycleBinControllerTestBase<T> : ManagementApiUserGroupTestBase<T>
    where T : ElementRecycleBinControllerBase
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    [Test]
    public async Task User_With_Non_Root_Element_Start_Node_Cannot_Access_Recycle_Bin()
    {
        // Create an element container (folder) to use as a non-root start node
        var containerResult = await ElementContainerService.CreateAsync(
            null,
            $"Test Folder {Guid.NewGuid()}",
            null, // at root
            Constants.Security.SuperUserKey);
        Assert.IsTrue(containerResult.Success);
        var container = containerResult.Result!;

        // Create a user group with Library section access but with a non-root element start node
        var userGroup = new UserGroupBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName("Test Group With Element Start Node")
            .WithAllowedSections(["library"])
            .WithStartElementId(container.Id)
            .Build();

        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        // Authenticate as a user in the group with non-root element start node
        await AuthenticateClientAsync(Client, $"startnodetest{Guid.NewGuid():N}@umbraco.com", "1234567890", userGroup.Key);

        // Try to access the recycle bin
        var response = await ClientRequest();

        // Should be forbidden because user doesn't have root element access
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}