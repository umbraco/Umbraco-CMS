using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType;

public class CreateDocumentTypeTemplateControllerTests : ManagementApiUserGroupTestBase<CreateDocumentTypeTemplateController>
{
    protected override Expression<Func<CreateDocumentTypeTemplateController, object>> MethodSelector =>
        x => x.CreateTemplate(CancellationToken.None, Guid.Empty, null);

    // Admin has Settings access, so authorization passes and the request reaches the handler,
    // which returns NotFound because the (empty) document type key does not exist.
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
        CreateDocumentTypeTemplateRequestModel requestModel = new()
        {
            Name = "Test Template", Alias = "testTemplate"
        };

        return await Client.PostAsync(Url, JsonContent.Create(requestModel));
    }
}
