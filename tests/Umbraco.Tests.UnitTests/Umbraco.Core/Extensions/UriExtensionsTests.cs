// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

/// <summary>
/// Contains unit tests for the <see cref="UriExtensions"/> class, which provides extension methods for <see cref="System.Uri"/>.
/// </summary>
[TestFixture]
public class UriExtensionsTests
{
    [TestCase("http://www.domain.com/foo/bar", "/", "http://www.domain.com/")]
    [TestCase("http://www.domain.com/foo/bar#hop", "/", "http://www.domain.com/")]
    [TestCase("http://www.domain.com/foo/bar?q=2#hop", "/", "http://www.domain.com/?q=2")]
    [TestCase("http://www.domain.com/foo/bar", "/path/to/page", "http://www.domain.com/path/to/page")]
    [TestCase("http://www.domain.com/foo/bar", "/path/to/page/", "http://www.domain.com/path/to/page/")]
    [TestCase("http://www.domain.com/", "/path/to/page/", "http://www.domain.com/path/to/page/")]
    [TestCase("http://www.domain.com", "/path/to/page/", "http://www.domain.com/path/to/page/")]
    [TestCase("http://www.domain.com/foo?q=3", "/path/to/page/", "http://www.domain.com/path/to/page/?q=3")]
    [TestCase("http://www.domain.com/foo#bang", "/path/to/page/", "http://www.domain.com/path/to/page/")]
    [TestCase("http://www.domain.com/foo?q=3#bang", "/path/to/page/", "http://www.domain.com/path/to/page/?q=3")]
    public void RewritePath(string input, string path, string expected)
    {
        var source = new Uri(input);
        var output = source.Rewrite(path);
        Assert.AreEqual(expected, output.ToString());
    }

    /// <summary>
    /// Unit test that verifies the <c>Rewrite</c> extension method on <see cref="Uri"/> throws the expected exception types when provided with invalid input URIs or paths.
    /// </summary>
    /// <param name="input">The input URI string to be tested.</param>
    /// <param name="path">The path value to attempt to rewrite the URI with.</param>
    /// <param name="exception">The expected <see cref="Type"/> of exception to be thrown.</param>
    [TestCase("http://www.domain.com/", "path/to/page/", typeof(ArgumentException))]
    [TestCase("http://www.domain.com", "path/to/page/", typeof(ArgumentException))]
    public void RewritePath_Exceptions(string input, string path, Type exception)
    {
        var source = new Uri(input);
        Assert.Throws(exception, () =>
        {
            var output = source.Rewrite(path);
        });
    }

    /// <summary>
    /// Verifies that the <c>Rewrite</c> extension method correctly rewrites the path and query components of a URI.
    /// </summary>
    /// <param name="input">The original URI as a string.</param>
    /// <param name="path">The new path to set on the URI.</param>
    /// <param name="query">The new query string to set on the URI (including the leading '?', or empty for none).</param>
    /// <param name="expected">The expected URI as a string after rewriting.</param>
    [TestCase("http://www.domain.com/foo/bar", "/path/to/page", "", "http://www.domain.com/path/to/page")]
    [TestCase("http://www.domain.com/foo/bar?k=3", "/path/to/page", "", "http://www.domain.com/path/to/page")]
    [TestCase("http://www.domain.com/foo/bar?k=3#hop", "/path/to/page", "", "http://www.domain.com/path/to/page")]
    [TestCase("http://www.domain.com/foo/bar", "/path/to/page", "?x=12", "http://www.domain.com/path/to/page?x=12")]
    [TestCase("http://www.domain.com/foo/bar#hop", "/path/to/page", "?x=12", "http://www.domain.com/path/to/page?x=12")]
    [TestCase("http://www.domain.com/foo/bar?k=3", "/path/to/page", "?x=12", "http://www.domain.com/path/to/page?x=12")]
    [TestCase("http://www.domain.com/foo/bar?k=3#hop", "/path/to/page", "?x=12", "http://www.domain.com/path/to/page?x=12")]
    public void RewritePathAndQuery(string input, string path, string query, string expected)
    {
        var source = new Uri(input);
        var output = source.Rewrite(path, query);
        Assert.AreEqual(expected, output.ToString());
    }

    /// <summary>
    /// Tests that the Rewrite method throws the expected exceptions for invalid path and query inputs.
    /// </summary>
    /// <param name="input">The original URI string to rewrite.</param>
    /// <param name="path">The path to rewrite to.</param>
    /// <param name="query">The query string to rewrite to.</param>
    /// <param name="exception">The type of exception expected to be thrown.</param>
    [TestCase("http://www.domain.com/", "path/to/page/", "", typeof(ArgumentException))]
    [TestCase("http://www.domain.com/", "/path/to/page/", "x=27", typeof(ArgumentException))]
    public void RewritePathAndQuery_Exceptions(string input, string path, string query, Type exception)
    {
        var source = new Uri(input);
        Assert.Throws(exception, () =>
        {
            var output = source.Rewrite(path, query);
        });
    }

