using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.PartialView.Items;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PartialView.Items;

public class ItemPartialViewItemControllerTests : ManagementApiUserGroupTestBase<ItemPartialViewItemController>
{
    protected override Expression<Func<ItemPartialViewItemController, object>> MethodSelector =>
        x => x.Item(new HashSet<string> { "TestPartialViewItem" });

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
