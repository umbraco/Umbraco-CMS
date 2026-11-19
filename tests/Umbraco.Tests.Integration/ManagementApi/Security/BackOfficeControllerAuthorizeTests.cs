using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

public class BackOfficeControllerAuthorizeTests : ManagementApiTest<BackOfficeController>
{
    private const string CodeVerifier = "12345";

    protected override Expression<Func<BackOfficeController, object>> MethodSelector { get; set; } =
        x => x.Authorize(CancellationToken.None);

    private IUserService UserService => GetRequiredService<IUserService>();

    [Test]
    public async Task Authorize_Issues_Code_For_Approved_User()
    {
        await LoginAsNewUserAsync("approved@umbraco.com");

        HttpResponseMessage response = await Client.GetAsync(AuthorizeUrl());

        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
        Assert.IsNotNull(AuthorizationCode(response));
    }

    [Test]
    public async Task Authorize_Does_Not_Issue_Code_For_Disabled_User()
    {
        IUser user = await LoginAsNewUserAsync("disabled@umbraco.com");

        UserOperationStatus status = await UserService.DisableAsync(Constants.Security.SuperUserKey, new HashSet<Guid> { user.Key });
        Assert.AreEqual(UserOperationStatus.Success, status);

        HttpResponseMessage response = await Client.GetAsync(AuthorizeUrl());

        Assert.IsNull(AuthorizationCode(response), "A disabled user was issued an authorization code.");
        AssertRedirectedToLogin(response);
        AssertBackOfficeCookieCleared(response);
    }

    [Test]
    public async Task Authorize_Does_Not_Issue_Code_For_Locked_Out_User()
    {
        IUser user = await LoginAsNewUserAsync("lockedout@umbraco.com");

        user.IsLockedOut = true;
        UserService.Save(user);

        HttpResponseMessage response = await Client.GetAsync(AuthorizeUrl());

        Assert.IsNull(AuthorizationCode(response), "A locked out user was issued an authorization code.");
        AssertRedirectedToLogin(response);
        AssertBackOfficeCookieCleared(response);
    }

    [Test]
    public async Task Authorize_Issues_Code_For_User_With_Expired_Lockout()
    {
        IUser user = await LoginAsNewUserAsync("expiredlockout@umbraco.com");

        var lockoutMinutes = GetRequiredService<IOptions<SecuritySettings>>().Value.UserDefaultLockoutTimeInMinutes;
        user.IsLockedOut = true;
        user.LastLockoutDate = DateTime.UtcNow.AddMinutes(-lockoutMinutes).AddDays(-1);
        UserService.Save(user);

        HttpResponseMessage response = await Client.GetAsync(AuthorizeUrl());

        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
        Assert.IsNotNull(AuthorizationCode(response));
    }

    private async Task<IUser> LoginAsNewUserAsync(string email)
    {
        IUser? createdUser = null;
        await AuthenticateClientAsync(
            Client,
            async userService =>
            {
                createdUser = (await userService.CreateAsync(
                    Constants.Security.SuperUserKey,
                    new UserCreateModel
                    {
                        Email = email,
                        Name = email,
                        UserName = email,
                        UserGroupKeys = new HashSet<Guid> { Constants.Security.EditorGroupKey },
                    },
                    true)).Result.CreatedUser;

                return (createdUser!, UserPassword);
            });

        return UserService.GetByEmail(email)!;
    }

    private string AuthorizeUrl()
    {
        using IServiceScope scope = GetRequiredService<IServiceScopeFactory>().CreateScope();
        var backOfficeApplicationManager = (BackOfficeApplicationManager)scope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>();
        var descriptor = backOfficeApplicationManager.BackofficeOpenIddictApplicationDescriptor(Client.BaseAddress!);
        var codeChallenge = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(CodeVerifier))).TrimEnd("=");

        return Url
               + $"?client_id={descriptor.ClientId}"
               + "&response_type=code"
               + $"&redirect_uri={WebUtility.UrlEncode(descriptor.RedirectUris.First().AbsoluteUri)}"
               + "&code_challenge_method=S256"
               + $"&code_challenge={codeChallenge}";
    }

    private static string? AuthorizationCode(HttpResponseMessage response)
        => response.Headers.Location is null
            ? null
            : HttpUtility.ParseQueryString(response.Headers.Location.Query).Get("code");

    private static void AssertRedirectedToLogin(HttpResponseMessage response)
    {
        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
        StringAssert.Contains("/umbraco/login", response.Headers.Location?.OriginalString);
    }

    private void AssertBackOfficeCookieCleared(HttpResponseMessage response)
    {
        var cookieName = GetRequiredService<IOptions<SecuritySettings>>().Value.AuthCookieName;
        Assert.IsTrue(response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies), "No cookies were set.");
        Assert.IsTrue(
            cookies!.Any(c => c.StartsWith($"{cookieName}=;") && c.Contains("expires=Thu, 01 Jan 1970", StringComparison.OrdinalIgnoreCase)),
            "The back-office cookie was not cleared.");
    }
}
