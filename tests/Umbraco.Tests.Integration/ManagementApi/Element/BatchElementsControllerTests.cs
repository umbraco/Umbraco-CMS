using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element;

public class BatchElementsControllerTests : ManagementApiUserGroupTestBase<BatchElementsController>
{
    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private Guid _elementKey;

    [SetUp]
    public async Task Setup()
    {
        _elementKey = await CreateElement("Test Element Instance");
    }

    protected override Expression<Func<BatchElementsController, object>> MethodSelector =>
        x => x.Batch(CancellationToken.None, new HashSet<Guid>());

    protected override async Task<HttpResponseMessage> ClientRequest()
        => await Client.GetAsync($"{Url}?id={_elementKey}");

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    // SensitiveData and Translator lack Library section access, but the batch endpoint is not gated on
    // section access - an inaccessible id is simply omitted from the result rather than failing the request.
    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };

    [Test]
    public async Task As_Sensitive_Data_I_Get_No_Items()
    {
        var response = await AuthorizedRequest(Constants.Security.SensitiveDataGroupKey, "SensitiveData");
        var body = await response.Content.ReadFromJsonAsync<BatchResponseModel<ElementResponseModel>>(JsonSerializerOptions);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(0, body!.Total);
        Assert.IsEmpty(body.Items);
    }

    [Test]
    public async Task Batch_Returns_Only_The_Elements_The_Current_User_May_Browse()
    {
        // Arrange - two elements, a group that can only browse one of them.
        var accessibleElementKey = _elementKey;
        var inaccessibleElementKey = await CreateElement("Inaccessible Element Instance");

        var userGroup = new UserGroupBuilder()
            .WithName(Guid.NewGuid().ToString())
            .WithAlias(Guid.NewGuid().ToString())
            .WithAllowedSections([Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Library])
            .WithPermissions(new HashSet<string>())
            .WithGranularPermissions(
            [
                new ElementGranularPermission { Key = accessibleElementKey, Permission = ActionElementBrowse.ActionLetter },
            ])
            .Build();
        var createGroupResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(createGroupResult.Success, $"Failed to create user group with status {createGroupResult.Status}.");

        await AuthenticateClientAsync(Client, $"{Guid.NewGuid()}@test.com", UserPassword, userGroup.Key);

        // Act
        var response = await Client.GetAsync($"{Url}?id={accessibleElementKey}&id={inaccessibleElementKey}");
        var body = await response.Content.ReadFromJsonAsync<BatchResponseModel<ElementResponseModel>>(JsonSerializerOptions);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        Assert.AreEqual(1, body!.Total);
        Assert.That(body.Items.Select(i => i.Id), Is.EquivalentTo(new[] { accessibleElementKey }));
    }

    private async Task<Guid> CreateElement(string name)
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString())
            .WithName(Guid.NewGuid().ToString())
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = null,
            Variants = [new VariantModel { Name = name }],
        };
        var response = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(response.Success, $"Failed to create element: {response.Status}");
        return response.Result!.Content!.Key;
    }
}
