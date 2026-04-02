using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;

using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType;

public class CreateDocumentTypeControllerTests : ManagementApiUserGroupTestBase<CreateDocumentTypeController>
{
    protected override Expression<Func<CreateDocumentTypeController, object>> MethodSelector =>
        x => x.Create(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created
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
        CreateDocumentTypeRequestModel createDocumentTypeRequestModel = new()
        {
            Alias = "test", Name = "Test", Id = Guid.NewGuid(), Icon = "icon-document",
        };

        return await Client.PostAsync(Url, JsonContent.Create(createDocumentTypeRequestModel));
    }
}
