using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Stylesheet;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Stylesheet;

public class GetRichTextRulesByPathTests : ManagementApiUserGroupTestBase<GetRichTextRulesByPath>
{
    protected override Expression<Func<GetRichTextRulesByPath, object>> MethodSelector =>
        x => x.GetByPath("TestRuleName");

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
