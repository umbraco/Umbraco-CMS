using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Script;
using Umbraco.Cms.Api.Management.ViewModels.Script;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Script;

public class UpdateScriptControllerTests : ManagementApiUserGroupTestBase<UpdateScriptController>
{
    protected override Expression<Func<UpdateScriptController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, "testUpdateScript.js", null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
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
        UpdateScriptRequestModel updateScriptRequestModel = new() { Content = "TestUpdatedContent" };

        return await Client.PutAsync(Url, JsonContent.Create(updateScriptRequestModel));
    }
}
