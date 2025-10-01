using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.User.Current;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User.Current;

public class GetMediaPermissionsCurrentUserControllerTests : ManagementApiUserGroupTestBase<GetMediaPermissionsCurrentUserController>
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private Guid _userKey;

    [SetUp]
    public async Task SetUp()
    {
        var adminUserGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);

        var stringKey = Guid.NewGuid();
        var model = new UserCreateModel()
        {
            Email = stringKey + "@test.com",
            UserName = stringKey + "@test.com",
            Name = stringKey.ToString(),
            UserGroupKeys = new HashSet<Guid> { adminUserGroup.Key },
        };
        var response = await UserService.CreateAsync(Constants.Security.SuperUserKey, model);
        _userKey = response.Result.CreatedUser.Key;
    }

    protected override Expression<Func<GetMediaPermissionsCurrentUserController, object>> MethodSelector => x => x.GetPermissions(CancellationToken.None, new HashSet<Guid> { _userKey });

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };
}