    /// <summary>
    /// Unit test for the GetSafeAbsolutePath extension method, verifying that it returns the correct absolute path portion of a given URI string.
    /// </summary>
    /// <param name="input">The URI string to test.</param>
    /// <param name="expected">The expected absolute path result.</param>
    [TestCase("http://www.domain.com", "/")]
    [TestCase("http://www.domain.com/", "/")]
    [TestCase("http://www.domain.com/foo", "/foo")]
    [TestCase("http://www.domain.com/foo/", "/foo/")]
    [TestCase("http://www.domain.com/foo/bar", "/foo/bar")]
    [TestCase("http://www.domain.com/foo/bar%20nix", "/foo/bar%20nix")]
    [TestCase("http://www.domain.com/foo/bar?q=7#hop", "/foo/bar")]
    [TestCase("/", "/")]
    [TestCase("/foo", "/foo")]
    [TestCase("/foo/", "/foo/")]
    [TestCase("/foo/bar", "/foo/bar")]
    [TestCase("/foo/bar?q=7#hop", "/foo/bar")]
    [TestCase("/foo%20bar/pof", "/foo%20bar/pof")]
    public void GetSafeAbsolutePath(string input, string expected)
    {
        var source = new Uri(input, UriKind.RelativeOrAbsolute);
        var output = source.GetSafeAbsolutePath();
        Assert.AreEqual(expected, output);
    }

    /// <summary>
    /// Unit test for the <see cref="UriExtensions.GetAbsolutePathDecoded"/> extension method.
    /// Verifies that the method correctly decodes the absolute path component of a given URI string.
    /// </summary>
    /// <param name="input">The URI string whose absolute path will be decoded.</param>
    /// <param name="expected">The expected decoded absolute path result.</param>
    [TestCase("http://www.domain.com/foo/bar%20nix", "/foo/bar nix")]
    public void GetAbsolutePathDecoded(string input, string expected)
    {
        var source = new Uri(input, UriKind.RelativeOrAbsolute);
        var output = source.GetAbsolutePathDecoded();
        Assert.AreEqual(expected, output);
    }

    /// <summary>
    /// Unit test for <see cref="UriExtensions.GetSafeAbsolutePathDecoded"/>.
    /// Verifies that the method returns the expected decoded absolute path for a given input URI.
    /// </summary>
    /// <param name="input">The input URI string to be decoded.</param>
    /// <param name="expected">The expected decoded absolute path result.</param>
    [TestCase("http://www.domain.com/foo/bar%20nix", "/foo/bar nix")]
    [TestCase("/foo%20bar/pof", "/foo bar/pof")]
    public void GetSafeAbsolutePathDecoded(string input, string expected)
    {
        var source = new Uri(input, UriKind.RelativeOrAbsolute);
        var output = source.GetSafeAbsolutePathDecoded();
        Assert.AreEqual(expected, output);
    }

    /// <summary>
    /// Tests that the <c>EndPathWithSlash</c> extension method correctly appends a trailing slash to the path component of a URI if it is missing, while preserving the query string and removing any fragment.
    /// </summary>
    /// <param name="input">The input URI string to test.</param>
    /// <param name="expected">The expected URI string result after ensuring the path ends with a slash and removing any fragment.</param>
    [TestCase("http://www.domain.com/path/to/page", "http://www.domain.com/path/to/page/")]
    [TestCase("http://www.domain.com/path/to/", "http://www.domain.com/path/to/")]
    [TestCase("http://www.domain.com/", "http://www.domain.com/")]
    [TestCase("http://www.domain.com", "http://www.domain.com/")]
    [TestCase("http://www.domain.com/path/to?q=3#yop", "http://www.domain.com/path/to/?q=3")]
    public void EndPathWithSlash(string input, string expected)
    {
        var source = new Uri(input);
        var output = source.EndPathWithSlash();
        Assert.AreEqual(expected, output.ToString());
    }

