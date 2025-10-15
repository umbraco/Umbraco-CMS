using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.UserGroup.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.UserGroup.Item;

public class ItemUserGroupItemControllerTests : ManagementApiUserGroupTestBase<ItemUserGroupItemController>
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private Guid _userGroupKey;

    [SetUp]
    public async Task SetUp()
    {
        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        _userGroupKey = userGroup.Key;
    }

    protected override Expression<Func<ItemUserGroupItemController, object>> MethodSelector =>
        x => x.Item(CancellationToken.None, new HashSet<Guid> { _userGroupKey });

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
