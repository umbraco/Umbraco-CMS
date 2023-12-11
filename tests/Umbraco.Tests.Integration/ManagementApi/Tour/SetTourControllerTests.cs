using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Tour;
using Umbraco.Cms.Api.Management.ViewModels.Tour;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Tour;

public class SetTourControllerTests : ManagementApiUserGroupTestBase<SetTourController>
{
    protected override Expression<Func<SetTourController, object>> MethodSelector => x => x.SetTour(null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        SetTourStatusRequestModel setTourStatusRequest = new() { Alias = "testTour", Disabled = false };

        return await Client.PostAsync(Url, JsonContent.Create(setTourStatusRequest));
    }
}
