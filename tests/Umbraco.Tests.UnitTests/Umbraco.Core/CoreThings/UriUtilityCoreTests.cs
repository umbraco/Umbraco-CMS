using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.CoreThings;

[TestFixture]
public class UriUtilityCoreTests
{
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
