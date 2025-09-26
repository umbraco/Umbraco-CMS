using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Script.Folder;
using Umbraco.Cms.Api.Management.ViewModels.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Api.Management.ViewModels.Script.Folder;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Script.Folder;

public class CreateScriptFolderControllerTests : ManagementApiUserGroupTestBase<CreateScriptFolderController>
{
    protected override Expression<Func<CreateScriptFolderController, object>> MethodSelector =>
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
        CreateScriptFolderRequestModel createScriptFolderRequestModel = new() { Name = "TestCreateScriptFolder", Parent = new FileSystemFolderModel() { Path = "test" } };

        return await Client.PostAsync(Url, JsonContent.Create(createScriptFolderRequestModel));
    }
}
