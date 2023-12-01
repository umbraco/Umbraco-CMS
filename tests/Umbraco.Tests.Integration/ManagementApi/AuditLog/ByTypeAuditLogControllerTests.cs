using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.AuditLog;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.AuditLog;

[TestFixture]
public class ByTypeAuditLogControllerTests : ManagementApiUserGroupTestBase<ByTypeAuditLogController>
{
    protected override Expression<Func<ByTypeAuditLogController, object>> MethodSelector =>
        x => x.ByType(AuditType.New, null, 0, 100);

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
