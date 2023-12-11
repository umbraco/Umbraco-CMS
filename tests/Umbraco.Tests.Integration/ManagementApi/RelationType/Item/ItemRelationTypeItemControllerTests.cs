using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.RelationType.Item;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.RelationType.Item;

public class ItemRelationTypeItemControllerTests : ManagementApiUserGroupTestBase<ItemRelationTypeItemController>
{
    protected override Expression<Func<ItemRelationTypeItemController, object>> MethodSelector =>
        x => x.Item(new HashSet<Guid> { Guid.NewGuid() });

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
