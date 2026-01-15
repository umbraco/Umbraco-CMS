using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Controllers.Element.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.Tree;

public class SiblingsElementTreeControllerTests : ManagementApiUserGroupTestBase<SiblingsElementTreeController>
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private Guid _folder1Key;
    private int _folder1Id;

    [SetUp]
    public async Task Setup()
    {
        // Create two folders at root level (siblings of each other)
        var folder1Result = await ElementContainerService.CreateAsync(null, $"Folder1 {Guid.NewGuid()}", null, Constants.Security.SuperUserKey);
        Assert.IsTrue(folder1Result.Success, $"Failed to create folder1: {folder1Result.Status}");
        _folder1Key = folder1Result.Result!.Key;
        _folder1Id = folder1Result.Result!.Id;

        var folder2Result = await ElementContainerService.CreateAsync(null, $"Folder2 {Guid.NewGuid()}", null, Constants.Security.SuperUserKey);
        Assert.IsTrue(folder2Result.Success, $"Failed to create folder2: {folder2Result.Status}");
    }

    protected override Expression<Func<SiblingsElementTreeController, object>> MethodSelector =>
        x => x.Siblings(CancellationToken.None, _folder1Key, 10, 10, false);

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
    public async Task User_With_Start_Node_Only_Sees_Accessible_Siblings()
    {
        // User's start node is folder1, so they have no access to folder2 (its sibling)
        var userGroup = new UserGroupBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName("Test Group With Element Start Node")
            .WithAllowedSections(["library"])
            .WithStartElementId(_folder1Id)
            .Build();

        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        await AuthenticateClientAsync(Client, $"startnodetest{Guid.NewGuid():N}@umbraco.com", "1234567890", userGroup.Key);

        // Get siblings of folder1 (folder2 is a sibling but user has no access)
        var response = await ClientRequest();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        var result = await response.Content.ReadFromJsonAsync<SubsetViewModel<ElementTreeItemResponseModel>>(JsonSerializerOptions);
        Assert.IsNotNull(result);

        // Only folder1 (the target) should be returned; folder2 should be filtered out completely
        Assert.AreEqual(1, result.Items.Count(), "Only the target folder should be returned");
        Assert.AreEqual(_folder1Key, result.Items.First().Id, "The target folder should be folder1");

        // No accessible siblings before or after the target
        Assert.AreEqual(0, result.TotalBefore, "No accessible siblings before");
        Assert.AreEqual(0, result.TotalAfter, "No accessible siblings after");
    }
}

