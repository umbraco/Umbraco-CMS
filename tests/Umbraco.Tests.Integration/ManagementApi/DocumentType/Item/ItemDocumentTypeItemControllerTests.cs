using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.DocumentType.Item;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType.Item;

public class ItemDocumentTypeItemControllerTests : ManagementApiUserGroupTestBase<ItemDocumentTypeItemController>
{
    protected override Expression<Func<ItemDocumentTypeItemController, object>> MethodSelector =>
        x => x.Item(CancellationToken.None, new HashSet<Guid> { Guid.NewGuid() });

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
        ExpectedStatusCode = HttpStatusCode.OK,
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
