using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Stylesheet.Folder;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Folder;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Stylesheet.Folder;

public class CreateStylesheetFolderControllerTests : ManagementApiUserGroupTestBase<CreateStylesheetFolderController>
{
    protected override Expression<Func<CreateStylesheetFolderController, object>> MethodSelector =>
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
        CreateStylesheetFolderRequestModel createStylesheetFolderRequestModel = new() { Name = Guid.NewGuid().ToString() };

        return await Client.PostAsync(Url, JsonContent.Create(createStylesheetFolderRequestModel));
    }
}
