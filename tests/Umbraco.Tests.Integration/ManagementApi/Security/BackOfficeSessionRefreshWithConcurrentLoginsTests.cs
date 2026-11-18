using System.Linq.Expressions;
using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.Controllers.User.Current;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

/// <summary>
/// Verifies that a backoffice session which is renewed on activity still has its security stamp re-validated once per
/// validation interval, so a copy of a cookie cannot be kept alive past a sign-out by continuously using it.
/// </summary>
/// <remarks>
/// With concurrent logins allowed the security stamp is validated once per validation interval rather than on every
/// request. The requests go through the real cookie middleware and security stamp validator, with a controllable clock.
/// The current-user endpoint is used because it relies on cookie validation alone, unlike the authorize endpoint
/// that has a security stamp check of its own.
/// </remarks>
public class BackOfficeSessionRefreshWithConcurrentLoginsTests : ManagementApiTest<BackOfficeController>
{
    private readonly MutableTimeProvider _clock = new(DateTimeOffset.UtcNow);

    protected override Expression<Func<BackOfficeController, object>> MethodSelector { get; set; } =
        x => x.Signout(CancellationToken.None);

    private TimeSpan ValidationInterval =>
        GetRequiredService<IOptions<BackOfficeSecurityStampValidatorOptions>>().Value.ValidationInterval;

    private string CurrentUserUrl =>
        GetManagementApiUrl<GetCurrentUserController>(x => x.GetCurrentUser(CancellationToken.None));

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.Services.Configure<SecuritySettings>(x => x.UserAllowConcurrentLogins = true);
        builder.Services.RemoveAll<TimeProvider>();
        builder.Services.AddSingleton<TimeProvider>(_clock);
        builder.Services.Configure<CookieAuthenticationOptions>(
            Constants.Security.BackOfficeAuthenticationType,
            options => options.TimeProvider = _clock);
    }

    [Test]
    public void Security_Stamp_Validation_Is_Deferred()
        => Assert.Greater(ValidationInterval, TimeSpan.Zero);

    [Test]
    public async Task Sign_Out_Ends_A_Copied_Session_That_Is_Used_More_Often_Than_The_Validation_Interval()
    {
        const string email = "session-refresh-signout@umbraco.com";
        using HttpClient client = await CreateCookielessClientAsync(email);
        var cookie = await LoginAsync(client, email);

        // The copy of the cookie is used more often than the stamp validation interval, as a script keeping the
        // session alive would do. Each response carries the renewed cookie, which the copy keeps using.
        TimeSpan activityInterval = ValidationInterval / 3;
        for (var i = 0; i < 3; i++)
        {
            _clock.Advance(activityInterval);
            (HttpStatusCode statusCode, cookie) = await GetAsync(client, CurrentUserUrl, cookie);
            Assert.AreEqual(HttpStatusCode.OK, statusCode);
        }

        var copiedCookie = cookie;

        // The user signs out in their own browser, which renews the user's security stamp.
        _clock.Advance(activityInterval);
        await GetAsync(client, Url, cookie);

        // The copy is rejected no later than the first request after the next stamp validation is due.
        var rejected = false;
        for (var i = 0; i < 5 && rejected is false; i++)
        {
            _clock.Advance(activityInterval);
            (HttpStatusCode statusCode, copiedCookie) = await GetAsync(client, CurrentUserUrl, copiedCookie);
            rejected = statusCode == HttpStatusCode.Unauthorized;
        }

        Assert.IsTrue(rejected, "The copy of the cookie is still accepted after sign-out.");
    }

    [Test]
    public async Task Session_Is_Extended_By_Activity_And_Ends_When_The_Timeout_Passes_Without_Activity()
    {
        const string email = "session-refresh-timeout@umbraco.com";
        using HttpClient client = await CreateCookielessClientAsync(email);
        var cookie = await LoginAsync(client, email);
        TimeSpan timeout = GetRequiredService<IOptions<GlobalSettings>>().Value.TimeOut;

        // Keep active for longer than the timeout, with requests closer together than the validation interval.
        TimeSpan activityInterval = ValidationInterval * 0.8;
        for (TimeSpan elapsed = TimeSpan.Zero; elapsed < timeout * 1.5; elapsed += activityInterval)
        {
            _clock.Advance(activityInterval);
            (HttpStatusCode statusCode, cookie) = await GetAsync(client, CurrentUserUrl, cookie);
            Assert.AreEqual(HttpStatusCode.OK, statusCode, $"The session ended after {elapsed} of activity.");
        }

        // The session expires one timeout after the last request, no later.
        _clock.Advance(timeout + TimeSpan.FromSeconds(1));
        (HttpStatusCode idleStatusCode, _) = await GetAsync(client, CurrentUserUrl, cookie);
        Assert.AreEqual(HttpStatusCode.Unauthorized, idleStatusCode);
    }

    private async Task<HttpClient> CreateCookielessClientAsync(string email)
    {
        // Creates the user and sets its password. The client authenticated here is not used further.
        await AuthenticateClientAsync(Client, email, UserPassword, Constants.Security.EditorGroupKey);

        // This client keeps no cookies, so every request carries exactly the cookie the test attaches.
        return Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false,
            BaseAddress = Client.BaseAddress,
        });
    }

    private async Task<string> LoginAsync(HttpClient client, string email)
    {
        HttpResponseMessage loginResponse = await client.PostAsync(
            GetManagementApiUrl<BackOfficeController>(x => x.Login(CancellationToken.None, null)),
            JsonContent.Create(new LoginRequestModel { Username = email, Password = UserPassword }));
        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode, await loginResponse.Content.ReadAsStringAsync());
        return GetRenewedAuthCookie(loginResponse) ?? throw new AssertionException("The login did not set the auth cookie.");
    }

    // Sends the request with the given cookie, and returns the cookie to use next: the renewed one if the response sets it.
    private async Task<(HttpStatusCode StatusCode, string Cookie)> GetAsync(HttpClient client, string url, string cookie)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Cookie", cookie);
        using HttpResponseMessage response = await client.SendAsync(request);
        return (response.StatusCode, GetRenewedAuthCookie(response) ?? cookie);
    }

    // A response that deletes the cookie sets it to an empty value, which is not a renewal.
    private string? GetRenewedAuthCookie(HttpResponseMessage response)
    {
        var name = GetRequiredService<IOptions<SecuritySettings>>().Value.AuthCookieName;
        if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? headers) is false)
        {
            return null;
        }

        return headers
            .Select(header => header.Split(';')[0])
            .FirstOrDefault(cookie => cookie.StartsWith($"{name}=", StringComparison.Ordinal) && cookie.Length > name.Length + 1);
    }

    private sealed class MutableTimeProvider(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset _now = start;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
