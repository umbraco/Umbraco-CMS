using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Script.Item;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Script.Item;

public class ItemScriptItemControllerTests : ManagementApiUserGroupTestBase<ItemScriptItemController>
{
    protected override Expression<Func<ItemScriptItemController, object>> MethodSelector =>
        x => x.Item(new HashSet<string> { "TestItemScript" });

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
