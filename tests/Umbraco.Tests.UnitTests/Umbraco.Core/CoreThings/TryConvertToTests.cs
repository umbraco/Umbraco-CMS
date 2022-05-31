// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Globalization;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.CoreThings;

[TestFixture]
public class TryConvertToTests
{
    [Test]
    public void ConvertToBoolTest()
    {
        var conv = 1.TryConvertTo<bool>();
        Assert.IsTrue(conv);
        Assert.AreEqual(true, conv.Result);

        conv = "1".TryConvertTo<bool>();
        Assert.IsTrue(conv);
        Assert.AreEqual(true, conv.Result);

        conv = 0.TryConvertTo<bool>();
        Assert.IsTrue(conv);
        Assert.AreEqual(false, conv.Result);

        conv = "0".TryConvertTo<bool>();
        Assert.IsTrue(conv);
        Assert.AreEqual(false, conv.Result);

        conv = "Yes".TryConvertTo<bool>();
        Assert.IsTrue(conv);
        Assert.AreEqual(true, conv.Result);

        conv = "yes".TryConvertTo<bool>();
        Assert.IsTrue(conv);
        Assert.AreEqual(true, conv.Result);

        conv = "No".TryConvertTo<bool>();
        Assert.IsTrue(conv);
        Assert.AreEqual(false, conv.Result);

        conv = "no".TryConvertTo<bool>();
        Assert.IsTrue(conv);
        Assert.AreEqual(false, conv.Result);
    }

    [Test]
    [TestCase("en-US", -1, ExpectedResult = -1)]
    [TestCase("en-US", "-1", ExpectedResult = -1)]
    [TestCase("en-US", "100", ExpectedResult = 100)]
    [TestCase("en-US", "100.000", ExpectedResult = 100)]
    [TestCase("en-US", "100,000", ExpectedResult = 100)]
    [TestCase("en-US", "100.001", ExpectedResult = 100)]
    [TestCase("en-US", 100, ExpectedResult = 100)]
    [TestCase("en-US", 100.000, ExpectedResult = 100)]
    [TestCase("en-US", 100.001, ExpectedResult = 100)]
    [TestCase("sv-SE", -1, ExpectedResult = -1)]
    [TestCase("sv-SE", "−1", ExpectedResult = -1)] // Note '−' vs '-'
    [TestCase("sv-SE", "100", ExpectedResult = 100)]
    [TestCase("sv-SE", "100.000", ExpectedResult = 100)]
    [TestCase("sv-SE", "100,000", ExpectedResult = 100)]
    [TestCase("sv-SE", "100.001", ExpectedResult = 100)]
    [TestCase("sv-SE", 100, ExpectedResult = 100)]
    [TestCase("sv-SE", 100.000, ExpectedResult = 100)]
    [TestCase("sv-SE", 100.001, ExpectedResult = 100)]
    [TestCase("da-DK", "-1", ExpectedResult = -1)]
    [TestCase("da-DK", -1, ExpectedResult = -1)]
    [TestCase("da-DK", "100", ExpectedResult = 100)]
    [TestCase("da-DK", "100.000", ExpectedResult = 100)]
    [TestCase("da-DK", "100,000", ExpectedResult = 100)]
    [TestCase("da-DK", "100.001", ExpectedResult = 100)]
    [TestCase("da-DK", 100, ExpectedResult = 100)]
    [TestCase("da-DK", 100.000, ExpectedResult = 100)]
    [TestCase("da-DK", 100.001, ExpectedResult = 100)]
    public int ConvertToIntegerTest(string culture, object input)
    {
        if (culture is not null)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        }

        var conv = input.TryConvertTo<int>();
        Assert.IsTrue(conv);

        return conv.Result;
    }

    [Test]
    public void ConvertToDecimalTest()
    {
        var conv = "100".TryConvertTo<decimal>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100m, conv.Result);

        conv = "100.000".TryConvertTo<decimal>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100m, conv.Result);

        conv = "100,000".TryConvertTo<decimal>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100m, conv.Result);

        conv = "100.001".TryConvertTo<decimal>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100.001m, conv.Result);

        conv = 100m.TryConvertTo<decimal>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100m, conv.Result);

        conv = 100.000m.TryConvertTo<decimal>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100m, conv.Result);

        conv = 100.001m.TryConvertTo<decimal>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100.001m, conv.Result);

        conv = 100.TryConvertTo<decimal>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100m, conv.Result);
    }

    [Test]
    public void ConvertToDateTimeTest()
    {
        var conv = "2016-06-07".TryConvertTo<DateTime>();
        Assert.IsTrue(conv);
        Assert.AreEqual(new DateTime(2016, 6, 7), conv.Result);
    }
}
