using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

public class VerifyResetPasswordTokenControllerTests : ManagementApiUserGroupTestBase<VerifyResetPasswordTokenController>
{
    protected override Expression<Func<VerifyResetPasswordTokenController, object>> MethodSelector =>
        x => x.VerifyResetPasswordToken(CancellationToken.None, null);

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
        VerifyResetPasswordTokenRequestModel verifyResetPasswordTokenRequestModel = new() { User = new ReferenceByIdModel(Guid.NewGuid()) , ResetCode = "test" };

        return await Client.PostAsync(Url, JsonContent.Create(verifyResetPasswordTokenRequestModel));
    }
}
