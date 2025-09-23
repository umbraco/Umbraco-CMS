using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Template;
using Umbraco.Cms.Api.Management.ViewModels.Template;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Template;

public class CreateTemplateControllerTests : ManagementApiUserGroupTestBase<CreateTemplateController>
{
    protected override Expression<Func<CreateTemplateController, object>> MethodSelector => x => x.Create(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created
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
        ExpectedStatusCode = HttpStatusCode.Created
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        CreateTemplateRequestModel createTemplateModel = new()
        {
            Name = Guid.NewGuid().ToString(), Alias = Guid.NewGuid().ToString(), Content = "<h1>Test Template</h1>"
        };

        return await Client.PostAsync(Url, JsonContent.Create(createTemplateModel));
    }
}
