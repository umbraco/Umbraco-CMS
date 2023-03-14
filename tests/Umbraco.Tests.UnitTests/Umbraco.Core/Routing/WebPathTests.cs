// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class WebPathTests
{
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

    [Test]
    public void Combine_must_handle_empty_array() =>
        Assert.AreEqual(string.Empty, WebPath.Combine(Array.Empty<string>()));

    [Test]
    public void Combine_must_handle_null() => Assert.Throws<ArgumentNullException>(() => WebPath.Combine(null));
}
