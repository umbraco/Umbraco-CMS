using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using OpenIddict.Abstractions;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

public class BackOfficeControllerSignoutTests : ManagementApiTest<BackOfficeController>
{
    protected override Expression<Func<BackOfficeController, object>> MethodSelector { get; set; } =
        x => x.Signout(CancellationToken.None);

    private IUserService UserService => GetRequiredService<IUserService>();

    [Test]
    public async Task Signout_Updates_The_Security_Stamp_Of_The_Signed_In_User()
    {
        const string email = "signout-stamp@umbraco.com";
        await AuthenticateClientAsync(Client, email, UserPassword, Constants.Security.EditorGroupKey);
        var stampBeforeSignout = GetUser(email).SecurityStamp;

        await SignOutAsync();

        Assert.AreNotEqual(stampBeforeSignout, GetUser(email).SecurityStamp);
    }

    [Test]
    public async Task Signout_Ends_The_Login_Session_Of_The_Signed_In_User()
    {
        const string email = "signout-session@umbraco.com";
        await AuthenticateClientAsync(Client, email, UserPassword, Constants.Security.EditorGroupKey);
        var userId = GetUser(email).Id;
        Assert.IsNotEmpty(GetLoginSessions(userId).Where(session => session.LoggedOut is null));

        await SignOutAsync();

        Assert.IsEmpty(GetLoginSessions(userId).Where(session => session.LoggedOut is null));
    }

