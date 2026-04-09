using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.UserGroup;

public class UpdateUserGroupControllerTests : ManagementApiUserGroupTestBase<UpdateUserGroupController>
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

    protected override Expression<Func<UpdateUserGroupController, object>> MethodSelector => x => x.Update(CancellationToken.None, _userGroupKey, null);

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
        UpdateUserGroupRequestModel updateUserGroupRequest = new()
        {
            Name = "UpdatedTestGroup",
            Alias = "testAlias",
            Description = "Updated test group description",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = [],
            Sections = ["Umb.Section.Content"],
            Permissions = new HashSet<IPermissionPresentationModel> { },
        };

        return await Client.PutAsync(Url, JsonContent.Create(updateUserGroupRequest));
    }
}
