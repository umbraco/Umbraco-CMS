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
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User.Current;

/// <summary>
/// Verifies that a user who changes their own password stays signed in on the device they made the change from,
/// while any other copy of the session ends.
/// </summary>
/// <remarks>
/// The password change renews the security stamp, which is checked against the stamp of the cookie either on every
/// request or once per validation interval, depending on whether concurrent logins are allowed. The test therefore
/// advances a controllable clock past the validation interval before it uses the cookies again.
/// </remarks>
[TestFixture(true)]
[TestFixture(false)]
public class ChangePasswordCurrentUserSessionTests(bool allowConcurrentLogins)
    : ManagementApiTest<ChangePasswordCurrentUserController>
{
    private const string NewPassword = "0987654321";

    private static int _loginNotificationCount;

    private readonly MutableTimeProvider _clock = new(DateTimeOffset.UtcNow);

    protected override Expression<Func<ChangePasswordCurrentUserController, object>> MethodSelector { get; set; } =
        x => x.ChangePassword(CancellationToken.None, null);

    private TimeSpan ValidationInterval =>
        GetRequiredService<IOptions<BackOfficeSecurityStampValidatorOptions>>().Value.ValidationInterval;

    private string CurrentUserUrl =>
        GetManagementApiUrl<GetCurrentUserController>(x => x.GetCurrentUser(CancellationToken.None));

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.Services.Configure<SecuritySettings>(x => x.UserAllowConcurrentLogins = allowConcurrentLogins);
        builder.AddNotificationHandler<UserLoginSuccessNotification, LoginNotificationCounter>();
        builder.Services.RemoveAll<TimeProvider>();
        builder.Services.AddSingleton<TimeProvider>(_clock);
        builder.Services.Configure<CookieAuthenticationOptions>(
            Constants.Security.BackOfficeAuthenticationType,
            options => options.TimeProvider = _clock);
    }

    [Test]
    public async Task Can_Stay_Signed_In_After_Changing_Own_Password_While_Other_Sessions_End()
    {
        const string email = "change-password-session@umbraco.com";
        using HttpClient client = await CreateCookielessClientAsync(email);

        // Without concurrent logins the second login already ends the first session, which stays ended.
        var cookieOfOtherDevice = await LoginAsync(client, email);
        var cookie = await LoginAsync(client, email);

        var loginNotificationsBeforeChange = Volatile.Read(ref _loginNotificationCount);

        (HttpStatusCode changeStatusCode, string renewedCookie) = await SendAsync(
            client,
            HttpMethod.Post,
            Url,
            cookie,
            JsonContent.Create(new ChangePasswordCurrentUserRequestModel { OldPassword = UserPassword, NewPassword = NewPassword }));
        Assert.AreEqual(HttpStatusCode.OK, changeStatusCode);
        Assert.AreNotEqual(cookie, renewedCookie, "The password change did not re-issue the cookie.");
        Assert.AreEqual(
            loginNotificationsBeforeChange,
            Volatile.Read(ref _loginNotificationCount),
            "Re-issuing the cookie was handled as a login.");

        _clock.Advance(ValidationInterval + TimeSpan.FromSeconds(1));

        (HttpStatusCode ownStatusCode, _) = await SendAsync(client, HttpMethod.Get, CurrentUserUrl, renewedCookie);
        Assert.AreEqual(HttpStatusCode.OK, ownStatusCode, "The user was signed out on the device changing the password.");

        (HttpStatusCode otherStatusCode, _) = await SendAsync(client, HttpMethod.Get, CurrentUserUrl, cookieOfOtherDevice);
        Assert.AreEqual(HttpStatusCode.Unauthorized, otherStatusCode, "Another session of the user survived the password change.");
    }

    private async Task<HttpClient> CreateCookielessClientAsync(string email)
    {
        await AuthenticateClientAsync(Client, email, UserPassword, Constants.Security.EditorGroupKey);

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
    private async Task<(HttpStatusCode StatusCode, string Cookie)> SendAsync(
        HttpClient client,
        HttpMethod method,
        string url,
        string cookie,
        HttpContent? content = null)
    {
        using var request = new HttpRequestMessage(method, url) { Content = content };
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

    private sealed class LoginNotificationCounter : INotificationHandler<UserLoginSuccessNotification>
    {
        public void Handle(UserLoginSuccessNotification notification) => Interlocked.Increment(ref _loginNotificationCount);
    }

    private sealed class MutableTimeProvider(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset _now = start;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
