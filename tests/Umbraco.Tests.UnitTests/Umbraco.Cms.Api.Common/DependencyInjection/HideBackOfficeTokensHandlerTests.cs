using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OpenIddict.Server;
using OpenIddict.Validation;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.DependencyInjection;

[TestFixture]
internal class HideBackOfficeTokensHandlerTests
{
    // We have decided to keep these in sync manually => it will make the tests fail,
    // which notifies whoever changes their counterparts in HideBackOfficeTokensHandler to think about the implications
    private const string RedactedTokenValue = "[redacted]";
    private const string SecureCookiePrefix = "__Host-";
    private const string AccessTokenCookieName = "umbAccessToken";
    private const string RefreshTokenCookieName = "umbRefreshToken";
    private const string PkceCodeCookieName = "umbPkceCode";

    private const string AccessTokenValue = "my-access-token";
    private const string RefreshTokenValue = "my-refresh-token";
    private const string PkceCodeValue = "my-pkce-code";

    private class TestSetup
    {
    /// <summary>
    /// Gets the system under test (SUT) instance of <see cref="HideBackOfficeTokensHandler"/>.
    /// </summary>
        [field: AllowNull]
        [field: MaybeNull]
        public HideBackOfficeTokensHandler Sut => field ??= new HideBackOfficeTokensHandler(
            HttpContextAccessor.Object,
            DataProtectionProvider.Object,
            Options.Create(BackOfficeTokenCookieSettings),
            Options.Create(GlobalSettings));

    /// <summary>
    /// Gets the mock for IHttpContextAccessor used in the test setup.
    /// </summary>
        public Mock<IHttpContextAccessor> HttpContextAccessor { get; } = new();

        private Mock<IDataProtectionProvider> DataProtectionProvider { get; } = new();

    /// <summary>
    /// Gets the mock for the data protector used in the test setup.
    /// </summary>
        public Mock<IDataProtector> DataProtector { get; } = new();

#pragma warning disable CS0618 // Type or member is obsolete
        /// <summary>
        /// Gets or sets the settings for the back office token cookie.
        /// </summary>
        public BackOfficeTokenCookieSettings BackOfficeTokenCookieSettings { get; set; } = new();
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Gets or sets the global settings instance.
        /// </summary>
        public GlobalSettings GlobalSettings { get; set; } = new();

        private DefaultHttpContext HttpContext { get; set; } = new();

    /// <summary>
    /// Gets the collection of response cookies as key-value pairs.
    /// </summary>
        public Dictionary<string, string> ResponseCookies { get; } = new();

    /// <summary>
    /// Gets the set of deleted cookies.
    /// </summary>
        public HashSet<string> DeletedCookies { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TestSetup"/> class.
    /// </summary>
        public TestSetup()
        {
            // Setup DataProtector to append a single character on protect and remove the character on unprotect
            DataProtector.Setup(p => p.Protect(It.IsAny<byte[]>()))
                .Returns<byte[]>(data => data.Concat(new byte[] { 0xEE }).ToArray());
            DataProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>()))
                .Returns<byte[]>(data => data.Take(data.Length - 1).ToArray());

            DataProtectionProvider.Setup(p => p.CreateProtector(It.IsAny<string>()))
                .Returns(DataProtector.Object);
        }

    /// <summary>
    /// Sets up the <see cref="HttpContext"/> for testing, configuring HTTPS, request cookies, and response cookies as needed.
    /// Also sets up the <see cref="IHttpContextAccessor"/> to return the configured context.
    /// </summary>
    /// <param name="isHttps">If <c>true</c>, the request is treated as HTTPS; otherwise, HTTP.</param>
    /// <param name="requestCookies">An optional dictionary of cookies to include in the request.</param>
        public void SetupHttpContext(bool isHttps = false, Dictionary<string, string>? requestCookies = null)
        {
            HttpContext = new DefaultHttpContext();
            HttpContext.Request.IsHttps = isHttps;

            if (requestCookies != null)
            {
                HttpContext.Request.Headers["Cookie"] = string.Join("; ", requestCookies.Select(cookieKeyValue => $"{cookieKeyValue.Key}={cookieKeyValue.Value}"));
                var cookieCollection = new MockRequestCookieCollection(requestCookies);
                HttpContext.Features.Set<IRequestCookiesFeature>(new MockRequestCookiesFeature(cookieCollection));
            }

            // Track response cookies
            var responseCookies = new MockResponseCookies(ResponseCookies, DeletedCookies);
            HttpContext.Features.Set<IResponseCookiesFeature>(new MockResponseCookiesFeature(responseCookies));

            HttpContextAccessor.Setup(a => a.HttpContext).Returns(HttpContext);
        }

