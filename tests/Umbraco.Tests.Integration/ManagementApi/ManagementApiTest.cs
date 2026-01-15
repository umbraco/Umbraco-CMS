using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using OpenIddict.Abstractions;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
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
    private static readonly Dictionary<string, TokenModel> _tokenCache = new();
    private static readonly SHA256 _sha256 = SHA256.Create();

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
        InMemoryConfiguration["Umbraco:CMS:ModelsBuilder:ModelsMode"] = "Nothing";

        base.Setup();
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
    }

    [SetUp]
    public override void SetUp_Logging() =>
        TestContext.Out.Write($"Start test {TestCount++}: {TestContext.CurrentContext.Test.FullName}");

    [OneTimeTearDown]
    public void ClearCache() => _tokenCache.Clear();

    protected override void CustomTestAuthSetup(IServiceCollection services)
    {
        // We do not wanna fake anything, and thereby have protection
    }

    protected async Task AuthenticateClientAsync(HttpClient client, string username, string password, bool isAdmin) =>
        await AuthenticateClientAsync(client,
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
        if (!string.IsNullOrEmpty(cacheKey) && _tokenCache.TryGetValue(cacheKey, out var cachedToken))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", cachedToken.AccessToken);
            return;
        }

        OpenIddictApplicationDescriptor backofficeOpenIddictApplicationDescriptor;
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

            var changePasswordAttempt = await userService.ChangePasswordAsync(userKey,
                new ChangeUserPasswordModel
                {
                    NewPassword = password, ResetPasswordToken = token.Result.ToUrlBase64(), UserKey = userKey,
                });

            Assert.IsTrue(changePasswordAttempt.Success);

            var backOfficeApplicationManager =
                serviceScope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>() as
                    BackOfficeApplicationManager;
            backofficeOpenIddictApplicationDescriptor =
                backOfficeApplicationManager.BackofficeOpenIddictApplicationDescriptor(client.BaseAddress);
            scope.Complete();
        }

        var loginModel = new LoginRequestModel { Username = username, Password = password };

        // Login to ensure the cookie is set (used in next request)
        var loginResponse = await client.PostAsync(
            GetManagementApiUrl<BackOfficeController>(x => x.Login(CancellationToken.None, null)), JsonContent.Create(loginModel));

        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode, await loginResponse.Content.ReadAsStringAsync());

        const string codeVerifier = "12345"; // Just a dummy value we use in tests
        var codeChallenge = Convert.ToBase64String(_sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier)))
            .TrimEnd("=");

        var authorizationUrl = GetManagementApiUrl<BackOfficeController>(x => x.Authorize(CancellationToken.None)) + $"?client_id={backofficeOpenIddictApplicationDescriptor.ClientId}&response_type=code&redirect_uri={WebUtility.UrlEncode(backofficeOpenIddictApplicationDescriptor.RedirectUris.FirstOrDefault()?.AbsoluteUri)}&code_challenge_method=S256&code_challenge={codeChallenge}";
        var authorizeResponse = await client.GetAsync(authorizationUrl);

        Assert.AreEqual(HttpStatusCode.Found, authorizeResponse.StatusCode, await authorizeResponse.Content.ReadAsStringAsync());

        var tokenResponse = await client.PostAsync("/umbraco/management/api/v1/security/back-office/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code_verifier"] = codeVerifier,
                ["client_id"] = backofficeOpenIddictApplicationDescriptor.ClientId,
                ["code"] = HttpUtility.ParseQueryString(authorizeResponse.Headers.Location.Query).Get("code"),
                ["redirect_uri"] =
                    backofficeOpenIddictApplicationDescriptor.RedirectUris.FirstOrDefault().AbsoluteUri,
            }));

        var tokenModel = await tokenResponse.Content.ReadFromJsonAsync<TokenModel>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.AccessToken);

        // Cache the token if cache key provided
        if (!string.IsNullOrEmpty(cacheKey))
        {
            _tokenCache[cacheKey] = tokenModel;
        }
    }

    private class TokenModel
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }
}
