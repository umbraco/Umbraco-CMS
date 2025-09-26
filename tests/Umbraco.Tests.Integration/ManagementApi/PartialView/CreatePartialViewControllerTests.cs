using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.PartialView;
using Umbraco.Cms.Api.Management.ViewModels.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.PartialView;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PartialView;

public class CreatePartialViewControllerTests : ManagementApiUserGroupTestBase<CreatePartialViewController>
{
    protected override Expression<Func<CreatePartialViewController, object>> MethodSelector =>
        x => x.Create(CancellationToken.None, null);

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
        CreatePartialViewRequestModel createPartialViewRequestModel = new() { Name = "TestPartialView.cshtml", Content = string.Empty, Parent = new FileSystemFolderModel() { Path = "test" } };

        return await Client.PostAsync(Url, JsonContent.Create(createPartialViewRequestModel));
    }
}
