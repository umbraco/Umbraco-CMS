using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.PartialView;
using Umbraco.Cms.Api.Management.ViewModels.PartialView;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PartialView;

public class UpdatePartialViewControllerTests : ManagementApiUserGroupTestBase<UpdatePartialViewController>
{
    protected override Expression<Func<UpdatePartialViewController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, "testPath", null);

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
        UpdatePartialViewRequestModel updatePartialViewRequestModel = new() { Content = string.Empty};

        return await Client.PutAsync(Url, JsonContent.Create(updatePartialViewRequestModel));
    }
}