    /// <summary>
    /// Tests the <c>TrimPathEndSlash</c> extension method to ensure it correctly trims the trailing slash from the path of a URI, if present.
    /// Also verifies that query strings and fragments are handled appropriately.
    /// </summary>
    /// <param name="input">The input URI string to be trimmed.</param>
    /// <param name="expected">The expected URI string after trimming the trailing slash from the path, if applicable.</param>
    [TestCase("http://www.domain.com/path/to/page", "http://www.domain.com/path/to/page")]
    [TestCase("http://www.domain.com/path/to/", "http://www.domain.com/path/to")]
    [TestCase("http://www.domain.com/", "http://www.domain.com/")]
    [TestCase("http://www.domain.com", "http://www.domain.com/")]
    [TestCase("http://www.domain.com/path/to/?q=3#yop", "http://www.domain.com/path/to?q=3")]
    public void TrimPathEndSlash(string input, string expected)
    {
        var source = new Uri(input);
        var output = source.TrimPathEndSlash();
        Assert.AreEqual(expected, output.ToString());
    }

    /// <summary>
    /// Tests the MakeAbsolute extension method to ensure it correctly converts a relative URI to an absolute URI based on a reference URI.
    /// </summary>
    /// <param name="input">The relative URI string to be made absolute.</param>
    /// <param name="reference">The base absolute URI string used as the reference for making the input absolute.</param>
    /// <param name="expected">The expected absolute URI string result after conversion.</param>
    [TestCase("/foo/bar", "http://www.domain.com", "http://www.domain.com/foo/bar")]
    [TestCase("/foo/bar", "http://www.domain.com/dang/dang", "http://www.domain.com/foo/bar")]
    [TestCase("/", "http://www.domain.com/dang/dang", "http://www.domain.com/")]
    [TestCase("/foo/bar", "http://www.domain.com/dang/dang?q=3#dang", "http://www.domain.com/foo/bar")]
    [TestCase("/foo/bar?k=6#yop", "http://www.domain.com/dang/dang?q=3#dang", "http://www.domain.com/foo/bar?k=6")]
    public void MakeAbsolute(string input, string reference, string expected)
    {
        var source = new Uri(input, UriKind.Relative);
        var absolute = new Uri(reference);
        var output = source.MakeAbsolute(absolute);
        Assert.AreEqual(expected, output.ToString());
    }

    /// <summary>
    /// Tests the <c>GetFileExtension</c> method to ensure it correctly extracts the file extension from a given URI string.
    /// This includes handling URIs with or without file extensions, query parameters, and different casing.
    /// </summary>
    /// <param name="input">The input URI string from which to extract the file extension.</param>
    /// <param name="expected">The expected file extension result (in lowercase, or empty if none).</param>
    [TestCase("https://example.com/media/image.jpg", "jpg")]
    [TestCase("/media/image.png", "png")]
    [TestCase("/media/doc.pdf?v=123", "pdf")]
    [TestCase("/media/someimage", "")]
    [TestCase("/media/image.JPG", "jpg")]
    public void GetFileExtension(string input, string expected)
    {
        var source = new Uri(input, UriKind.RelativeOrAbsolute);
        var output = source.GetFileExtension();
        Assert.AreEqual(expected, output);
    }

    /// <summary>
    /// Tests the WithoutPort extension method to ensure it removes the port from the URI correctly.
    /// </summary>
    /// <param name="input">The input URI string to test.</param>
    /// <param name="expected">The expected URI string result after removing the port.</param>
    [TestCase("http://www.domain.com/path/to/page", "http://www.domain.com/path/to/page")]
    [TestCase("http://www.domain.com/path/to/page/", "http://www.domain.com/path/to/page/")]
    [TestCase("http://www.domain.com", "http://www.domain.com/")]
    [TestCase("http://www.domain.com/", "http://www.domain.com/")]
    [TestCase("http://www.domain.com/path/to?q=3#yop", "http://www.domain.com/path/to?q=3#yop")]
    [TestCase("http://www.domain.com/path/to/?q=3#yop", "http://www.domain.com/path/to/?q=3#yop")]
    [TestCase("http://www.domain.com:666/path/to/page", "http://www.domain.com/path/to/page")]
    [TestCase("http://www.domain.com:666/path/to/page/", "http://www.domain.com/path/to/page/")]
    [TestCase("http://www.domain.com:666", "http://www.domain.com/")]
    [TestCase("http://www.domain.com:666/", "http://www.domain.com/")]
    [TestCase("http://www.domain.com:666/path/to?q=3#yop", "http://www.domain.com/path/to?q=3#yop")]
    [TestCase("http://www.domain.com:666/path/to/?q=3#yop", "http://www.domain.com/path/to/?q=3#yop")]
    public void WithoutPort(string input, string expected)
    {
        var source = new Uri(input);
        var output = source.WithoutPort();
        Assert.AreEqual(expected, output.ToString());
    }
}
