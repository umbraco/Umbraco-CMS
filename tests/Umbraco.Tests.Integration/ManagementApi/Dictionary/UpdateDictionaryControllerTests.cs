using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary;

public class UpdateDictionaryControllerTests : ManagementApiUserGroupTestBase<UpdateDictionaryController>
{
    protected override Expression<Func<UpdateDictionaryController, object>> MethodSelector =>
            x => x.Update(CancellationToken.None, Guid.NewGuid(), null);

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
        ExpectedStatusCode = HttpStatusCode.NotFound
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
        UpdateDictionaryItemRequestModel updateDictionaryItemRequestModel =
            new() { Name = "TestDictionary", Translations = { } };
        return await Client.PutAsync(Url, JsonContent.Create(updateDictionaryItemRequestModel));
    }
}
