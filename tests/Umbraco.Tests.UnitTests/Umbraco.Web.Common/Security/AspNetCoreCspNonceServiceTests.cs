// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Security;

[TestFixture]
public class AspNetCoreCspNonceServiceTests
{
    [Test]
    public void GetNonce_ReturnsNull_WhenHttpContextIsNull()
    {
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == null);
        var service = new AspNetCoreCspNonceService(httpContextAccessor);

        var result = service.GetNonce();

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetNonce_GeneratesNonce_WhenNoneExists()
    {
        var httpContext = new DefaultHttpContext();
        var service = CreateCspNonceService(httpContext);

        var result = service.GetNonce();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GetNonce_ReturnsSameNonce_OnSubsequentCalls()
    {
        var httpContext = new DefaultHttpContext();
        var service = CreateCspNonceService(httpContext);

        var firstCall = service.GetNonce();
        var secondCall = service.GetNonce();
        var thirdCall = service.GetNonce();

        Assert.That(firstCall, Is.EqualTo(secondCall));
        Assert.That(secondCall, Is.EqualTo(thirdCall));
    }

    [Test]
    public void GetNonce_GeneratesValidBase64String()
    {
        var httpContext = new DefaultHttpContext();
        var service = CreateCspNonceService(httpContext);

        var result = service.GetNonce();

        // Should be able to decode from Base64 without exception.
        Assert.DoesNotThrow(() => Convert.FromBase64String(result!));

        // 32 bytes generates ~44 character Base64 string (with padding).
        var decodedBytes = Convert.FromBase64String(result!);
        Assert.That(decodedBytes.Length, Is.EqualTo(32));
    }

    [Test]
    public void GetNonce_GeneratesDifferentNonces_ForDifferentRequests()
    {
        var httpContext1 = new DefaultHttpContext();
        var httpContext2 = new DefaultHttpContext();
        var service1 = CreateCspNonceService(httpContext1);
        var service2 = CreateCspNonceService(httpContext2);

        var nonce1 = service1.GetNonce();
        var nonce2 = service2.GetNonce();

        Assert.That(nonce1, Is.Not.EqualTo(nonce2));
    }

    [Test]
    public void GetNonce_StoresNonceInHttpContextItems()
    {
        var httpContext = new DefaultHttpContext();
        var service = CreateCspNonceService(httpContext);

        var result = service.GetNonce();

        Assert.That(httpContext.Items.ContainsKey(Constants.HttpContext.Items.CspNonce), Is.True);
        Assert.That(httpContext.Items[Constants.HttpContext.Items.CspNonce], Is.EqualTo(result));
    }

    [Test]
    public void GetNonce_ReturnsExistingNonce_WhenAlreadyInHttpContextItems()
    {
        var httpContext = new DefaultHttpContext();
        var existingNonce = "pre-existing-nonce-value";
        httpContext.Items[Constants.HttpContext.Items.CspNonce] = existingNonce;
        var service = CreateCspNonceService(httpContext);

        var result = service.GetNonce();

        Assert.That(result, Is.EqualTo(existingNonce));
    }

    private static AspNetCoreCspNonceService CreateCspNonceService(DefaultHttpContext httpContext)
    {
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
        return new AspNetCoreCspNonceService(httpContextAccessor);
    }
}
