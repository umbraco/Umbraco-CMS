using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenIddict.Abstractions;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

/// <summary>
/// Verifies that back-office authorization codes and tokens can only be delivered to, and redeemed by, the site itself.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console, Boot = true)]
public class BackOfficeAuthorizationFlowTests : UmbracoTestServerTestBase
{
    private const string UserEmail = "authflow@umbraco.com";
    private const string UserPassword = "1234567890";
    private const string CodeVerifier = "a-code-verifier-that-is-long-enough-for-the-pkce-requirements";
    private const string ForeignHost = "https://foreign.test/";

    private const string AuthorizeUrl = "/umbraco/management/api/v1/security/back-office/authorize";

    protected override void CustomTestAuthSetup(IServiceCollection services)
    {
        // We do not want fake authentication for these tests
    }

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration["Umbraco:CMS:ModelsBuilder:ModelsMode"] = "Nothing";
        base.Setup();
    }

    [Test]
    public async Task Authorize_Issues_Code_To_Redirect_Uri_On_Request_Host()
    {
        await LoginAsync();

        HttpResponseMessage response = await Client.GetAsync(BuildAuthorizeUrl(Constants.OAuthClientIds.BackOffice, BackOfficeRedirectUri(Client.BaseAddress!)));

        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode, await response.Content.ReadAsStringAsync());
        Assert.AreEqual(Client.BaseAddress!.Authority, response.Headers.Location!.Authority);
    }

    [TestCase(Constants.OAuthClientIds.BackOffice, SecuritySettings.StaticCallbackPathName)]
    [TestCase(Constants.OAuthClientIds.OpenApiUi, "/umbraco/openapi/oauth2-redirect.html")]
    public async Task Authorize_Rejects_Registered_Redirect_Uri_On_Another_Host(string clientId, string callbackPath)
    {
        await RegisterBackOfficeHostsAsync(new Uri(ForeignHost));
        await LoginAsync();

        var foreignRedirectUri = new Uri(new Uri(ForeignHost), callbackPath);
        await AssertRedirectUriIsRegisteredAsync(clientId, foreignRedirectUri);

        HttpResponseMessage response = await Client.GetAsync(BuildAuthorizeUrl(clientId, foreignRedirectUri));

        Assert.AreNotEqual(HttpStatusCode.Found, response.StatusCode, $"Redirected to {response.Headers.Location}");
        Assert.IsNull(response.Headers.Location);
    }

    [TestCase(OpenIddictConstants.ResponseModes.FormPost)]
    [TestCase(OpenIddictConstants.ResponseModes.Fragment)]
    public async Task Authorize_Rejects_Response_Modes_Other_Than_Query_For_BackOffice_Client(string responseMode)
    {
        await LoginAsync();

        HttpResponseMessage response = await Client.GetAsync(
            BuildAuthorizeUrl(Constants.OAuthClientIds.BackOffice, BackOfficeRedirectUri(Client.BaseAddress!)) + $"&response_mode={responseMode}");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private static Uri BackOfficeRedirectUri(Uri host) => new(host, SecuritySettings.StaticCallbackPathName);

    private static string BuildAuthorizeUrl(string clientId, Uri redirectUri)
    {
        var codeChallenge = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(CodeVerifier)))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        return AuthorizeUrl
               + $"?client_id={clientId}"
               + "&response_type=code"
               + $"&redirect_uri={WebUtility.UrlEncode(redirectUri.AbsoluteUri)}"
               + "&code_challenge_method=S256"
               + $"&code_challenge={codeChallenge}";
    }

    private async Task RegisterBackOfficeHostsAsync(params Uri[] hosts)
    {
        using IServiceScope scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>()
            .EnsureBackOfficeApplicationAsync([Client.BaseAddress!, .. hosts]);
    }

    private async Task AssertRedirectUriIsRegisteredAsync(string clientId, Uri redirectUri)
    {
        using IServiceScope scope = Services.CreateScope();
        IOpenIddictApplicationManager applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var application = await applicationManager.FindByClientIdAsync(clientId);
        Assert.IsNotNull(application, $"The {clientId} application should be registered.");
        Assert.That(await applicationManager.GetRedirectUrisAsync(application!), Does.Contain(redirectUri.AbsoluteUri));
    }


    private async Task LoginAsync()
    {
        IUserService userService = GetRequiredService<IUserService>();
        IUser user = await userService.GetRequiredUserAsync(Constants.Security.SuperUserKey);
        user.Username = user.Email = UserEmail;
        userService.Save(user);

        using (IServiceScope scope = Services.CreateScope())
        {
            ICoreBackOfficeUserManager userManager = scope.ServiceProvider.GetRequiredService<ICoreBackOfficeUserManager>();
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var changePasswordAttempt = await userService.ChangePasswordAsync(
                user.Key,
                new ChangeUserPasswordModel
                {
                    NewPassword = UserPassword,
                    ResetPasswordToken = resetToken.Result.ToUrlBase64(),
                    UserKey = user.Key,
                });
            Assert.IsTrue(changePasswordAttempt.Success);
        }

        HttpResponseMessage loginResponse = await Client.PostAsync(
            GetManagementApiUrl<BackOfficeController>(x => x.Login(CancellationToken.None, null)),
            JsonContent.Create(new LoginRequestModel { Username = UserEmail, Password = UserPassword }));
        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode, await loginResponse.Content.ReadAsStringAsync());
    }
}
