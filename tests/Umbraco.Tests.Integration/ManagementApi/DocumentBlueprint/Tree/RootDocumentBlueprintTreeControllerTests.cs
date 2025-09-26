using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Tree;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentBlueprint.Tree;

public class RootDocumentBlueprintTreeControllerTests : ManagementApiUserGroupTestBase<RootDocumentBlueprintTreeController>
{
    protected override Expression<Func<RootDocumentBlueprintTreeController, object>> MethodSelector =>
        x => x.Root(CancellationToken.None, 0, 100, false);

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
