// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

// FIXME: not testing virtual directory!
[TestFixture]
public class UriUtilityTests
{
    // test normal urls
    [TestCase("http://LocalHost/", "http://localhost/")]
    [TestCase("http://LocalHost/?x=y", "http://localhost/?x=y")]
    [TestCase("http://LocalHost/Home", "http://localhost/home")]
    [TestCase("http://LocalHost/Home?x=y", "http://localhost/home?x=y")]
    [TestCase("http://LocalHost/Home/Sub1", "http://localhost/home/sub1")]
    [TestCase("http://LocalHost/Home/Sub1?x=y", "http://localhost/home/sub1?x=y")]

    // test that the trailing slash goes but not on hostname
    [TestCase("http://LocalHost/", "http://localhost/")]
    [TestCase("http://LocalHost/Home/", "http://localhost/home")]
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
