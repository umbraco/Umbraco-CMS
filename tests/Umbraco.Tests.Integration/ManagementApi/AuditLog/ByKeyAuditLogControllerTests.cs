using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.AuditLog;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.AuditLog;


[TestFixture]
public class ByKeyAuditLogControllerTests : ManagementApiBaseTest<ByKeyAuditLogController>
{
    protected override Expression<Func<ByKeyAuditLogController, object>> MethodSelector =>
        x => x.ByKey(Constants.Security.SuperUserKey, Direction.Ascending, null, 0, 100);

    protected override List<HttpStatusCode> AuthenticatedStatusCodes { get; } = new()
    {
        HttpStatusCode.OK
    };

    [Test]
    public virtual async Task As_Admin_I_Have_Access()
    {
        var response = await AccessAsAdmin();

        AssertStatusCode(response, true);
    }

    [Test]
    public virtual async Task As_Editor_I_Have_Access()
    {
        var response = await AccessAsEditor();

        AssertStatusCode(response, true);
    }

    [Test]
    public virtual async Task As_Sensitive_Data_I_Have_No_Access()
    {
        var response = await AccessAsSensitiveData();

        AssertStatusCode(response, false);
    }

    [Test]
    public virtual async Task As_Translator_I_Have_No_Access()
    {
        var response = await AccessAsTranslator();

        AssertStatusCode(response, false);
    }

    [Test]
    public virtual async Task As_Writer_I_Have_Access()
    {
        var response = await AccessAsWriter();

        AssertStatusCode(response, true);
    }
}
