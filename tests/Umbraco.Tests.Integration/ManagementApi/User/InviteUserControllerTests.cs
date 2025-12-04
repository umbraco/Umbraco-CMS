using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.User;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User;

public class InviteUserControllerTests : ManagementApiUserGroupTestBase<InviteUserController>
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private Guid _userGroupKey;

    [SetUp]
    public async Task SetUp()
    {
        var adminUserGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        _userGroupKey = adminUserGroup.Key;
    }

    protected override Expression<Func<InviteUserController, object>> MethodSelector => x => x.Invite(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.InternalServerError, // We expect an error here because email sending is not configured in these tests.
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
        var stringKey = Guid.NewGuid();

        InviteUserRequestModel inviteUserRequestModel = new()
        {
            Message = "Welcome to Umbraco!",
            Email = stringKey + "@test.com",
            UserName = stringKey + "@test.com",
            Name = "test",
            Id = Guid.NewGuid(),
            UserGroupIds = new HashSet<ReferenceByIdModel> { new(_userGroupKey) },
        };
        return await Client.PostAsync(Url, JsonContent.Create(inviteUserRequestModel));
    }
}
