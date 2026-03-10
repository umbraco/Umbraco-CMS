using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.UserGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.UserGroup;

public class ByKeyUserGroupControllerTests : ManagementApiUserGroupTestBase<ByKeyUserGroupController>
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

    protected override Expression<Func<ByKeyUserGroupController, object>> MethodSelector => x => x.ByKey(CancellationToken.None, _userGroupKey);

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
}
