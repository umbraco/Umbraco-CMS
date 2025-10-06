using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Json.Patch;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.UserGroup;

public class BulkDeleteUserGroupControllerTests : ManagementApiUserGroupTestBase<BulkDeleteUserGroupsController>
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private Guid _userGroupKey;

    [SetUp]
    public async Task SetUp()
    {
        var userGroupModel = UserGroupBuilder.CreateUserGroup();
        var userGroup = await UserGroupService.CreateAsync(userGroupModel, Constants.Security.SuperUserKey);
        _userGroupKey = userGroup.Result.Key;
    }

    protected override Expression<Func<BulkDeleteUserGroupsController, object>> MethodSelector => x => x.BulkDelete(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        var model = new DeleteUserGroupsRequestModel
        {
            UserGroupIds = new HashSet<ReferenceByIdModel> { new(_userGroupKey) },
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, Url)
        {
            Content = JsonContent.Create(model),
        };

        return await Client.SendAsync(request);
    }
}
