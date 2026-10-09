using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.User.Current;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

public class BackOfficeCookieRedirectTests : ManagementApiTest<GetCurrentUserController>
{
    protected override Expression<Func<GetCurrentUserController, object>> MethodSelector { get; set; } =
        x => x.GetCurrentUser(CancellationToken.None);

    [Test]
    public async Task Unauthenticated_Management_Api_Request_Gets_Unauthorized_Instead_Of_Redirect()
    {
        HttpResponseMessage response = await Client.GetAsync(Url);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Test]
    public async Task Unauthenticated_Authorize_Request_Redirects_To_Login()
    {
        // Ensures the back office OAuth client is registered before the authorize request is validated.
        await Client.GetAsync(Url);

        var query = string.Join(
            "&",
            "client_id=" + Constants.OAuthClientIds.BackOffice,
            "response_type=code",
            "redirect_uri=" + Uri.EscapeDataString("https://localhost/umbraco"),
            "code_challenge=" + new string('a', 43),
            "code_challenge_method=S256");

        HttpResponseMessage response = await Client.GetAsync($"/umbraco/management/api/v1/security/back-office/authorize?{query}");

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode, await response.Content.ReadAsStringAsync());
        Assert.AreEqual("/umbraco/login", response.Headers.Location?.AbsolutePath);
    }
}
