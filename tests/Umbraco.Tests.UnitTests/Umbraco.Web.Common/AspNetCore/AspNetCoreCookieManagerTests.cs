// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Web.Common.AspNetCore;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class AspNetCoreCookieManagerTests
{
    private const string CookieName = "testCookie";
    private const string CookieValue = "testValue";

    [Test]
    public void Can_Set_Cookie()
    {
        var httpContext = new DefaultHttpContext();
        var cookieManager = CreateCookieManager(httpContext);

        cookieManager.SetCookieValue(CookieName, CookieValue, true, true, "Strict");

        Assert.That(httpContext.Response.Headers.SetCookie, Is.EqualTo(GetExpectedCookie()));
    }

    [Test]
    public void Set_Cookie_With_Invalid_Same_Site_Value_Throws_Expected_Exception()
    {
        var httpContext = new DefaultHttpContext();
        var cookieManager = CreateCookieManager(httpContext);

        Assert.Throws<ArgumentException>(() => cookieManager.SetCookieValue(CookieName, CookieValue, true, true, "invalid"));
    }

    [Test]
    public void Can_Get_Cookie()
    {
        var httpContext = new DefaultHttpContext();
        AddCookieToRequest(httpContext);
        var cookieManager = CreateCookieManager(httpContext);

        var result = cookieManager.GetCookieValue(CookieName);

        Assert.That(result, Is.EqualTo(CookieValue));
    }

    [Test]
    public void Can_Verify_Cookie_Exists()
    {
        var httpContext = new DefaultHttpContext();
        AddCookieToRequest(httpContext);
        var cookieManager = CreateCookieManager(httpContext);

        var result = cookieManager.HasCookie(CookieName);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Can_Expire_Cookie()
    {
        var httpContext = new DefaultHttpContext();
        AddCookieToRequest(httpContext);
        var cookieManager = CreateCookieManager(httpContext);

        cookieManager.SetCookieValue(CookieName, CookieValue, true, true, "Strict");
        cookieManager.ExpireCookie(CookieName);

        var setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        Assert.That(setCookieHeader, Does.StartWith("testCookie="));
        Assert.That(setCookieHeader, Does.Contain($"expires="));
    }

    private static AspNetCoreCookieManager CreateCookieManager(DefaultHttpContext httpContext)
    {
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
        return new AspNetCoreCookieManager(httpContextAccessor);
    }

    private static void AddCookieToRequest(DefaultHttpContext httpContext)
    {
        var cookie = new StringValues(CookieName + "=" + CookieValue);
        httpContext.Request.Headers.Append(HeaderNames.Cookie, cookie);
    }

    private static string GetExpectedCookie() => $"testCookie={CookieValue}; path=/; secure; samesite=strict; httponly";
}
