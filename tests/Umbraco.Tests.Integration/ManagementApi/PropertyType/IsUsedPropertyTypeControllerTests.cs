using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.PropertyType;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PropertyType;

public class IsUsedPropertyTypeControllerTests : ManagementApiUserGroupTestBase<IsUsedPropertyTypeController>
{
    protected override Expression<Func<IsUsedPropertyTypeController, object>> MethodSelector =>
        x => x.Get(CancellationToken.None, Guid.NewGuid(), "test");

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
