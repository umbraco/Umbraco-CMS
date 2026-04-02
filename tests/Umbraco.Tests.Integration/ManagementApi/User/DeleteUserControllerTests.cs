using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User;

[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DeleteUserControllerTests : ManagementApiUserGroupTestBase<DeleteUserController>
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

    protected override Expression<Func<DeleteUserController, object>> MethodSelector => x => x.DeleteUser(CancellationToken.None, _userKey);

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

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.DeleteAsync(Url);
}
