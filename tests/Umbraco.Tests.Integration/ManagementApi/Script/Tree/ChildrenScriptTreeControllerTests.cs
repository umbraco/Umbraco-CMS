using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Script.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.FileSystem;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Script.Tree;

public class ChildrenScriptTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenScriptTreeController>
{
    private IScriptFolderService ScriptFolderService => GetRequiredService<IScriptFolderService>();

    private IScriptService ScriptService => GetRequiredService<IScriptService>();

    private string _scriptFolderPath;

    [SetUp]
    public async Task SetUp()
    {
        // Script Folder
        var folderModel = new ScriptFolderCreateModel() { Name = Guid.NewGuid().ToString() };
        var responseFolder = await ScriptFolderService.CreateAsync(folderModel);
        _scriptFolderPath = responseFolder.Result.Path;

        // Script
        var model = new ScriptCreateModel() { Name = Guid.NewGuid() + ".js", ParentPath = _scriptFolderPath };
        await ScriptService.CreateAsync(model, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ChildrenScriptTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, _scriptFolderPath, 0, 100);

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
