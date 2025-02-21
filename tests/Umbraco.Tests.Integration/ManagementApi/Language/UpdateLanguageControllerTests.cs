using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Language;
using Umbraco.Cms.Api.Management.ViewModels.Language;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Language;

public class UpdateLanguageControllerTests : ManagementApiUserGroupTestBase<UpdateLanguageController>
{
    protected override Expression<Func<UpdateLanguageController, object>> MethodSelector => x => x.Update(CancellationToken.None, "da", null);

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
        UpdateLanguageRequestModel updateLanguageModel = new() { Name = "TestUpdatedLanguage", IsDefault = false, FallbackIsoCode = "vi", IsMandatory = true };

        return await Client.PutAsync(Url, JsonContent.Create(updateLanguageModel));
    }
}