    /// <summary>
    /// Encrypts the specified string value by encoding it to UTF8 bytes, appending a marker byte, and converting to a Base64 string.
    /// </summary>
    /// <param name="value">The string value to encrypt.</param>
    /// <returns>The encrypted string as a Base64 encoded value.</returns>
        public string EncryptValue(string value)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            var protectedBytes = bytes.Concat(new byte[] { 0xEE }).ToArray();
            return Convert.ToBase64String(protectedBytes);
        }

    /// <summary>
    /// Gets the expected cookie name based on the base name and HTTPS usage.
    /// </summary>
    /// <param name="baseName">The base name of the cookie.</param>
    /// <param name="useHttps">Indicates whether HTTPS is used.</param>
    /// <param name="isHttps">Indicates whether the current request is HTTPS.</param>
    /// <returns>The expected cookie name, prefixed if HTTPS is used.</returns>
        public string GetExpectedCookieName(string baseName, bool useHttps, bool isHttps)
            => (useHttps || isHttps) ? $"{SecureCookiePrefix}{baseName}" : baseName;
    }

    #region ApplyTokenResponseContext Handler Tests

    /// <summary>
    /// Tests that when the client is not a back office client, the token response remains unchanged and no cookies are set.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ApplyTokenResponse_NonBackOfficeClient_DoesNothing()
    {
        // Arrange
        var setup = new TestSetup();
        setup.SetupHttpContext();

        var context = CreateApplyTokenResponseContext("other-client", AccessTokenValue, RefreshTokenValue);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(AccessTokenValue, context.Response.AccessToken);
        Assert.AreEqual(RefreshTokenValue, context.Response.RefreshToken);
        Assert.IsEmpty(setup.ResponseCookies);
    }

    /// <summary>
    /// Tests that the HandleAsync method redacts the access token in the response when using the BackOffice client.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ApplyTokenResponse_BackOfficeClient_RedactsAccessToken()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false);

        var context = CreateApplyTokenResponseContext(Constants.OAuthClientIds.BackOffice, AccessTokenValue, null);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(RedactedTokenValue, context.Response.AccessToken);
        Assert.IsTrue(setup.ResponseCookies.ContainsKey(AccessTokenCookieName));
    }

    /// <summary>
    /// Verifies that <c>HandleAsync</c> redacts the refresh token in the token response when the client is a BackOffice client.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ApplyTokenResponse_BackOfficeClient_RedactsRefreshToken()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false);

        var context = CreateApplyTokenResponseContext(Constants.OAuthClientIds.BackOffice, null, RefreshTokenValue);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(RedactedTokenValue, context.Response.RefreshToken);
        Assert.IsTrue(setup.ResponseCookies.ContainsKey(RefreshTokenCookieName));
    }

    /// <summary>
    /// Tests that the HandleAsync method redacts both access and refresh tokens in the response
    /// when applied for the BackOffice client.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ApplyTokenResponse_BackOfficeClient_RedactsBothTokens()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false);

        var context = CreateApplyTokenResponseContext(Constants.OAuthClientIds.BackOffice, AccessTokenValue, RefreshTokenValue);

        // Act
        await setup.Sut.HandleAsync(context);

        setup.ResponseCookies.TryGetValue(AccessTokenCookieName, out var accessTokenCookieValue);
        var unprotectedAccessTokenCookieValue = setup.DataProtector.Object.Unprotect(accessTokenCookieValue!);

        setup.ResponseCookies.TryGetValue(RefreshTokenCookieName, out var refreshTokenCookieValue);
        var unprotectedRefreshTokenCookieValue = setup.DataProtector.Object.Unprotect(refreshTokenCookieValue!);

        // Assert
        Assert.AreEqual(RedactedTokenValue, context.Response.AccessToken);
        Assert.AreEqual(RedactedTokenValue, context.Response.RefreshToken);
        Assert.IsTrue(setup.ResponseCookies.ContainsKey(AccessTokenCookieName));
        Assert.IsTrue(setup.ResponseCookies.ContainsKey(RefreshTokenCookieName));
        Assert.AreEqual(AccessTokenValue, unprotectedAccessTokenCookieValue);
        Assert.AreEqual(RefreshTokenValue, unprotectedRefreshTokenCookieValue);
    }

    /// <summary>
    /// Tests the ApplyTokenResponse method to ensure the cookie prefix is applied correctly based on HTTPS settings.
    /// </summary>
    /// <param name="useHttps">Indicates whether HTTPS is used in global settings.</param>
    /// <param name="isHttps">Indicates whether the current request is HTTPS.</param>
    /// <param name="expectSecurePrefix">Indicates whether the secure prefix is expected on the cookies.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestCase(true, false, true, TestName = "ApplyTokenResponse_UseHttpsTrue_HttpRequest_UsesSecurePrefix")]
    [TestCase(true, true, true, TestName = "ApplyTokenResponse_UseHttpsTrue_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, true, true, TestName = "ApplyTokenResponse_UseHttpsFalse_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, false, false, TestName = "ApplyTokenResponse_UseHttpsFalse_HttpRequest_NoPrefix")]
    public async Task ApplyTokenResponse_CookiePrefix(bool useHttps, bool isHttps, bool expectSecurePrefix)
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = useHttps;
        setup.SetupHttpContext(isHttps: isHttps);

        var context = CreateApplyTokenResponseContext(Constants.OAuthClientIds.BackOffice, AccessTokenValue, RefreshTokenValue);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        var expectedAccessCookieName = setup.GetExpectedCookieName(AccessTokenCookieName, useHttps, isHttps);
        var expectedRefreshCookieName = setup.GetExpectedCookieName(RefreshTokenCookieName, useHttps, isHttps);

        Assert.IsTrue(setup.ResponseCookies.ContainsKey(expectedAccessCookieName), $"Expected cookie '{expectedAccessCookieName}' not found");
        Assert.IsTrue(setup.ResponseCookies.ContainsKey(expectedRefreshCookieName), $"Expected cookie '{expectedRefreshCookieName}' not found");

        if (!expectSecurePrefix)
        {
            Assert.IsFalse(setup.ResponseCookies.ContainsKey($"{SecureCookiePrefix}{AccessTokenCookieName}"));
            Assert.IsFalse(setup.ResponseCookies.ContainsKey($"{SecureCookiePrefix}{RefreshTokenCookieName}"));
        }
    }

    /// <summary>
    /// Tests that the token response applies the cookie site name postfix correctly,
    /// ensuring that cookies are set with the expected site name suffix.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyTokenResponse_CookieSiteNamePostfix()
    {
        // Arrange
        var setup = new TestSetup();
        setup.BackOfficeTokenCookieSettings.SiteName = "-test-site-name";
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false);

        var context = CreateApplyTokenResponseContext(Constants.OAuthClientIds.BackOffice, AccessTokenValue, RefreshTokenValue);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.IsFalse(setup.ResponseCookies.ContainsKey(AccessTokenCookieName));
        Assert.IsFalse(setup.ResponseCookies.ContainsKey(RefreshTokenCookieName));
        Assert.IsTrue(setup.ResponseCookies.ContainsKey($"{AccessTokenCookieName}-test-site-name"));
        Assert.IsTrue(setup.ResponseCookies.ContainsKey($"{RefreshTokenCookieName}-test-site-name"));
    }

    #endregion

    #region ApplyAuthorizationResponseContext Handler Tests

    /// <summary>
    /// Tests that the HandleAsync method does nothing when the client is not a back office client.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_ApplyAuthorizationResponse_NonBackOfficeClient_DoesNothing()
    {
        // Arrange
        var setup = new TestSetup();
        setup.SetupHttpContext();

        var context = CreateApplyAuthorizationResponseContext("other-client", PkceCodeValue);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(PkceCodeValue, context.Response.Code);
        Assert.IsEmpty(setup.ResponseCookies);
    }

    /// <summary>
    /// Tests that the HandleAsync method redacts the PKCE code in the authorization response for the BackOffice client.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ApplyAuthorizationResponse_BackOfficeClient_RedactsPkceCode()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false);

        var context = CreateApplyAuthorizationResponseContext(Constants.OAuthClientIds.BackOffice, PkceCodeValue);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(RedactedTokenValue, context.Response.Code);
        Assert.IsTrue(setup.ResponseCookies.ContainsKey(PkceCodeCookieName));
    }

    /// <summary>
    /// Verifies that the <c>ApplyAuthorizationResponse</c> method applies the correct cookie name prefix (secure or not)
    /// depending on the global HTTPS setting and the current HTTP request scheme.
    /// </summary>
    /// <param name="useHttps">If <c>true</c>, HTTPS is enabled in the application's global settings; otherwise, HTTP is used.</param>
    /// <param name="isHttps">If <c>true</c>, the current HTTP request is made over HTTPS; otherwise, it is over HTTP.</param>
    /// <param name="expectSecurePrefix">If <c>true</c>, the cookie name is expected to include the secure prefix; otherwise, it should not.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [TestCase(true, false, true, TestName = "ApplyAuthorizationResponse_UseHttpsTrue_HttpRequest_UsesSecurePrefix")]
    [TestCase(true, true, true, TestName = "ApplyAuthorizationResponse_UseHttpsTrue_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, true, true, TestName = "ApplyAuthorizationResponse_UseHttpsFalse_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, false, false, TestName = "ApplyAuthorizationResponse_UseHttpsFalse_HttpRequest_NoPrefix")]
    public async Task ApplyAuthorizationResponse_CookiePrefix(bool useHttps, bool isHttps, bool expectSecurePrefix)
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = useHttps;
        setup.SetupHttpContext(isHttps: isHttps);

        var context = CreateApplyAuthorizationResponseContext(Constants.OAuthClientIds.BackOffice, PkceCodeValue);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        var expectedCookieName = setup.GetExpectedCookieName(PkceCodeCookieName, useHttps, isHttps);
        Assert.IsTrue(setup.ResponseCookies.ContainsKey(expectedCookieName), $"Expected cookie '{expectedCookieName}' not found");

        if (!expectSecurePrefix)
        {
            Assert.IsFalse(setup.ResponseCookies.ContainsKey($"{SecureCookiePrefix}{PkceCodeCookieName}"));
        }
    }

    /// <summary>
    /// Verifies that the authorization response applies the configured site name postfix to the PKCE code cookie name.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ApplyAuthorizationResponse_CookieSiteNamePostfix()
    {
        // Arrange
        var setup = new TestSetup();
        setup.BackOfficeTokenCookieSettings.SiteName = "-test-site-name";
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false);

        var context = CreateApplyAuthorizationResponseContext(Constants.OAuthClientIds.BackOffice, PkceCodeValue);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.IsFalse(setup.ResponseCookies.ContainsKey(PkceCodeCookieName));
        Assert.IsTrue(setup.ResponseCookies.ContainsKey($"{PkceCodeCookieName}-test-site-name"));
    }

    #endregion

    #region ExtractTokenRequestContext Handler Tests

    /// <summary>
    /// Tests that the HandleAsync method does nothing when the client is not a back-office client.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_ExtractTokenRequest_NonBackOfficeClient_DoesNothing()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false);

        var context = CreateExtractTokenRequestContext("other-client", RedactedTokenValue, RedactedTokenValue);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert - values should remain unchanged (handler skips non-back-office clients)
        Assert.AreEqual(RedactedTokenValue, context.Request.Code);
        Assert.AreEqual(RedactedTokenValue, context.Request.RefreshToken);
    }

    /// <summary>
    /// Tests that the HandleAsync method correctly restores the PKCE code from the cookie during an ExtractTokenRequest.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ExtractTokenRequest_RestoresPkceCodeFromCookie()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        var encryptedCode = setup.EncryptValue(PkceCodeValue);
        setup.SetupHttpContext(isHttps: false, requestCookies: new Dictionary<string, string> { { PkceCodeCookieName, encryptedCode } });

        var context = CreateExtractTokenRequestContext(Constants.OAuthClientIds.BackOffice, RedactedTokenValue, null);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(PkceCodeValue, context.Request.Code);
    }

    /// <summary>
    /// Tests that the PKCE code cookie is removed after handling an extract token request.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ExtractTokenRequest_RemovesPkceCodeCookieAfterUse()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        var encryptedCode = setup.EncryptValue(PkceCodeValue);
        setup.SetupHttpContext(isHttps: false, requestCookies: new Dictionary<string, string> { { PkceCodeCookieName, encryptedCode } });

        var context = CreateExtractTokenRequestContext(Constants.OAuthClientIds.BackOffice, RedactedTokenValue, null);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.IsTrue(setup.DeletedCookies.Contains(PkceCodeCookieName));
    }

    /// <summary>
    /// Verifies that <c>HandleAsync</c> removes a non-redacted PKCE code from an <c>ExtractTokenRequest</c> for security reasons.
    /// </summary>
    /// <remarks>
    /// Ensures that sensitive PKCE codes are not retained in the request context after handling, mitigating potential security risks.
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ExtractTokenRequest_DiscardsNonRedactedPkceCode()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false);

        var context = CreateExtractTokenRequestContext(Constants.OAuthClientIds.BackOffice, "non-redacted-code", null);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert - code should be nullified for security
        Assert.IsNull(context.Request.Code);
    }

    /// <summary>
    /// Tests that the HandleAsync method correctly restores the refresh token from the cookie
    /// when handling an extract token request for the back office.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ExtractTokenRequest_RestoresRefreshTokenFromCookie()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        var encryptedToken = setup.EncryptValue(RefreshTokenValue);
        setup.SetupHttpContext(isHttps: false, requestCookies: new Dictionary<string, string> { { RefreshTokenCookieName, encryptedToken } });

        var context = CreateExtractTokenRequestContext(Constants.OAuthClientIds.BackOffice, null, RedactedTokenValue);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(RefreshTokenValue, context.Request.RefreshToken);
    }

    /// <summary>
    /// Verifies that <c>HandleAsync</c> removes a non-redacted refresh token from an extract token request for security reasons.
    /// </summary>
    /// <remarks>
    /// This test ensures that when a non-redacted refresh token is present in the request, it is discarded (set to <c>null</c>) to prevent potential security risks.
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ExtractTokenRequest_DiscardsNonRedactedRefreshToken()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false);

        var context = CreateExtractTokenRequestContext(Constants.OAuthClientIds.BackOffice, null, "non-redacted-token");

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert - token should be nullified for security
        Assert.IsNull(context.Request.RefreshToken);
    }

    /// <summary>
    /// Verifies that the correct cookie prefix is used when extracting the PKCE code from a token request,
    /// depending on the global HTTPS setting and the current HTTP request scheme.
    /// </summary>
    /// <param name="useHttps">If set to <c>true</c>, HTTPS is enabled in global settings; otherwise, HTTP is used.</param>
    /// <param name="isHttps">If set to <c>true</c>, the current HTTP request is over HTTPS; otherwise, it is over HTTP.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestCase(true, false, TestName = "ExtractTokenRequest_PkceCode_UseHttpsTrue_HttpRequest_UsesSecurePrefix")]
    [TestCase(true, true, TestName = "ExtractTokenRequest_PkceCode_UseHttpsTrue_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, true, TestName = "ExtractTokenRequest_PkceCode_UseHttpsFalse_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, false, TestName = "ExtractTokenRequest_PkceCode_UseHttpsFalse_HttpRequest_NoPrefix")]
    public async Task ExtractTokenRequest_PkceCode_CookiePrefix(bool useHttps, bool isHttps)
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = useHttps;
        var encryptedCode = setup.EncryptValue(PkceCodeValue);
        var expectedCookieName = setup.GetExpectedCookieName(PkceCodeCookieName, useHttps, isHttps);
        setup.SetupHttpContext(isHttps: isHttps, requestCookies: new Dictionary<string, string>
        {
            { expectedCookieName, encryptedCode }
        });

        var context = CreateExtractTokenRequestContext(Constants.OAuthClientIds.BackOffice, RedactedTokenValue, null);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(PkceCodeValue, context.Request.Code);
        Assert.IsTrue(setup.DeletedCookies.Contains(expectedCookieName), $"Expected cookie '{expectedCookieName}' to be deleted");

        if (!useHttps && !isHttps)
        {
            Assert.IsFalse(setup.DeletedCookies.Contains($"{SecureCookiePrefix}{PkceCodeCookieName}"));
        }
    }

    /// <summary>
    /// Verifies that the refresh token is correctly extracted from the appropriate cookie, depending on whether HTTPS is enforced in global settings and whether the current HTTP request is secure (HTTPS).
    /// </summary>
    /// <param name="useHttps">If <c>true</c>, HTTPS is enforced in the application's global settings, affecting the cookie prefix used for the refresh token.</param>
    /// <param name="isHttps">If <c>true</c>, the current HTTP request is made over HTTPS, which may affect the cookie prefix used.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestCase(true, false, TestName = "ExtractTokenRequest_RefreshToken_UseHttpsTrue_HttpRequest_UsesSecurePrefix")]
    [TestCase(true, true, TestName = "ExtractTokenRequest_RefreshToken_UseHttpsTrue_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, true, TestName = "ExtractTokenRequest_RefreshToken_UseHttpsFalse_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, false, TestName = "ExtractTokenRequest_RefreshToken_UseHttpsFalse_HttpRequest_NoPrefix")]
    public async Task ExtractTokenRequest_RefreshToken_CookiePrefix(bool useHttps, bool isHttps)
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = useHttps;
        var encryptedToken = setup.EncryptValue(RefreshTokenValue);
        var expectedCookieName = setup.GetExpectedCookieName(RefreshTokenCookieName, useHttps, isHttps);
        setup.SetupHttpContext(isHttps: isHttps, requestCookies: new Dictionary<string, string>
        {
            { expectedCookieName, encryptedToken }
        });

        var context = CreateExtractTokenRequestContext(Constants.OAuthClientIds.BackOffice, null, RedactedTokenValue);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(RefreshTokenValue, context.Request.RefreshToken);
    }

    #endregion

    #region ExtractRevocationRequestContext Handler Tests

    /// <summary>
    /// Tests that the HandleAsync method does nothing when the revocation request is from a non-back-office client.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_ExtractRevocationRequest_NonBackOfficeClient_DoesNothing()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false);

        var context = CreateExtractRevocationRequestContext("other-client", RedactedTokenValue, "access_token");

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert - values should remain unchanged (handler skips non-back-office clients)
        Assert.AreEqual(RedactedTokenValue, context.Request.Token);
    }

    /// <summary>
    /// Tests that the HandleAsync method correctly extracts the revocation request and restores the access token from the cookie.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ExtractRevocationRequest_RestoresAccessTokenFromCookie()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        var encryptedToken = setup.EncryptValue(AccessTokenValue);
        setup.SetupHttpContext(isHttps: false, requestCookies: new Dictionary<string, string> { { AccessTokenCookieName, encryptedToken } });

        var context = CreateExtractRevocationRequestContext(Constants.OAuthClientIds.BackOffice, RedactedTokenValue, "access_token");

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(AccessTokenValue, context.Request.Token);
    }

    /// <summary>
    /// Tests that HandleAsync correctly extracts the revocation request and restores the refresh token from the cookie.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ExtractRevocationRequest_RestoresRefreshTokenFromCookie()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        var encryptedToken = setup.EncryptValue(RefreshTokenValue);
        setup.SetupHttpContext(isHttps: false, requestCookies: new Dictionary<string, string> { { RefreshTokenCookieName, encryptedToken } });

        var context = CreateExtractRevocationRequestContext(Constants.OAuthClientIds.BackOffice, RedactedTokenValue, "refresh_token");

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(RefreshTokenValue, context.Request.Token);
    }

    /// <summary>
    /// Tests that the HandleAsync method correctly extracts a revocation request and discards a non-redacted token.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ExtractRevocationRequest_DiscardsNonRedactedToken()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false);

        var context = CreateExtractRevocationRequestContext(Constants.OAuthClientIds.BackOffice, "non-redacted-token", "access_token");

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert - token should be nullified for security
        Assert.IsNull(context.Request.Token);
    }

    /// <summary>
    /// Tests that when no token type hint is provided, the handler tries to extract the access token from the cookie.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ExtractRevocationRequest_NoTokenTypeHint_TriesAccessTokenCookie()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        var encryptedToken = setup.EncryptValue(AccessTokenValue);
        setup.SetupHttpContext(isHttps: false, requestCookies: new Dictionary<string, string> { { AccessTokenCookieName, encryptedToken } });

        var context = CreateExtractRevocationRequestContext(Constants.OAuthClientIds.BackOffice, RedactedTokenValue, null);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert - without a hint, it should default to access token cookie
        Assert.AreEqual(AccessTokenValue, context.Request.Token);
    }

    /// <summary>
    /// Tests that when handling an extract revocation request with a redacted token but the cookie is missing,
    /// the token is nullified to prevent downstream errors.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ExtractRevocationRequest_RedactedTokenButCookieMissing_NullsToken()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false); // no cookies

        var context = CreateExtractRevocationRequestContext(Constants.OAuthClientIds.BackOffice, RedactedTokenValue, "access_token");

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert - token should be nullified to prevent IDX10400 errors downstream
        Assert.IsNull(context.Request.Token);
    }

    /// <summary>
    /// Tests that the access token cookie prefix is correctly extracted based on the UseHttps setting and the request's HTTPS status.
    /// </summary>
    /// <param name="useHttps">Indicates whether HTTPS is enabled in the global settings.</param>
    /// <param name="isHttps">Indicates whether the current HTTP request is using HTTPS.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestCase(true, false, TestName = "ExtractRevocationRequest_UseHttpsTrue_HttpRequest_UsesSecurePrefix")]
    [TestCase(true, true, TestName = "ExtractRevocationRequest_UseHttpsTrue_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, true, TestName = "ExtractRevocationRequest_UseHttpsFalse_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, false, TestName = "ExtractRevocationRequest_UseHttpsFalse_HttpRequest_NoPrefix")]
    public async Task ExtractRevocationRequest_AccessToken_CookiePrefix(bool useHttps, bool isHttps)
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = useHttps;
        var encryptedToken = setup.EncryptValue(AccessTokenValue);
        var expectedCookieName = setup.GetExpectedCookieName(AccessTokenCookieName, useHttps, isHttps);
        setup.SetupHttpContext(isHttps: isHttps, requestCookies: new Dictionary<string, string>
        {
            { expectedCookieName, encryptedToken }
        });

        var context = CreateExtractRevocationRequestContext(Constants.OAuthClientIds.BackOffice, RedactedTokenValue, "access_token");

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(AccessTokenValue, context.Request.Token);
    }

    /// <summary>
    /// Verifies that the correct cookie prefix is used when extracting the refresh token from a revocation request,
    /// depending on the global HTTPS setting and the current HTTP request's HTTPS status.
    /// </summary>
    /// <param name="useHttps">True if HTTPS is enabled in global settings; otherwise, false.</param>
    /// <param name="isHttps">True if the current HTTP request is over HTTPS; otherwise, false.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestCase(true, false, TestName = "ExtractRevocationRequest_RefreshToken_UseHttpsTrue_HttpRequest_UsesSecurePrefix")]
    [TestCase(true, true, TestName = "ExtractRevocationRequest_RefreshToken_UseHttpsTrue_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, true, TestName = "ExtractRevocationRequest_RefreshToken_UseHttpsFalse_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, false, TestName = "ExtractRevocationRequest_RefreshToken_UseHttpsFalse_HttpRequest_NoPrefix")]
    public async Task ExtractRevocationRequest_RefreshToken_CookiePrefix(bool useHttps, bool isHttps)
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = useHttps;
        var encryptedToken = setup.EncryptValue(RefreshTokenValue);
        var expectedCookieName = setup.GetExpectedCookieName(RefreshTokenCookieName, useHttps, isHttps);
        setup.SetupHttpContext(isHttps: isHttps, requestCookies: new Dictionary<string, string>
        {
            { expectedCookieName, encryptedToken }
        });

        var context = CreateExtractRevocationRequestContext(Constants.OAuthClientIds.BackOffice, RedactedTokenValue, "refresh_token");

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(RefreshTokenValue, context.Request.Token);
    }

    #endregion

    #region ProcessAuthenticationContext Handler Tests

    /// <summary>
    /// Verifies that when processing authentication with a non-redacted access token, the handler leaves the token unchanged.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ProcessAuthentication_NonRedactedToken_DoesNothing()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false);

        var context = CreateProcessAuthenticationContext("actual-access-token");

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert - token should not be modified
        Assert.AreEqual("actual-access-token", context.AccessToken);
    }

    /// <summary>
    /// Tests that when processing authentication with a redacted token, the token is restored from the cookie.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ProcessAuthentication_RedactedToken_RestoresFromCookie()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        var encryptedToken = setup.EncryptValue(AccessTokenValue);
        setup.SetupHttpContext(isHttps: false, requestCookies: new Dictionary<string, string> { { AccessTokenCookieName, encryptedToken } });

        var context = CreateProcessAuthenticationContext(RedactedTokenValue);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(AccessTokenValue, context.AccessToken);
    }

    /// <summary>
    /// Tests the cookie prefix used in the authentication process based on HTTPS settings.
    /// </summary>
    /// <param name="useHttps">Indicates whether HTTPS is used in the global settings.</param>
    /// <param name="isHttps">Indicates whether the current request is HTTPS.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestCase(true, false, TestName = "ProcessAuthentication_UseHttpsTrue_HttpRequest_UsesSecurePrefix")]
    [TestCase(true, true, TestName = "ProcessAuthentication_UseHttpsTrue_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, true, TestName = "ProcessAuthentication_UseHttpsFalse_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, false, TestName = "ProcessAuthentication_UseHttpsFalse_HttpRequest_NoPrefix")]
    public async Task ProcessAuthentication_CookiePrefix(bool useHttps, bool isHttps)
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = useHttps;
        var encryptedToken = setup.EncryptValue(AccessTokenValue);
        var expectedCookieName = setup.GetExpectedCookieName(AccessTokenCookieName, useHttps, isHttps);
        setup.SetupHttpContext(isHttps: isHttps, requestCookies: new Dictionary<string, string>
        {
            { expectedCookieName, encryptedToken }
        });

        var context = CreateProcessAuthenticationContext(RedactedTokenValue);

        // Act
        await setup.Sut.HandleAsync(context);

        // Assert
        Assert.AreEqual(AccessTokenValue, context.AccessToken);
    }

    #endregion

    #region UserLogoutSuccessNotification Handler Tests

    /// <summary>
    /// Tests that the Handle method removes the access and refresh token cookies upon user logout.
    /// </summary>
    [Test]
    public void Handle_Logout_RemovesCookies()
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = false;
        setup.SetupHttpContext(isHttps: false);

        var notification = new UserLogoutSuccessNotification("127.0.0.1", "test-user-id", "test-user-id");

        // Act
        setup.Sut.Handle(notification);

        // Assert
        Assert.IsTrue(setup.DeletedCookies.Contains(AccessTokenCookieName));
        Assert.IsTrue(setup.DeletedCookies.Contains(RefreshTokenCookieName));
    }

    /// <summary>
    /// Tests that the Handle method does not throw an exception when there is no HttpContext during logout.
    /// </summary>
    [Test]
    public void Handle_Logout_NoHttpContext_DoesNotThrow()
    {
        // Arrange
        var setup = new TestSetup();
        setup.HttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var notification = new UserLogoutSuccessNotification("127.0.0.1", "test-user-id", "test-user-id");

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() => setup.Sut.Handle(notification));
    }

    /// <summary>
    /// Tests the cookie prefix used during logout based on HTTPS settings.
    /// </summary>
    /// <param name="useHttps">Indicates whether HTTPS is enabled in global settings.</param>
    /// <param name="isHttps">Indicates whether the current request is HTTPS.</param>
    /// <param name="expectSecurePrefix">Indicates whether a secure cookie prefix is expected.</param>
    [TestCase(true, false, true, TestName = "Logout_UseHttpsTrue_HttpRequest_UsesSecurePrefix")]
    [TestCase(true, true, true, TestName = "Logout_UseHttpsTrue_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, true, true, TestName = "Logout_UseHttpsFalse_HttpsRequest_UsesSecurePrefix")]
    [TestCase(false, false, false, TestName = "Logout_UseHttpsFalse_HttpRequest_NoPrefix")]
    public void Logout_CookiePrefix(bool useHttps, bool isHttps, bool expectSecurePrefix)
    {
        // Arrange
        var setup = new TestSetup();
        setup.GlobalSettings.UseHttps = useHttps;
        setup.SetupHttpContext(isHttps: isHttps);

        var notification = new UserLogoutSuccessNotification("127.0.0.1", "test-user-id", "test-user-id");

        // Act
        setup.Sut.Handle(notification);

        // Assert
        var expectedAccessCookieName = setup.GetExpectedCookieName(AccessTokenCookieName, useHttps, isHttps);
        var expectedRefreshCookieName = setup.GetExpectedCookieName(RefreshTokenCookieName, useHttps, isHttps);

        Assert.IsTrue(setup.DeletedCookies.Contains(expectedAccessCookieName), $"Expected cookie '{expectedAccessCookieName}' to be deleted");
        Assert.IsTrue(setup.DeletedCookies.Contains(expectedRefreshCookieName), $"Expected cookie '{expectedRefreshCookieName}' to be deleted");

        if (!expectSecurePrefix)
        {
            Assert.IsFalse(setup.DeletedCookies.Contains($"{SecureCookiePrefix}{AccessTokenCookieName}"));
            Assert.IsFalse(setup.DeletedCookies.Contains($"{SecureCookiePrefix}{RefreshTokenCookieName}"));
        }
    }

    #endregion

    #region Context Factory Methods

    private static OpenIddictServerEvents.ApplyTokenResponseContext CreateApplyTokenResponseContext(
        string? clientId,
        string? accessToken,
        string? refreshToken)
    {
        var transaction = new OpenIddictServerTransaction();
        var context = new OpenIddictServerEvents.ApplyTokenResponseContext(transaction)
        {
            Request = new OpenIddict.Abstractions.OpenIddictRequest { ClientId = clientId },
            Response = new OpenIddict.Abstractions.OpenIddictResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            },
        };

        return context;
    }

    private static OpenIddictServerEvents.ApplyAuthorizationResponseContext CreateApplyAuthorizationResponseContext(
        string? clientId,
        string? code)
    {
        var transaction = new OpenIddictServerTransaction();
        var context = new OpenIddictServerEvents.ApplyAuthorizationResponseContext(transaction)
        {
            Request = new OpenIddict.Abstractions.OpenIddictRequest { ClientId = clientId },
            Response = new OpenIddict.Abstractions.OpenIddictResponse { Code = code },
        };

        return context;
    }

    private static OpenIddictServerEvents.ExtractTokenRequestContext CreateExtractTokenRequestContext(
        string? clientId,
        string? code,
        string? refreshToken)
    {
        var transaction = new OpenIddictServerTransaction();
        var context = new OpenIddictServerEvents.ExtractTokenRequestContext(transaction)
        {
            Request = new OpenIddict.Abstractions.OpenIddictRequest
            {
                ClientId = clientId,
                Code = code,
                RefreshToken = refreshToken,
            }
        };

        return context;
    }

    private static OpenIddictServerEvents.ExtractRevocationRequestContext CreateExtractRevocationRequestContext(
        string? clientId,
        string? token,
        string? tokenTypeHint)
    {
        var transaction = new OpenIddictServerTransaction();
        var context = new OpenIddictServerEvents.ExtractRevocationRequestContext(transaction)
        {
            Request = new OpenIddict.Abstractions.OpenIddictRequest
            {
                ClientId = clientId,
                Token = token,
                TokenTypeHint = tokenTypeHint,
            },
        };

        return context;
    }

    private static OpenIddictValidationEvents.ProcessAuthenticationContext CreateProcessAuthenticationContext(
        string? accessToken)
    {
        var transaction = new OpenIddictValidationTransaction();
        var context = new OpenIddictValidationEvents.ProcessAuthenticationContext(transaction)
        {
            AccessToken = accessToken
        };

        return context;
    }

    #endregion

    #region Mock Cookie Classes

    private class MockRequestCookieCollection : IRequestCookieCollection
    {
        private readonly Dictionary<string, string> _cookies;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockRequestCookieCollection"/> class with the specified cookies.
    /// </summary>
    /// <param name="cookies">The dictionary of cookie names and values.</param>
        public MockRequestCookieCollection(Dictionary<string, string> cookies) => _cookies = cookies;

    /// <summary>
    /// Gets the cookie value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the cookie to retrieve.</param>
    /// <returns>The cookie value if found; otherwise, null.</returns>
        public string? this[string key] => _cookies.TryGetValue(key, out var value) ? value : null;
    /// <summary>
    /// Gets the number of cookies in the collection.
    /// </summary>
        public int Count => _cookies.Count;
    /// <summary>
    /// Gets the collection of keys in the cookie collection.
    /// </summary>
        public ICollection<string> Keys => _cookies.Keys;
    /// <summary>
    /// Determines whether the collection contains a cookie with the specified key.
    /// </summary>
    /// <param name="key">The key of the cookie to locate.</param>
    /// <returns>True if the collection contains a cookie with the specified key; otherwise, false.</returns>
        public bool ContainsKey(string key) => _cookies.ContainsKey(key);
    /// <summary>
    /// Attempts to get the value of a cookie by its key.
    /// </summary>
    /// <param name="key">The key of the cookie to retrieve.</param>
    /// <param name="value">When this method returns, contains the cookie value associated with the specified key, if the key is found; otherwise, null.</param>
    /// <returns>True if the cookie with the specified key was found; otherwise, false.</returns>
        public bool TryGetValue(string key, out string? value) => _cookies.TryGetValue(key, out value);
    /// <summary>
    /// Returns an enumerator that iterates through the collection of cookies.
    /// </summary>
    /// <returns>An enumerator for the collection of key-value pairs representing cookies.</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _cookies.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private class MockRequestCookiesFeature : IRequestCookiesFeature
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="MockRequestCookiesFeature"/> class with the specified cookies.
    /// </summary>
    /// <param name="cookies">The collection of request cookies.</param>
        public MockRequestCookiesFeature(IRequestCookieCollection cookies) => Cookies = cookies;
    /// <summary>
    /// Gets or sets the collection of request cookies.
    /// </summary>
        public IRequestCookieCollection Cookies { get; set; }
    }

    private class MockResponseCookies : IResponseCookies
    {
        private readonly Dictionary<string, string> _cookies;
        private readonly HashSet<string> _deletedCookies;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockResponseCookies"/> class with the specified cookies and deleted cookies.
    /// </summary>
    /// <param name="cookies">A dictionary containing the initial cookies.</param>
    /// <param name="deletedCookies">A set containing the names of cookies that have been deleted.</param>
        public MockResponseCookies(Dictionary<string, string> cookies, HashSet<string> deletedCookies)
        {
            _cookies = cookies;
            _deletedCookies = deletedCookies;
        }

    /// <summary>
    /// Appends a cookie with the specified key and value.
    /// </summary>
    /// <param name="key">The key of the cookie.</param>
    /// <param name="value">The value of the cookie.</param>
        public void Append(string key, string value) => _cookies[key] = value;
    /// <summary>Appends a cookie with the specified key, value, and options.</summary>
    /// <param name="key">The key of the cookie to append.</param>
    /// <param name="value">The value of the cookie to append.</param>
    /// <param name="options">The cookie options to use when appending the cookie.</param>
        public void Append(string key, string value, CookieOptions options) => _cookies[key] = value;
    /// <summary>
    /// Deletes the cookie with the specified key.
    /// </summary>
    /// <param name="key">The key of the cookie to delete.</param>
        public void Delete(string key) => _deletedCookies.Add(key);
    /// <summary>
    /// Deletes the cookie with the specified key and options.
    /// </summary>
    /// <param name="key">The key of the cookie to delete.</param>
    /// <param name="options">The options for the cookie to delete.</param>
        public void Delete(string key, CookieOptions options) => _deletedCookies.Add(key);
    }

    private class MockResponseCookiesFeature : IResponseCookiesFeature
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="MockResponseCookiesFeature"/> class.
    /// </summary>
    /// <param name="cookies">The response cookies to wrap.</param>
        public MockResponseCookiesFeature(IResponseCookies cookies) => Cookies = cookies;
    /// <summary>
    /// Gets the response cookies.
    /// </summary>
        public IResponseCookies Cookies { get; }
    }

    #endregion
}
