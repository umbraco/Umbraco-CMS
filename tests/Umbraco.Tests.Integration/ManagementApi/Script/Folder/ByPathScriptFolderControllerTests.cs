using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Script.Folder;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Script.Folder;

public class ByPathScriptFolderControllerTests : ManagementApiUserGroupTestBase<ByPathScriptFolderController>
{
    protected override Expression<Func<ByPathScriptFolderController, object>> MethodSelector =>
        x => x.ByPath(CancellationToken.None, "TestScriptFolder");

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
}
