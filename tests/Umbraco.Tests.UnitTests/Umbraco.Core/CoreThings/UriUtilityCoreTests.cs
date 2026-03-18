using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.CoreThings;

/// <summary>
/// Contains unit tests that verify the functionality of the <see cref="UriUtilityCore"/> class.
/// </summary>
[TestFixture]
public class UriUtilityCoreTests
{
    /// <summary>
    /// Tests the TrimPathEndSlash method to ensure it correctly trims the trailing slash from the given URI.
    /// </summary>
    /// <param name="uri">The input URI string to trim.</param>
    /// <param name="expected">The expected URI string after trimming the trailing slash.</param>
    [TestCase("/en", "/en")]
    [TestCase("/en#anchor", "/en#anchor")]
    [TestCase("/en/", "/en")]
    [TestCase("/en/#anchor", "/en#anchor")]
    [TestCase("/en/?abc=123", "/en?abc=123")]
    [TestCase("/en/#abc?abc=123", "/en#abc?abc=123")]
    public void TrimPathEndSlash(string uri, string expected)
    {
        var result = UriUtilityCore.TrimPathEndSlash(uri);
        Assert.AreEqual(expected, result);
    }


    /// <summary>
    /// Verifies that <see cref="UriUtilityCore.EndPathWithSlash"/> correctly appends a trailing slash to the path component of a URI string when needed, preserving any query strings or anchors.
    /// </summary>
    /// <param name="uri">The input URI string to process.</param>
    /// <param name="expected">The expected URI string after ensuring the path ends with a slash, with query and anchor components preserved.</param>
    [TestCase("/en/", "/en/")]
    [TestCase("/en#anchor", "/en/#anchor")]
    [TestCase("/en", "/en/")]
    [TestCase("/en/#anchor", "/en/#anchor")]
    [TestCase("/en?abc=123", "/en/?abc=123")]
    [TestCase("/en#abc?abc=123", "/en/#abc?abc=123")]
    public void EndPathWithSlash(string uri, string expected)
    {
        var result = UriUtilityCore.EndPathWithSlash(uri);
        Assert.AreEqual(expected, result);
    }
}
