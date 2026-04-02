using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Template;
using Umbraco.Cms.Api.Management.ViewModels.Template;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Template;

public class UpdateTemplateControllerTests : ManagementApiUserGroupTestBase<UpdateTemplateController>
{
    protected override Expression<Func<UpdateTemplateController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, Guid.Empty, null);

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
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        UpdateTemplateRequestModel updateTemplateModel = new()
        {
            Name = "UpdatedTestTemplate", Alias = "testTemplate", Content = "<h1>Test Template</h1>"
        };

        return await Client.PutAsync(Url, JsonContent.Create(updateTemplateModel));
    }
}
