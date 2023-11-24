using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.AuditLog;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Policies;

/// <summary>
///
/// </summary>
[TestFixture]
public class ByKeyAuditLogControllerTests : ManagementApiTest<ByKeyAuditLogController>
{
    protected override Expression<Func<ByKeyAuditLogController, object>> MethodSelector =>
        x => x.ByKey(Constants.Security.SuperUserKey, Direction.Ascending, null, 0, 100);

    [Test]
    public virtual async Task As_Admin_I_Have_Access()
    {
        await AuthenticateClientAsync(Client, "admin@umbraco.com", "1234567890", true);

        var response = await Client.GetAsync(Url);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,  await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Editor_I_Have_Access()
    {
        await AuthenticateClientAsync(Client, "editor@umbraco.com", "1234567890", false);

        var response = await Client.GetAsync(Url);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,  await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task Unauthourized_when_no_token_is_provided()
    {
        var response = await Client.GetAsync(Url);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}
