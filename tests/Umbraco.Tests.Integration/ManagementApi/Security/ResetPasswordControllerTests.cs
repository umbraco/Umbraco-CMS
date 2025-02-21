using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

public class ResetPasswordControllerTests : ManagementApiUserGroupTestBase<ResetPasswordController>
{
    protected override Expression<Func<ResetPasswordController, object>> MethodSelector =>
        x => x.RequestPasswordReset(CancellationToken.None, null);

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
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        ResetPasswordRequestModel resetPasswordRequestModel = new() { Email = "testResetPassword@umbraco.com" };

        return await Client.PostAsync(Url, JsonContent.Create(resetPasswordRequestModel));
    }
}
