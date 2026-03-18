// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

/// <summary>
/// Contains unit tests for the <see cref="Umbraco.Core.Routing.WebPath"/> class.
/// </summary>
[TestFixture]
public class WebPathTests
{
    /// <summary>
    /// Combines multiple path parts into a single normalized path.
    /// </summary>
    /// <param name="parts">The parts of the path to combine.</param>
    /// <returns>A combined path string with normalized separators.</returns>
    [Test]
    [TestCase("/umbraco", "config", "lang", ExpectedResult = "/umbraco/config/lang")]
    [TestCase("/umbraco", "/config", "/lang", ExpectedResult = "/umbraco/config/lang")]
    [TestCase("/umbraco/", "/config/", "/lang/", ExpectedResult = "/umbraco/config/lang")]
    [TestCase("/umbraco/", "config/", "lang/", ExpectedResult = "/umbraco/config/lang")]
    [TestCase("umbraco", "config", "lang", ExpectedResult = "umbraco/config/lang")]
    [TestCase("umbraco", ExpectedResult = "umbraco")]
    [TestCase("~/umbraco", "config", "lang", ExpectedResult = "~/umbraco/config/lang")]
    [TestCase("~/umbraco", "/config", "/lang", ExpectedResult = "~/umbraco/config/lang")]
    [TestCase("~/umbraco/", "/config/", "/lang/", ExpectedResult = "~/umbraco/config/lang")]
    [TestCase("~/umbraco/", "config/", "lang/", ExpectedResult = "~/umbraco/config/lang")]
    [TestCase("~/umbraco", ExpectedResult = "~/umbraco")]
    [TestCase("https://hello.com/", "/world", ExpectedResult = "https://hello.com/world")]
    public string Combine(params string[] parts) => WebPath.Combine(parts);

    /// <summary>
    /// Tests that the Combine method correctly handles an empty array input.
    /// </summary>
    [Test]
    public void Combine_must_handle_empty_array() =>
        Assert.AreEqual(string.Empty, WebPath.Combine(Array.Empty<string>()));

    /// <summary>
    /// Tests that the Combine method throws an ArgumentNullException when passed a null argument.
    /// </summary>
    [Test]
    public void Combine_must_handle_null() => Assert.Throws<ArgumentNullException>(() => WebPath.Combine(null));


