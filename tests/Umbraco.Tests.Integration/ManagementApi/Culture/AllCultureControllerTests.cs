using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Culture;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Culture;

[TestFixture]
public class AllCultureControllerTests : ManagementApiUserGroupTestBase<AllCultureController>
{
    protected override Expression<Func<AllCultureController, object>> MethodSelector =>
        x => x.GetAll(0, 100);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        Allowed = true, ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        Allowed = true, ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        Allowed = true, ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        Allowed = true, ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        Allowed = true, ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        Allowed = false, ExpectedStatusCode = HttpStatusCode.Unauthorized,
    };
}
