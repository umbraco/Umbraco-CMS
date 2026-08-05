using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.ManagementApi;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture, Logger = UmbracoTestOptions.Logger.Console, Boot = true)]
public abstract class ManagementApiTest<T> : UmbracoTestServerTestBase
    where T : ManagementApiControllerBase
{
    private static readonly Dictionary<string, string> _authCookieCache = new();

    protected JsonSerializerOptions JsonSerializerOptions
    {
        get
        {
            var options = GetRequiredService<IOptionsMonitor<JsonOptions>>();
            return options
                .Get(Constants.JsonOptionsNames.BackOffice)
                .JsonSerializerOptions;
        }
    }

    protected abstract Expression<Func<T, object>> MethodSelector { get; set; }

    protected string Url => GetManagementApiUrl(MethodSelector);

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
    }

    [SetUp]
    public override void SetUp_Logging() =>
        TestContext.Out.Write($"Start test {GetNextTestCount()}: {TestContext.CurrentContext.Test.FullName}");

    [OneTimeTearDown]
    public void ClearCache() => _authCookieCache.Clear();

    protected override void CustomTestAuthSetup(IServiceCollection services)
    {
        // We do not wanna fake anything, and thereby have protection
    }

    protected async Task AuthenticateClientAsync(HttpClient client, string username, string password, bool isAdmin) =>
        await AuthenticateClientAsync(
            client,
            async userService =>
            {
                IUser user;
                if (isAdmin)
                {
                    user = await userService.GetRequiredUserAsync(Constants.Security.SuperUserKey);
                    user.Username = user.Email = username;
                    userService.Save(user);
                }
                else
                {
                    user = (await userService.CreateAsync(
                        Constants.Security.SuperUserKey,
                        new UserCreateModel
                        {
                            Email = username,
                            Name = username,
                            UserName = username,
                            UserGroupKeys = new HashSet<Guid>(new[] { Constants.Security.EditorGroupKey })
                        },
                        true)).Result.CreatedUser;
                }

                return (user, password);
            },
            $"{username}:{isAdmin}");

    protected async Task AuthenticateClientAsync(HttpClient client, string username, string password, Guid userGroupKey) =>
        await AuthenticateClientAsync(
            client,
            async userService =>
            {
                IUser user;
                if (userGroupKey == Constants.Security.AdminGroupKey)
                {
                    user = await userService.GetRequiredUserAsync(Constants.Security.SuperUserKey);
                    user.Username = user.Email = username;
                    userService.Save(user);
                }
                else
                {
                    user = (await userService.CreateAsync(
                        Constants.Security.SuperUserKey,
                        new UserCreateModel
                        {
                            Email = username,
                            Name = username,
                            UserName = username,
                            UserGroupKeys = new HashSet<Guid>([userGroupKey]),
                        },
                        true)).Result.CreatedUser;
                }

                return (user, password);
            },
            $"{username}:{userGroupKey}");

    protected async Task AuthenticateClientAsync(HttpClient client, Func<IUserService, Task<(IUser User, string Password)>> createUser, string cacheKey = null)
    {
        // Check cache first
        if (!string.IsNullOrEmpty(cacheKey) && _authCookieCache.TryGetValue(cacheKey, out var cachedCookie))
        {
            SetAuthCookie(client, cachedCookie);
            return;
        }

        var scopeProvider = GetRequiredService<ICoreScopeProvider>();

        string username;
        string password;

        using (var scope = scopeProvider.CreateCoreScope())
        {
            var userService = GetRequiredService<IUserService>();
            using var serviceScope = GetRequiredService<IServiceScopeFactory>().CreateScope();
            var userManager = serviceScope.ServiceProvider.GetRequiredService<ICoreBackOfficeUserManager>();

            var userCreationResult = await createUser(userService);
            username = userCreationResult.User.Username;
            password = userCreationResult.Password;
            var userKey = userCreationResult.User.Key;

            var token = await userManager.GeneratePasswordResetTokenAsync(userCreationResult.User);

            var changePasswordAttempt = await userService.ChangePasswordAsync(
                userKey,
                new ChangeUserPasswordModel
                {
                    NewPassword = password, ResetPasswordToken = token.Result.ToUrlBase64(), UserKey = userKey,
                });

            Assert.IsTrue(changePasswordAttempt.Success);

            scope.Complete();
        }

        var loginModel = new LoginRequestModel { Username = username, Password = password };

        // Login to ensure the cookie is set (used in next request)
        var loginResponse = await client.PostAsync(
            GetManagementApiUrl<BackOfficeController>(x => x.Login(CancellationToken.None, null)), JsonContent.Create(loginModel));

        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode, await loginResponse.Content.ReadAsStringAsync());

        // The login response sets the authentication cookie, and WebApplicationFactoryClientOptions
        // handles cookies by default, so this client is already authenticated. Nothing further is
        // needed: back office auth is cookie-only, with no token to exchange for.
        if (!string.IsNullOrEmpty(cacheKey))
        {
            _authCookieCache[cacheKey] = ReadAuthCookie(loginResponse);
        }
    }

    // The cookie is the whole session, so replaying it authenticates a fresh client without repeating
    // the user creation and password sign-in that produced it.
    private string ReadAuthCookie(HttpResponseMessage loginResponse)
    {
        var cookieName = GetRequiredService<IOptions<SecuritySettings>>().Value.AuthCookieName;

        var cookies = new CookieContainer();
        foreach (var cookieHeader in loginResponse.Headers.GetValues("Set-Cookie"))
        {
            cookies.SetCookies(loginResponse.RequestMessage!.RequestUri!, cookieHeader);
        }

        Cookie authCookie = cookies.GetCookies(loginResponse.RequestMessage!.RequestUri!)
            .FirstOrDefault(c => c.Name == cookieName)
            ?? throw new InvalidOperationException(
                $"The login response did not set the '{cookieName}' authentication cookie.");

        return authCookie.Value;
    }

    private void SetAuthCookie(HttpClient client, string authCookieValue)
    {
        var cookieName = GetRequiredService<IOptions<SecuritySettings>>().Value.AuthCookieName;

        if (client.DefaultRequestHeaders.Contains("Cookie"))
        {
            client.DefaultRequestHeaders.Remove("Cookie");
        }

        client.DefaultRequestHeaders.Add("Cookie", $"{cookieName}={authCookieValue}");
    }
}
