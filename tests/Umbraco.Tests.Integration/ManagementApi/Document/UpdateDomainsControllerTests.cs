using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document;

public class UpdateDomainsControllerTests : ManagementApiUserGroupTestBase<UpdateDomainsController>
{
    protected override Expression<Func<UpdateDomainsController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, Guid.NewGuid(), null);

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
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
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
        UpdateDomainsRequestModel updateDomainsRequestModel = new() { DefaultIsoCode = "Test", Domains = null };

        return await Client.PutAsync(Url, JsonContent.Create(updateDomainsRequestModel));
    }
}
