using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Services;

[TestFixture]
public class RequestPreviewServiceTests
{
    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase("false", false)]
    [TestCase("true", true)]
    [TestCase("True", true)]
    public void IsPreview_Returns_Expected_Result(string? headerValue, bool expected)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Preview"] = headerValue;

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(httpContext);
        var sut = new RequestPreviewService(httpContextAccessorMock.Object);

        var result = sut.IsPreview();

        Assert.AreEqual(expected, result);
    }
}
