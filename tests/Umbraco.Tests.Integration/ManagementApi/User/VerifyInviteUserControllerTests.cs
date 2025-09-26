using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.User;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.User;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User;

public class VerifyInviteUserControllerTests : ManagementApiUserGroupTestBase<VerifyInviteUserController>
{
    protected override Expression<Func<VerifyInviteUserController, object>> MethodSelector => x => x.Invite(CancellationToken.None, null);

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
        VerifyInviteUserRequestModel verifyInviteUserRequestModel = new() { User = new ReferenceByIdModel(Guid.Empty), Token = string.Empty };
        return await Client.PutAsync(Url, JsonContent.Create(verifyInviteUserRequestModel));
    }
}
