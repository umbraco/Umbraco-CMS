using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document;

public class UpdatePublicAccessDocumentControllerTests : ManagementApiUserGroupTestBase<UpdatePublicAccessDocumentController>
{
    protected override Expression<Func<UpdatePublicAccessDocumentController, object>> MethodSelector =>
        x => x.Update(Guid.NewGuid(), null);

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
        ExpectedStatusCode = HttpStatusCode.Forbidden,
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
        PublicAccessRequestModel publicAccessRequestModel = new()
        {
            MemberUserNames = Array.Empty<string>(),
            MemberGroupNames = Array.Empty<string>(),
            LoginPageId = Guid.NewGuid(),
            ErrorPageId = Guid.NewGuid(),
        };

        return await Client.PutAsync(Url, JsonContent.Create(publicAccessRequestModel));
    }

}
