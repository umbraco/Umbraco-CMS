using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Controllers.Element.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.Tree;

public class ChildrenElementTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenElementTreeController>
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private Guid _parentKey;

    [SetUp]
    public async Task Setup()
    {
        // Create element type
        var elementType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString())
            .WithName($"Test Element {Guid.NewGuid()}")
            .WithIsElement(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        // Create parent container
        var parentResult = await ElementContainerService.CreateAsync(null, $"ParentContainer {Guid.NewGuid()}", null, Constants.Security.SuperUserKey);
        Assert.IsTrue(parentResult.Success, $"Failed to create parent: {parentResult.Status}");
        _parentKey = parentResult.Result!.Key;

        // Create child container
        var childContainerResult = await ElementContainerService.CreateAsync(null, $"ChildContainer {Guid.NewGuid()}", _parentKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(childContainerResult.Success, $"Failed to create child container: {childContainerResult.Status}");

        // Create child element
        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = _parentKey,
            Variants = [new VariantModel { Name = $"ChildElement {Guid.NewGuid()}" }],
        };
        await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ChildrenElementTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, _parentKey, 0, 100, false);

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
    public async Task User_With_Start_Node_Cannot_Access_Children_Outside_Start_Node()
    {
        // Create another folder to be the user's start node (different from the one in Setup)
        var startNodeResult = await ElementContainerService.CreateAsync(
            null,
            $"Start Node Folder {Guid.NewGuid()}",
            _parentKey,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(startNodeResult.Success, $"Failed to create start node folder: {startNodeResult.Status}");
        var startNodeFolder = startNodeResult.Result!;

        // Create a user group with Library section access but with a non-root element start node
        var userGroup = new UserGroupBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName("Test Group With Element Start Node")
            .WithAllowedSections(["library"])
            .WithStartElementId(startNodeFolder.Id)
            .Build();

        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        // Authenticate as a user in the group with the restricted start node
        await AuthenticateClientAsync(Client, $"startnodetest{Guid.NewGuid():N}@umbraco.com", "1234567890", userGroup.Key);

        // Try to access the children of the parent folder created in Setup (which is outside the start node)
        var response = await ClientRequest();

        // Should succeed
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        // Parse response and verify only folder1 is returned
        var result = await response.Content.ReadFromJsonAsync<PagedViewModel<ElementTreeItemResponseModel>>(JsonSerializerOptions);
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Total);
        Assert.AreEqual(startNodeFolder.Key, result.Items.First().Id);
    }
}
