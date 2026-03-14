using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Services;

/// <summary>
/// Contains unit tests for the <see cref="RequestHeaderHandler"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class RequestHeaderHandlerTests
{
    private const string HeaderName = "TestHeader";
    /// <summary>
    /// Tests that GetHeaderValue returns null when the HTTP context is unavailable.
    /// </summary>
    [Test]
    public void GetHeaderValue_return_null_when_http_context_is_unavailable()
    {
        IHttpContextAccessor httpContextAccessor = Mock.Of<IHttpContextAccessor>();

        var sut = new TestRequestHeaderHandler(httpContextAccessor);

        Assert.IsNull(sut.TestGetHeaderValue(HeaderName));
    }

    /// <summary>
    /// Tests that GetHeaderValue returns the header value when the HTTP context is available.
    /// </summary>
    [Test]
    public void GetHeaderValue_return_header_value_when_http_context_is_available()
    {

        const string headerValue = "TestValue";

        HttpContext httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[HeaderName] = headerValue;

        IHttpContextAccessor httpContextAccessor = Mock.Of<IHttpContextAccessor>();
        Mock.Get(httpContextAccessor).Setup(x => x.HttpContext).Returns(httpContext);

        var sut = new TestRequestHeaderHandler(httpContextAccessor);

        Assert.AreEqual(headerValue, sut.TestGetHeaderValue(HeaderName));
    }
}


internal class TestRequestHeaderHandler : RequestHeaderHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestRequestHeaderHandler"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public TestRequestHeaderHandler(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }

    /// <summary>
    /// Retrieves the value of the specified header.
    /// </summary>
    /// <param name="headerName">The name of the header to retrieve.</param>
    /// <returns>The value of the header if found; otherwise, null.</returns>
    public string? TestGetHeaderValue(string headerName) => base.GetHeaderValue(headerName);
}
