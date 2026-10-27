using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Umbraco.Cms.Api.Common.Security;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Security;

[TestFixture]
public class BackOfficeRedirectUriValidationHandlerTests
{
    private const string RequestHost = "https://site.test";

    [TestCase(Constants.OAuthClientIds.BackOffice)]
    [TestCase(Constants.OAuthClientIds.Swagger)]
    public async Task Authorization_Request_Allows_Redirect_Uri_On_Request_Host(string clientId)
    {
        OpenIddictServerEvents.ValidateAuthorizationRequestContext context = CreateAuthorizationContext(clientId, $"{RequestHost}/umbraco/oauth_complete");

        await CreateHandler().HandleAsync(context);

        Assert.IsFalse(context.IsRejected);
    }

    [TestCase("https://SITE.test/umbraco/oauth_complete")]
    [TestCase("https://site.test:443/umbraco/oauth_complete")]
    public async Task Authorization_Request_Compares_Authority_Of_Redirect_Uri_Normalized(string redirectUri)
    {
        OpenIddictServerEvents.ValidateAuthorizationRequestContext context = CreateAuthorizationContext(Constants.OAuthClientIds.BackOffice, redirectUri);

        await CreateHandler().HandleAsync(context);

        Assert.IsFalse(context.IsRejected);
    }

    [TestCase(Constants.OAuthClientIds.BackOffice, "https://other.test/umbraco/oauth_complete")]
    [TestCase(Constants.OAuthClientIds.BackOffice, "http://site.test/umbraco/oauth_complete")]
    [TestCase(Constants.OAuthClientIds.BackOffice, "https://site.test:8443/umbraco/oauth_complete")]
    [TestCase(Constants.OAuthClientIds.Swagger, "https://other.test/umbraco/swagger/oauth2-redirect.html")]
    public async Task Authorization_Request_Rejects_Redirect_Uri_On_Another_Authority(string clientId, string redirectUri)
    {
        OpenIddictServerEvents.ValidateAuthorizationRequestContext context = CreateAuthorizationContext(clientId, redirectUri);

        await CreateHandler().HandleAsync(context);

        Assert.IsTrue(context.IsRejected);
        Assert.AreEqual(OpenIddictConstants.Errors.InvalidRequest, context.Error);
    }

    [Test]
    public async Task Authorization_Request_Allows_Redirect_Uri_On_Configured_BackOffice_Host()
    {
        OpenIddictServerEvents.ValidateAuthorizationRequestContext context = CreateAuthorizationContext(Constants.OAuthClientIds.BackOffice, "https://localhost:5173/umbraco/oauth_complete");

        await CreateHandler(backOfficeHost: new Uri("https://localhost:5173")).HandleAsync(context);

        Assert.IsFalse(context.IsRejected);
    }

    [Test]
    public async Task Authorization_Request_Allows_Redirect_Uri_On_Configured_Application_Url()
    {
        OpenIddictServerEvents.ValidateAuthorizationRequestContext context = CreateAuthorizationContext(Constants.OAuthClientIds.BackOffice, "https://public.test/umbraco/oauth_complete");

        await CreateHandler(applicationUrl: "https://public.test/").HandleAsync(context);

        Assert.IsFalse(context.IsRejected);
    }

    [TestCase("other-client")]
    [TestCase(Constants.OAuthClientIds.Member)]
    [TestCase(Constants.OAuthClientIds.Postman)]
    public async Task Authorization_Request_Ignores_Clients_With_Independently_Registered_Redirect_Uris(string clientId)
    {
        OpenIddictServerEvents.ValidateAuthorizationRequestContext context = CreateAuthorizationContext(clientId, "https://other.test/callback", OpenIddictConstants.ResponseModes.FormPost);

        await CreateHandler().HandleAsync(context);

        Assert.IsFalse(context.IsRejected);
    }