    /// <summary>
    /// Verifies that a backoffice cookie captured before sign-out cannot be used to authenticate after it,
    /// for example by an attacker who copied it from the browser.
    /// </summary>
    /// <remarks>
    /// With the default <c>AllowConcurrentLogins = false</c>, the security stamp is validated on every
    /// cookie-authenticated request, so the cookie itself is rejected. With concurrent logins allowed, the stamp is
    /// only re-checked once per validation interval, so the cookie still authenticates until then and the authorize
    /// endpoint must refuse it. <see cref="BackOfficeControllerSignoutWithConcurrentLoginsTests"/> covers that case.
    /// </remarks>
    [Test]
    public async Task Signout_Prevents_The_Backoffice_Cookie_From_Being_Replayed()
    {
        // Create the user and sets its password. The client authenticated here is not used further.
        const string email = "signout-replay@umbraco.com";
        await AuthenticateClientAsync(Client, email, UserPassword, Constants.Security.EditorGroupKey);

        // Create an HttpClient that does not store cookies, so every request carries exactly the cookie
        // the test attaches. This lets the test keep sending the cookie after sign-out has told the browser
        // to delete it, as an attacker holding a copy would. Redirects are not followed, so the authorize
        // response can be inspected.
        using HttpClient cookielessClient = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false,
            BaseAddress = Client.BaseAddress,
        });

        // Log in and capture the backoffice cookie (UMB_UCONTEXT by default) from the response.
        HttpResponseMessage loginResponse = await cookielessClient.PostAsync(
            GetManagementApiUrl<BackOfficeController>(x => x.Login(CancellationToken.None, null)),
            JsonContent.Create(new LoginRequestModel { Username = email, Password = UserPassword }));
        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode, await loginResponse.Content.ReadAsStringAsync());
        var authCookie = GetAuthCookie(loginResponse);

        // Before sign-out the cookie must be accepted. Without this check, the final assertion could pass
        // because the cookie never worked, rather than because sign-out invalidated it.
        Assert.IsTrue(await AuthCookieIsAcceptedAsync(cookielessClient, authCookie));

        // Sign out, authenticated by the cookie alone, as the backoffice does once it has revoked its tokens.
        // This rotates the user's security stamp and ends the login session the cookie belongs to.
        using var signoutRequest = new HttpRequestMessage(HttpMethod.Get, Url);
        signoutRequest.Headers.Add("Cookie", authCookie);
        await cookielessClient.SendAsync(signoutRequest);

        // Replay the same cookie. Its security stamp no longer matches the user's, so the cookie is rejected
        // and the authorize endpoint sends the client to the login screen instead of issuing a code.
        Assert.IsFalse(await AuthCookieIsAcceptedAsync(cookielessClient, authCookie));
    }

    // Mirrors the backoffice logout: the tokens are revoked before the sign-out endpoint is requested,
    // so the sign-out request is authenticated by the cookie alone.
    private async Task SignOutAsync()
    {
        HttpResponseMessage revokeResponse = await Client.PostAsync(
            "/umbraco/management/api/v1/security/back-office/revoke",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = Constants.OAuthClientIds.BackOffice,
                ["token"] = "[redacted]",
                ["token_type_hint"] = "access_token",
            }));
        Assert.IsTrue(revokeResponse.IsSuccessStatusCode, await revokeResponse.Content.ReadAsStringAsync());

        await Client.GetAsync(Url);
    }

    /// <summary>
    /// Gets the backoffice authentication cookie set by a response, in the form expected by a <c>Cookie</c> request header.
    /// </summary>
    /// <param name="response">The response that signed the user in, and so set the cookie.</param>
    /// <returns>The cookie as <c>name=value</c>, without attributes such as <c>path</c> or <c>expires</c>.</returns>
    /// <remarks>
    /// The cookie name is read from <see cref="SecuritySettings.AuthCookieName"/>. The test fails if the response does not set it.
    /// </remarks>
    private string GetAuthCookie(HttpResponseMessage response)
    {
        var authCookieName = GetRequiredService<IOptions<SecuritySettings>>().Value.AuthCookieName;
        var setCookieHeader = response.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(header => header.StartsWith($"{authCookieName}=", StringComparison.Ordinal));
        Assert.IsNotNull(setCookieHeader, $"The response did not set the {authCookieName} cookie.");
        return setCookieHeader!.Split(';')[0];
    }

    /// <summary>
    /// Determines whether the backoffice accepts a cookie as proof of a signed-in user, by starting an OAuth
    /// authorization code flow with it.
    /// </summary>
    /// <param name="client">A client that neither stores cookies nor follows redirects.</param>
    /// <param name="authCookie">The backoffice cookie, as <c>name=value</c>.</param>
    /// <returns>
    /// <c>true</c> if the authorize endpoint issued an authorization code; <c>false</c> if it sent the client to the login screen.
    /// </returns>
    /// <remarks>
    /// The authorize endpoint authenticates by the backoffice cookie alone, so it shows whether the cookie is still valid.
    /// Either way it responds with a redirect: to the backoffice callback with a <c>code</c> in the query string when the
    /// cookie is accepted, or to the login screen when it is not. Anything other than a redirect fails the test.
    /// The PKCE code challenge is derived from a fixed dummy verifier. The code is never exchanged for tokens.
    /// </remarks>
    private async Task<bool> AuthCookieIsAcceptedAsync(HttpClient client, string authCookie)
    {
        OpenIddictApplicationDescriptor descriptor;
        using (IServiceScope serviceScope = GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var backOfficeApplicationManager = (BackOfficeApplicationManager)serviceScope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>();
            descriptor = backOfficeApplicationManager.BackofficeOpenIddictApplicationDescriptor(client.BaseAddress!);
        }

        var codeChallenge = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("12345"))).TrimEnd('=');
        var authorizeUrl = GetManagementApiUrl<BackOfficeController>(x => x.Authorize(CancellationToken.None))
                           + $"?client_id={descriptor.ClientId}&response_type=code"
                           + $"&redirect_uri={WebUtility.UrlEncode(descriptor.RedirectUris.First().AbsoluteUri)}"
                           + $"&code_challenge_method=S256&code_challenge={codeChallenge}";

        using var authorizeRequest = new HttpRequestMessage(HttpMethod.Get, authorizeUrl);
        authorizeRequest.Headers.Add("Cookie", authCookie);
        HttpResponseMessage authorizeResponse = await client.SendAsync(authorizeRequest);

        Assert.AreEqual(HttpStatusCode.Found, authorizeResponse.StatusCode, await authorizeResponse.Content.ReadAsStringAsync());
        var location = new Uri(client.BaseAddress!, authorizeResponse.Headers.Location!);
        return HttpUtility.ParseQueryString(location.Query).Get("code") is not null;
    }

    private IUser GetUser(string email)
    {
        IUser? user = UserService.GetByEmail(email);
        Assert.IsNotNull(user);
        return user!;
    }

    private List<UserLoginDto> GetLoginSessions(int userId)
    {
        using IScope scope = GetRequiredService<IScopeProvider>().CreateScope(autoComplete: true);
        var sql = scope.SqlContext.Sql()
            .Select<UserLoginDto>()
            .From<UserLoginDto>()
            .Where<UserLoginDto>(x => x.UserId == userId);
        return scope.Database.Fetch<UserLoginDto>(sql);
    }
}
