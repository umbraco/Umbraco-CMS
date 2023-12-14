using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.TrackedReference;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.TrackedReference;

public class ItemsTrackedReferenceControllerTests : ManagementApiUserGroupTestBase<ItemsTrackedReferenceController>
{
    protected override Expression<Func<ItemsTrackedReferenceController, object>> MethodSelector => x =>
        x.GetPagedReferencedItems(new HashSet<Guid> { Guid.Empty }, 0, 100, false);

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
        ExpectedStatusCode = HttpStatusCode.Forbidden
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
