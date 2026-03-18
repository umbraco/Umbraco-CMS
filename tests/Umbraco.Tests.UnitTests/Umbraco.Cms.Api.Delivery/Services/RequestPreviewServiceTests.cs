using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Services;

/// <summary>
/// Contains unit tests for the <see cref="RequestPreviewService"/> class in the Umbraco CMS API delivery layer.
/// These tests verify the functionality and behavior of the preview request service.
/// </summary>
[TestFixture]
public class RequestPreviewServiceTests
{
    /// <summary>
    /// Tests that the IsPreview method returns the expected result based on the provided preview header value.
    /// </summary>
    /// <param name="headerValue">The value of the preview header to test.</param>
    /// <param name="expected">The expected boolean result indicating if preview mode is active.</param>
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
