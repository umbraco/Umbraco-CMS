using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Script.Folder;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.FileSystem;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Script.Folder;

public class ByPathScriptFolderControllerTests : ManagementApiUserGroupTestBase<ByPathScriptFolderController>
{
    private IScriptFolderService ScriptFolderService => GetRequiredService<IScriptFolderService>();

    private string _scriptFolderPath;

    [SetUp]
    public async Task SetUp()
    {
        var model = new ScriptFolderCreateModel() { Name = Guid.NewGuid().ToString() };
        var response = await ScriptFolderService.CreateAsync(model);
        _scriptFolderPath = response.Result.Path;
    }

    protected override Expression<Func<ByPathScriptFolderController, object>> MethodSelector =>
        x => x.ByPath(CancellationToken.None, _scriptFolderPath);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
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
}
