using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Services;

[TestFixture]
public class RequestHeaderHandlerTests
{
    private const string HeaderName = "TestHeader";
    [Test]
    public void GetHeaderValue_return_null_when_http_context_is_unavailable()
    {
        IHttpContextAccessor httpContextAccessor = Mock.Of<IHttpContextAccessor>();

        var sut = new TestRequestHeaderHandler(httpContextAccessor);

        Assert.IsNull(sut.TestGetHeaderValue(HeaderName));
    }

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
    public TestRequestHeaderHandler(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }

    public string? TestGetHeaderValue(string headerName) => base.GetHeaderValue(headerName);
}