    [TestCase(null)]
    [TestCase(OpenIddictConstants.ResponseModes.Query)]
    public async Task Authorization_Request_Allows_Query_Response_Mode_For_BackOffice_Client(string? responseMode)
    {
        OpenIddictServerEvents.ValidateAuthorizationRequestContext context = CreateAuthorizationContext(Constants.OAuthClientIds.BackOffice, $"{RequestHost}/umbraco/oauth_complete", responseMode);

        await CreateHandler().HandleAsync(context);

        Assert.IsFalse(context.IsRejected);
    }

    [TestCase(OpenIddictConstants.ResponseModes.FormPost)]
    [TestCase(OpenIddictConstants.ResponseModes.Fragment)]
    public async Task Authorization_Request_Rejects_Other_Response_Modes_For_BackOffice_Client(string responseMode)
    {
        OpenIddictServerEvents.ValidateAuthorizationRequestContext context = CreateAuthorizationContext(Constants.OAuthClientIds.BackOffice, $"{RequestHost}/umbraco/oauth_complete", responseMode);

        await CreateHandler().HandleAsync(context);

        Assert.IsTrue(context.IsRejected);
    }

    [Test]
    public async Task End_Session_Request_Rejects_Post_Logout_Redirect_Uri_On_Another_Authority()
    {
        OpenIddictServerEvents.ValidateEndSessionRequestContext context = CreateEndSessionContext("https://other.test/umbraco/logout");

        await CreateHandler(requestPath: Paths.BackOfficeApi.LogoutEndpoint).HandleAsync(context);

        Assert.IsTrue(context.IsRejected);
    }

    [Test]
    public async Task End_Session_Request_Allows_Post_Logout_Redirect_Uri_On_Request_Host()
    {
        OpenIddictServerEvents.ValidateEndSessionRequestContext context = CreateEndSessionContext($"{RequestHost}/umbraco/logout");

        await CreateHandler(requestPath: Paths.BackOfficeApi.LogoutEndpoint).HandleAsync(context);

        Assert.IsFalse(context.IsRejected);
    }

    [Test]
    public async Task End_Session_Request_Ignores_Requests_Outside_The_BackOffice_Logout_Endpoint()
    {
        OpenIddictServerEvents.ValidateEndSessionRequestContext context = CreateEndSessionContext("https://other.test/logged-out");

        await CreateHandler(requestPath: Paths.MemberApi.LogoutEndpoint).HandleAsync(context);

        Assert.IsFalse(context.IsRejected);
    }

    private static BackOfficeRedirectUriValidationHandler CreateHandler(
        Uri? backOfficeHost = null,
        string? applicationUrl = null,
        string requestPath = "/umbraco/management/api/v1/security/back-office/authorize")
    {
        var requestHost = new Uri(RequestHost);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = requestHost.Scheme;
        httpContext.Request.Host = new HostString(requestHost.Authority);
        httpContext.Request.Path = requestPath;

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var securitySettings = new Mock<IOptionsMonitor<SecuritySettings>>();
        securitySettings.Setup(x => x.CurrentValue).Returns(new SecuritySettings { BackOfficeHost = backOfficeHost });

        var webRoutingSettings = new Mock<IOptionsMonitor<WebRoutingSettings>>();
        webRoutingSettings.Setup(x => x.CurrentValue).Returns(new WebRoutingSettings { UmbracoApplicationUrl = applicationUrl! });

        return new BackOfficeRedirectUriValidationHandler(httpContextAccessor.Object, securitySettings.Object, webRoutingSettings.Object);
    }

    private static OpenIddictServerEvents.ValidateAuthorizationRequestContext CreateAuthorizationContext(string clientId, string redirectUri, string? responseMode = null)
        => new(new OpenIddictServerTransaction())
        {
            Request = new OpenIddictRequest
            {
                ClientId = clientId,
                RedirectUri = redirectUri,
                ResponseMode = responseMode,
                ResponseType = OpenIddictConstants.ResponseTypes.Code,
            },
        };

    private static OpenIddictServerEvents.ValidateEndSessionRequestContext CreateEndSessionContext(string postLogoutRedirectUri)
        => new(new OpenIddictServerTransaction())
        {
            Request = new OpenIddictRequest { PostLogoutRedirectUri = postLogoutRedirectUri },
        };
}
