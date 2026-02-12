using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.TemporaryFile;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

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
        CreateTemporaryFileRequestModel createTemporaryFileRequest = new() { Id = Guid.NewGuid(), File = null! };

        return await Client.PostAsync(Url, JsonContent.Create(createTemporaryFileRequest));
    }
}
