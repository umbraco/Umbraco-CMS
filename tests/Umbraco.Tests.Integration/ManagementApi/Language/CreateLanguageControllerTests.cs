using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Language;
using Umbraco.Cms.Api.Management.ViewModels.Language;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Language;

public class CreateLanguageControllerTests : ManagementApiUserGroupTestBase<CreateLanguageController>
{
    protected override Expression<Func<CreateLanguageController, object>> MethodSelector => x => x.Create(CancellationToken.None, null);

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
        CreateLanguageRequestModel createLanguageModel = new() { IsoCode = "da-DK", Name = "Danish", IsDefault = false, IsMandatory = false };

        return await Client.PostAsync(Url, JsonContent.Create(createLanguageModel));
    }
}
