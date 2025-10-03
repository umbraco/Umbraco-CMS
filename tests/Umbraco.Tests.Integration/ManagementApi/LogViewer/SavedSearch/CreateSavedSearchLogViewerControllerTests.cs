using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.LogViewer.SavedSearch;

public class CreateSavedSearchLogViewerControllerTests : ManagementApiUserGroupTestBase<CreateSavedSearchLogViewerController>
{
    protected override Expression<Func<CreateSavedSearchLogViewerController, object>> MethodSelector => x => x.Create(CancellationToken.None, null);

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
        SavedLogSearchRequestModel savedLogSearchRequestModel = new() { Name = "Test", Query = "Tester" };

        return await Client.PostAsync(Url, JsonContent.Create(savedLogSearchRequestModel));
    }
}
