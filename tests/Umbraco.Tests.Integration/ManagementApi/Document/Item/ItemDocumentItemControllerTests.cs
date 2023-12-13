using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Document.Item;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document.Item;

public class ItemDocumentItemControllerTests : ManagementApiUserGroupTestBase<ItemDocumentItemController>
{
    protected override Expression<Func<ItemDocumentItemController, object>> MethodSelector =>
        x => x.Item(new HashSet<Guid> { Guid.NewGuid() }, null, null);

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
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
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
