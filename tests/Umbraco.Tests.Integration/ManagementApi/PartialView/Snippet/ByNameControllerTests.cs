using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.PartialView.Snippet;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PartialView.Snippet;

public class ByNameControllerTests : ManagementApiUserGroupTestBase<ByNameController>
{
    protected override Expression<Func<ByNameController, object>> MethodSelector =>
        x => x.GetByName("TestPartialViewByName.cshtml");

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
