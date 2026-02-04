using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.Tree;

public class AncestorsElementTreeControllerTests : ManagementApiUserGroupTestBase<AncestorsElementTreeController>
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private Guid _grandparentKey;
    private Guid _parentKey;
    private int _parentId;

    [SetUp]
    public async Task Setup()
    {
        // Create grandparent container
        var grandparentResult = await ElementContainerService.CreateAsync(null, $"GrandparentContainer {Guid.NewGuid()}", null, Constants.Security.SuperUserKey);
        Assert.IsTrue(grandparentResult.Success, $"Failed to create grandparent: {grandparentResult.Status}");
        _grandparentKey = grandparentResult.Result!.Key;

        // Create parent container
        var parentResult = await ElementContainerService.CreateAsync(null, $"ParentContainer {Guid.NewGuid()}", _grandparentKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(parentResult.Success, $"Failed to create parent: {parentResult.Status}");
        _parentKey = parentResult.Result!.Key;
        _parentId = parentResult.Result!.Id;
    }

    protected override Expression<Func<AncestorsElementTreeController, object>> MethodSelector =>
        x => x.Ancestors(CancellationToken.None, _parentKey);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };

    [Test]
    public async Task User_With_Start_Node_Can_Access_Ancestors()
    {
        // Create a user group with Library section access and start node = parent folder
        var userGroup = new UserGroupBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName("Test Group With Element Start Node")
            .WithAllowedSections(["library"])
            .WithStartElementId(_parentId)
            .Build();

        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        // Authenticate as a user in the group with the restricted start node
        await AuthenticateClientAsync(Client, $"startnodetest{Guid.NewGuid():N}@umbraco.com", "1234567890", userGroup.Key);

        // Get ancestors of the parent folder (user's start node)
        var response = await ClientRequest();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<ElementTreeItemResponseModel>>(JsonSerializerOptions);
        Assert.IsNotNull(result);
        var ancestors = result.ToList();

        // Ancestors should include grandparent and parent (the target itself)
        Assert.AreEqual(2, ancestors.Count, "Should return grandparent and parent");
        Assert.AreEqual(_grandparentKey, ancestors[0].Id, "First ancestor should be grandparent");
        Assert.AreEqual(_parentKey, ancestors[1].Id, "Second ancestor should be parent (target)");
    }
}
