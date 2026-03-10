using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Profiling;
using Umbraco.Cms.Api.Management.ViewModels.Profiling;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Profiling;

public class UpdateStatusProfilingControllerTests : ManagementApiUserGroupTestBase<UpdateStatusProfilingController>
{
    protected override Expression<Func<UpdateStatusProfilingController, object>> MethodSelector =>
        x => x.Status(CancellationToken.None, null);

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

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        ProfilingStatusRequestModel profilingStatusRequestModel = new ProfilingStatusRequestModel(true);

        return await Client.PutAsync(Url, JsonContent.Create(profilingStatusRequestModel));
    }
}
