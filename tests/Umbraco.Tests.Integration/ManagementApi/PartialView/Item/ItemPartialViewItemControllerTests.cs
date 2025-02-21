using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.PartialView.Item;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PartialView.Item;

public class ItemPartialViewItemControllerTests : ManagementApiUserGroupTestBase<ItemPartialViewItemController>
{
    protected override Expression<Func<ItemPartialViewItemController, object>> MethodSelector =>
        x => x.Item(CancellationToken.None, new HashSet<string> { "TestPartialViewItem" });

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };
}
