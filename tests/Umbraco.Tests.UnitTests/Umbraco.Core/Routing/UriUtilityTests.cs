// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

// TODO: not testing virtual directory!
/// <summary>
/// Contains unit tests for the <see cref="global::Umbraco.Core.Routing.UriUtility"/> class.
/// </summary>
[TestFixture]
public class UriUtilityTests
{
    // test normal urls
    /// <summary>
    /// Tests the UriToUmbraco method to ensure it converts source URLs to the expected Umbraco URLs correctly.
    /// </summary>
    /// <param name="sourceUrl">The source URL to convert.</param>
    /// <param name="expectedUrl">The expected Umbraco URL result after conversion.</param>
    [TestCase("http://LocalHost/", "http://localhost/")]
    [TestCase("http://LocalHost/?x=y", "http://localhost/?x=y")]
    [TestCase("http://LocalHost/Home", "http://localhost/home")]
    [TestCase("http://LocalHost/Home?x=y", "http://localhost/home?x=y")]
    [TestCase("http://LocalHost/Home/Sub1", "http://localhost/home/sub1")]
    [TestCase("http://LocalHost/Home/Sub1?x=y", "http://localhost/home/sub1?x=y")]

    // test that the trailing slash goes but not on hostname
    [TestCase("http://LocalHost/", "http://localhost/")]
    [TestCase("http://LocalHost/////", "http://localhost/")]
    [TestCase("http://LocalHost/Home/", "http://localhost/home")]
    [TestCase("http://LocalHost/Home/////", "http://localhost/home")]
    [TestCase("http://LocalHost/Home/?x=y", "http://localhost/home?x=y")]
    [TestCase("http://LocalHost/Home/Sub1/", "http://localhost/home/sub1")]
    [TestCase("http://LocalHost/Home/Sub1/?x=y", "http://localhost/home/sub1?x=y")]
    public void Uri_To_Umbraco(string sourceUrl, string expectedUrl)
    {
        // Arrange
        var sourceUri = new Uri(sourceUrl);
        var uriUtility = BuildUriUtility("/");

        // Act
        var resultUri = uriUtility.UriToUmbraco(sourceUri);

        // Assert
        var expectedUri = new Uri(expectedUrl);
        Assert.AreEqual(expectedUri.ToString(), resultUri.ToString());
    }

    // test directoryUrl true, trailingSlash false
    [TestCase("/", "/", false)]
    [TestCase("/home", "/home", false)]
    [TestCase("/home/sub1", "/home/sub1", false)]

    // test directoryUrl true, trailingSlash true
    [TestCase("/", "/", true)]
    [TestCase("/home", "/home/", true)]
    [TestCase("/home/sub1", "/home/sub1/", true)]
    public void Uri_From_Umbraco(string sourceUrl, string expectedUrl, bool trailingSlash)
    {
        // Arrange
        var sourceUri = new Uri(sourceUrl, UriKind.Relative);
        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = trailingSlash };
        var uriUtility = BuildUriUtility("/");

        // Act
        var resultUri = uriUtility.UriFromUmbraco(sourceUri, requestHandlerSettings);

        // Assert
        var expectedUri = new Uri(expectedUrl, UriKind.Relative);
        Assert.AreEqual(expectedUri.ToString(), resultUri.ToString());
    }

    /// <summary>
    /// Unit test for <see cref="UriUtility.ToAbsolute"/>, verifying that various combinations of virtual paths and source URLs are correctly converted to absolute URLs.
    /// </summary>
    /// <param name="virtualPath">The application virtual path used as the base for conversion (e.g., "/" or "/vdir").</param>
    /// <param name="sourceUrl">The source URL or path to convert to an absolute URL (may be rooted, relative, or app-relative).</param>
    /// <param name="expectedUrl">The expected absolute URL result after conversion.</param>
    [TestCase("/", "/", "/")]
    [TestCase("/", "/foo", "/foo")]
    [TestCase("/", "~/foo", "/foo")]
    [TestCase("/vdir", "/", "/vdir/")]
    [TestCase("/vdir", "/foo", "/vdir/foo")]
    [TestCase("/vdir", "/foo/", "/vdir/foo/")]
    [TestCase("/vdir", "~/foo", "/vdir/foo")]
    public void Uri_To_Absolute(string virtualPath, string sourceUrl, string expectedUrl)
    {
        // Arrange
        var uriUtility = BuildUriUtility(virtualPath);

        // Act
        var resultUrl = uriUtility.ToAbsolute(sourceUrl);

        // Assert
        Assert.AreEqual(expectedUrl, resultUrl);
    }

    /// <summary>
    /// Tests the conversion of a source URL to an application-relative URL based on the given virtual path.
    /// </summary>
    /// <param name="virtualPath">The virtual path of the application.</param>
    /// <param name="sourceUrl">The source URL to convert.</param>
    /// <param name="expectedUrl">The expected application-relative URL result.</param>
    [TestCase("/", "/", "/")]
    [TestCase("/", "/foo", "/foo")]
    [TestCase("/", "/foo/", "/foo/")]
    [TestCase("/vdir", "/vdir", "/")]
    [TestCase("/vdir", "/vdir/", "/")]
    [TestCase("/vdir", "/vdir/foo", "/foo")]
    [TestCase("/vdir", "/vdir/foo/", "/foo/")]
    public void Url_To_App_Relative(string virtualPath, string sourceUrl, string expectedUrl)
    {
        // Arrange
        var uriUtility = BuildUriUtility(virtualPath);

        // Act
        var resultUrl = uriUtility.ToAppRelative(sourceUrl);

        // Assert
        Assert.AreEqual(expectedUrl, resultUrl);
    }

    private UriUtility BuildUriUtility(string virtualPath)
    {
        var mockHostingEnvironment = new Mock<IHostingEnvironment>();
        mockHostingEnvironment.Setup(x => x.ApplicationVirtualPath).Returns(virtualPath);
        return new UriUtility(mockHostingEnvironment.Object);
    }
}
