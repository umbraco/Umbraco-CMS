// Copyright (c) Umbraco.
// See LICENSE for more details.

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
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(true));

        conv = "1".TryConvertTo<bool>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(true));

        conv = 0.TryConvertTo<bool>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(false));

        conv = "0".TryConvertTo<bool>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(false));

        conv = "Yes".TryConvertTo<bool>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(true));

        conv = "yes".TryConvertTo<bool>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(true));

        conv = "No".TryConvertTo<bool>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(false));

        conv = "no".TryConvertTo<bool>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(false));
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
        Assert.That((bool)conv, Is.True);

        return conv.Result;
    }

    [Test]
    public void ConvertToDecimalTest()
    {
        var conv = "100".TryConvertTo<decimal>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100m));

        conv = "100.000".TryConvertTo<decimal>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100m));

        conv = "100,000".TryConvertTo<decimal>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100m));

        conv = "100.001".TryConvertTo<decimal>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100.001m));

        conv = 100m.TryConvertTo<decimal>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100m));

        conv = 100.000m.TryConvertTo<decimal>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100m));

        conv = 100.001m.TryConvertTo<decimal>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100.001m));

        conv = 100.TryConvertTo<decimal>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100m));
    }

    [Test]
    public void ConvertToDateTimeTest()
    {
        var conv = "2016-06-07".TryConvertTo<DateTime>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(new DateTime(2016, 6, 7)));
    }
}
