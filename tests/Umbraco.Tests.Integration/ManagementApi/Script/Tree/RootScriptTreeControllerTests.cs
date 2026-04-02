using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Script.Tree;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.FileSystem;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Script.Tree;

public class RootScriptTreeControllerTests : ManagementApiUserGroupTestBase<RootScriptTreeController>
{
    private IScriptFolderService ScriptFolderService => GetRequiredService<IScriptFolderService>();

    [SetUp]
    public async Task SetUp()
    {
        var model = new ScriptFolderCreateModel() { Name = Guid.NewGuid().ToString() };
        await ScriptFolderService.CreateAsync(model);
    }

    protected override Expression<Func<RootScriptTreeController, object>> MethodSelector =>
        x => x.Root(CancellationToken.None, 0, 100);

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
