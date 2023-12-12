using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

public class ResetPasswordTokenControllerTests : ManagementApiUserGroupTestBase<ResetPasswordTokenController>
{
    protected override Expression<Func<ResetPasswordTokenController, object>> MethodSelector =>
        x => x.ResetPasswordToken(null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        ResetPasswordTokenRequestModel resetPasswordTokenRequestModel = new() { UserId = Guid.NewGuid(), Password = "0123456789" };

        return await Client.PostAsync(Url, JsonContent.Create(resetPasswordTokenRequestModel));
    }
}
