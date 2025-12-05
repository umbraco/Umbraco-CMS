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
        [field: AllowNull]
        [field: MaybeNull]
        public HideBackOfficeTokensHandler Sut => field ??= new HideBackOfficeTokensHandler(
            HttpContextAccessor.Object,
            DataProtectionProvider.Object,
            Options.Create(BackOfficeTokenCookieSettings),
            Options.Create(GlobalSettings));

        public Mock<IHttpContextAccessor> HttpContextAccessor { get; } = new();

        private Mock<IDataProtectionProvider> DataProtectionProvider { get; } = new();

        public Mock<IDataProtector> DataProtector { get; } = new();

        private BackOfficeTokenCookieSettings BackOfficeTokenCookieSettings { get; set; } = new();

        public GlobalSettings GlobalSettings { get; set; } = new();

        private DefaultHttpContext HttpContext { get; set; } = new();

        public Dictionary<string, string> ResponseCookies { get; } = new();

        public HashSet<string> DeletedCookies { get; } = new();

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

        public string EncryptValue(string value)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            var protectedBytes = bytes.Concat(new byte[] { 0xEE }).ToArray();
            return Convert.ToBase64String(protectedBytes);
        }

        public string GetExpectedCookieName(string baseName, bool useHttps, bool isHttps)
            => (useHttps || isHttps) ? $"{SecureCookiePrefix}{baseName}" : baseName;
    }

    #region ApplyTokenResponseContext Handler Tests

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

    #endregion

    #region ApplyAuthorizationResponseContext Handler Tests

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

    #endregion

    #region ExtractTokenRequestContext Handler Tests

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

    #region ProcessAuthenticationContext Handler Tests

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
        var context = new OpenIddictServerEvents.ApplyTokenResponseContext(transaction);

        context.Request = new OpenIddict.Abstractions.OpenIddictRequest { ClientId = clientId };
        context.Response = new OpenIddict.Abstractions.OpenIddictResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        return context;
    }

    private static OpenIddictServerEvents.ApplyAuthorizationResponseContext CreateApplyAuthorizationResponseContext(
        string? clientId,
        string? code)
    {
        var transaction = new OpenIddictServerTransaction();
        var context = new OpenIddictServerEvents.ApplyAuthorizationResponseContext(transaction);

        context.Request = new OpenIddict.Abstractions.OpenIddictRequest { ClientId = clientId };
        context.Response = new OpenIddict.Abstractions.OpenIddictResponse { Code = code };

        return context;
    }

    private static OpenIddictServerEvents.ExtractTokenRequestContext CreateExtractTokenRequestContext(
        string? clientId,
        string? code,
        string? refreshToken)
    {
        var transaction = new OpenIddictServerTransaction();
        var context = new OpenIddictServerEvents.ExtractTokenRequestContext(transaction);

        context.Request = new OpenIddict.Abstractions.OpenIddictRequest
        {
            ClientId = clientId,
            Code = code,
            RefreshToken = refreshToken
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

        public MockRequestCookieCollection(Dictionary<string, string> cookies) => _cookies = cookies;

        public string? this[string key] => _cookies.TryGetValue(key, out var value) ? value : null;
        public int Count => _cookies.Count;
        public ICollection<string> Keys => _cookies.Keys;
        public bool ContainsKey(string key) => _cookies.ContainsKey(key);
        public bool TryGetValue(string key, out string? value) => _cookies.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _cookies.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private class MockRequestCookiesFeature : IRequestCookiesFeature
    {
        public MockRequestCookiesFeature(IRequestCookieCollection cookies) => Cookies = cookies;
        public IRequestCookieCollection Cookies { get; set; }
    }

    private class MockResponseCookies : IResponseCookies
    {
        private readonly Dictionary<string, string> _cookies;
        private readonly HashSet<string> _deletedCookies;

        public MockResponseCookies(Dictionary<string, string> cookies, HashSet<string> deletedCookies)
        {
            _cookies = cookies;
            _deletedCookies = deletedCookies;
        }

        public void Append(string key, string value) => _cookies[key] = value;
        public void Append(string key, string value, CookieOptions options) => _cookies[key] = value;
        public void Delete(string key) => _deletedCookies.Add(key);
        public void Delete(string key, CookieOptions options) => _deletedCookies.Add(key);
    }

    private class MockResponseCookiesFeature : IResponseCookiesFeature
    {
        public MockResponseCookiesFeature(IResponseCookies cookies) => Cookies = cookies;
        public IResponseCookies Cookies { get; }
    }

    #endregion
}
