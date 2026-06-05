using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.TemporaryFile;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.TemporaryFile;

public class CreateTemporaryFileControllerTests : ManagementApiUserGroupTestBase<CreateTemporaryFileController>
{
    protected override Expression<Func<CreateTemporaryFileController, object>> MethodSelector => x => x.Create(CancellationToken.None, null);

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
        // The endpoint only consumes multipart/form-data, so the request must be sent as such to reach the action.
        // The required file is intentionally omitted, yielding the expected BadRequest for authenticated users.
        var content = new MultipartFormDataContent { { new StringContent(Guid.NewGuid().ToString()), "Id" } };

        return await Client.PostAsync(Url, content);
    }
}