    /// <summary>
    /// Determines whether the specified web path is well-formed according to the given URI kind.
    /// </summary>
    /// <param name="webPath">The web path to validate.</param>
    /// <param name="uriKind">The kind of URI to validate against (Absolute, Relative, or RelativeOrAbsolute).</param>
    /// <returns>True if the web path is well-formed for the specified URI kind; otherwise, false.</returns>
    [Test]
    [TestCase("ftp://hello.com/", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("file:///hello.com/", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("ws://hello.com/", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("wss://hello.com/", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("https://hello.com:8080/", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("http://hello.com:8080/", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("https://hello.com/path", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("http://hello.com/path", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("https://hello.com/path?query=param", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("http://hello.com/path?query=param", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("https://hello.com/path#fragment", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("http://hello.com/path#fragment", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("https://hello.com/path?query=param#fragment", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("http://hello.com/path?query=param#fragment", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("https://hello.com:8080/path?query=param#fragment", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("http://hello.com:8080/path?query=param#fragment", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("//hello.com:8080/path?query=param#fragment", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("//hello.com:8080/path", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("//hello.com:8080", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("//hello.com", UriKind.Absolute, ExpectedResult = true)]
    [TestCase("/test/test.jpg", UriKind.Absolute, ExpectedResult = false)]
    [TestCase("/test", UriKind.Absolute, ExpectedResult = false)]
    [TestCase("test", UriKind.Absolute, ExpectedResult = false)]
    [TestCase("", UriKind.Absolute, ExpectedResult = false)]
    [TestCase(null, UriKind.Absolute, ExpectedResult = false)]
    [TestCase("this is not welformed", UriKind.Absolute, ExpectedResult = false)]
    [TestCase("ftp://hello.com/", UriKind.Relative, ExpectedResult = false)]
    [TestCase("file:///hello.com/", UriKind.Relative, ExpectedResult = false)]
    [TestCase("ws://hello.com/", UriKind.Relative, ExpectedResult = false)]
    [TestCase("wss://hello.com/", UriKind.Relative, ExpectedResult = false)]
    [TestCase("https://hello.com:8080/", UriKind.Relative, ExpectedResult = false)]
    [TestCase("http://hello.com:8080/", UriKind.Relative, ExpectedResult = false)]
    [TestCase("https://hello.com/path", UriKind.Relative, ExpectedResult = false)]
    [TestCase("http://hello.com/path", UriKind.Relative, ExpectedResult = false)]
    [TestCase("https://hello.com/path?query=param", UriKind.Relative, ExpectedResult = false)]
    [TestCase("http://hello.com/path?query=param", UriKind.Relative, ExpectedResult = false)]
    [TestCase("https://hello.com/path#fragment", UriKind.Relative, ExpectedResult = false)]
    [TestCase("http://hello.com/path#fragment", UriKind.Relative, ExpectedResult = false)]
    [TestCase("https://hello.com/path?query=param#fragment", UriKind.Relative, ExpectedResult = false)]
    [TestCase("http://hello.com/path?query=param#fragment", UriKind.Relative, ExpectedResult = false)]
    [TestCase("https://hello.com:8080/path?query=param#fragment", UriKind.Relative, ExpectedResult = false)]
    [TestCase("http://hello.com:8080/path?query=param#fragment", UriKind.Relative, ExpectedResult = false)]
    [TestCase("//hello.com:8080/path?query=param#fragment", UriKind.Relative, ExpectedResult = false)]
    [TestCase("//hello.com:8080/path", UriKind.Relative, ExpectedResult = false)]
    [TestCase("//hello.com:8080", UriKind.Relative, ExpectedResult = false)]
    [TestCase("//hello.com", UriKind.Relative, ExpectedResult = false)]
    [TestCase("/test/test.jpg", UriKind.Relative, ExpectedResult = true)]
    [TestCase("/test", UriKind.Relative, ExpectedResult = true)]
    [TestCase("test", UriKind.Relative, ExpectedResult = true)]
    [TestCase("", UriKind.Relative, ExpectedResult = false)]
    [TestCase(null, UriKind.Relative, ExpectedResult = false)]
    [TestCase("this is not welformed", UriKind.Relative, ExpectedResult = false)]
    [TestCase("ftp://hello.com/", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("file:///hello.com/", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("ws://hello.com/", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("wss://hello.com/", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("https://hello.com:8080/", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("http://hello.com:8080/", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("https://hello.com/path", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("http://hello.com/path", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("https://hello.com/path?query=param", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("http://hello.com/path?query=param", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("https://hello.com/path#fragment", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("http://hello.com/path#fragment", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("https://hello.com/path?query=param#fragment", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("http://hello.com/path?query=param#fragment", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("https://hello.com:8080/path?query=param#fragment", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("http://hello.com:8080/path?query=param#fragment", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("//hello.com:8080/path?query=param#fragment", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("//hello.com:8080/path", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("//hello.com:8080", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("//hello.com", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("/test/test.jpg", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("/test", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("test", UriKind.RelativeOrAbsolute, ExpectedResult = true)]
    [TestCase("", UriKind.RelativeOrAbsolute, ExpectedResult = false)]
    [TestCase(null, UriKind.RelativeOrAbsolute, ExpectedResult = false)]
    [TestCase("this is not welformed", UriKind.RelativeOrAbsolute, ExpectedResult = false)]
    public bool IsWellFormedWebPath(string? webPath, UriKind uriKind) => WebPath.IsWellFormedWebPath(webPath, uriKind);

}
