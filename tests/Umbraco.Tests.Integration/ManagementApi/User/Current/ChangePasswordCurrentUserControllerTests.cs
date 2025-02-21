using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.User.Current;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User.Current;

public class
    ChangePasswordCurrentUserControllerTests : ManagementApiUserGroupTestBase<ChangePasswordCurrentUserController>
{
    protected override Expression<Func<ChangePasswordCurrentUserController, object>> MethodSelector => x => x.ChangePassword(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        ChangePasswordCurrentUserRequestModel changePasswordCurrentUserRequestModel = new()
        {
            OldPassword = "OldPassword",
            NewPassword = "NewPassword"
        };
        return await Client.PostAsync(Url, JsonContent.Create(changePasswordCurrentUserRequestModel));
    }
}
