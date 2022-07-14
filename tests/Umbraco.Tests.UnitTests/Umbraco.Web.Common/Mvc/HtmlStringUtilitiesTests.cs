// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Web.Common.Mvc;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Mvc;

[TestFixture]
public class HtmlStringUtilitiesTests
{
    [SetUp]
    public virtual void Initialize() => _htmlStringUtilities = new HtmlStringUtilities();

    private HtmlStringUtilities _htmlStringUtilities;

    [Test]
    public void TruncateWithElipsis()
    {
        var output = _htmlStringUtilities.Truncate("hello world", 5, true, false).ToString();
        var expected = "hello&hellip;";
        Assert.AreEqual(expected, output);
    }

    [Test]
    public void TruncateWithoutElipsis()
    {
        var output = _htmlStringUtilities.Truncate("hello world", 5, false, false).ToString();
        var expected = "hello";
        Assert.AreEqual(expected, output);
    }

    [Test]
    public void TruncateShorterWordThanHellip()
    {
        // http://issues.umbraco.org/issue/U4-10478
        var output = _htmlStringUtilities.Truncate("hi", 5, true, false).ToString();
        var expected = "hi";
        Assert.AreEqual(expected, output);
    }

    [Test]
    public void TruncateAndRemoveSpaceBetweenHellipAndWord()
    {
        var output = _htmlStringUtilities.Truncate("hello world", 6 /* hello plus space */, true, false).ToString();
        var expected = "hello&hellip;";
        Assert.AreEqual(expected, output);
    }
}
