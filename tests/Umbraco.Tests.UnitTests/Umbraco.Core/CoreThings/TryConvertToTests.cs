// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.CoreThings;

/// <summary>
/// Unit tests for the TryConvertTo functionality in Umbraco.Core.
/// </summary>
[TestFixture]
public class TryConvertToTests
{
    /// <summary>
    /// Tests the <c>TryConvertTo&lt;bool&gt;</c> extension method to ensure it correctly converts various numeric and string representations (such as 1, 0, "1", "0", "Yes", "No", etc.) to their corresponding boolean values.
    /// Verifies both successful conversion and the expected boolean result for each input.
    /// </summary>
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

    /// <summary>
    /// Tests conversion of various input values to an integer using the specified culture.
    /// The test asserts that the conversion succeeds and returns the expected integer value.
    /// </summary>
    /// <param name="culture">The culture name (e.g., "en-US", "sv-SE", "da-DK") to set as <see cref="CultureInfo.CurrentCulture"/> for the conversion.</param>
    /// <param name="input">The input value to convert to an integer. Can be a string or numeric type.</param>
    /// <returns>The integer result of the conversion, as asserted by the test case's <c>ExpectedResult</c>.</returns>
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

    /// <summary>
    /// Tests the TryConvertTo method for converting various inputs to decimal values.
    /// </summary>
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

    /// <summary>
    /// Tests the TryConvertTo extension method for converting a string to a DateTime.
    /// </summary>
    [Test]
    public void ConvertToDateTimeTest()
    {
        var conv = "2016-06-07".TryConvertTo<DateTime>();
        Assert.IsTrue(conv);
        Assert.AreEqual(new DateTime(2016, 6, 7), conv.Result);
    }
}
