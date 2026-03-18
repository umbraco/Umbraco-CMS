// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Extensions;
using DateTimeOffset = System.DateTimeOffset;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.CoreThings;

/// <summary>
/// Contains unit tests for the extension methods in the <see cref="ObjectExtensions"/> class.
/// </summary>
[TestFixture]
public class ObjectExtensionsTests
{
    /// <summary>
    /// Sets up the test environment by configuring the current thread culture to en-GB.
    /// </summary>
    [SetUp]
    public void TestSetup()
    {
        _savedCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture =
            CultureInfo.GetCultureInfo("en-GB"); // make sure the dates parse correctly
    }

    /// <summary>
    /// Resets the current thread's culture to the saved culture after each test.
    /// </summary>
    [TearDown]
    public void TestTearDown() => Thread.CurrentThread.CurrentCulture = _savedCulture;

    private CultureInfo _savedCulture;

    /// <summary>
    /// Tests that an enumerable of one element can be created from a single object.
    /// </summary>
    [Test]
    public void Can_Create_Enumerable_Of_One()
    {
        var input = "hello";
#pragma warning disable CS0618 // Type or member is obsolete
        var result = input.AsEnumerableOfOne<string>();
#pragma warning restore CS0618 // Type or member is obsolete
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("hello", result.First());
    }

    /// <summary>
    /// Tests that a List can be successfully converted to an IEnumerable using TryConvertTo.
    /// </summary>
    [Test]
    public void Can_Convert_List_To_Enumerable()
    {
        var list = new List<string> { "hello", "world", "awesome" };
        var result = list.TryConvertTo<IEnumerable<string>>();
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.Result.Count());
    }

    /// <summary>
    /// Tests that an int can be converted to a nullable int using TryConvertTo.
    /// </summary>
    [Test]
    public void CanConvertIntToNullableInt()
    {
        var i = 1;
        var result = i.TryConvertTo<int>();
        Assert.That(result.Success, Is.True);
    }

    /// <summary>
    /// Tests that a nullable int can be successfully converted to an int.
    /// </summary>
    [Test]
    public void CanConvertNullableIntToInt()
    {
        int? i = 1;
        var result = i.TryConvertTo<int>();
        Assert.That(result.Success, Is.True);
    }

    /// <summary>
    /// Verifies that various string representations of boolean values (such as "true", "false", "1", "0", and different casings) are correctly converted to their corresponding <see cref="bool"/> values using <c>TryConvertTo&lt;bool&gt;</c>.
    /// </summary>
    [Test]
    public virtual void CanConvertStringToBool()
    {
        var testCases = new Dictionary<string, bool>
        {
            { "TRUE", true },
            { "True", true },
            { "true", true },
            { "1", true },
            { "FALSE", false },
            { "False", false },
            { "false", false },
            { "0", false },
            { string.Empty, false },
        };

        foreach (var testCase in testCases)
        {
            var result = testCase.Key.TryConvertTo<bool>();

            Assert.IsTrue(result.Success, testCase.Key);
            Assert.AreEqual(testCase.Value, result.Result, testCase.Key);
        }
    }

    /// <summary>
    /// Verifies whether a string can be successfully converted to a <see cref="DateTime"/> value using the TryConvertTo extension method, and asserts that the result matches the expected outcome.
    /// </summary>
    /// <param name="date">The string representation of the date to attempt to convert.</param>
    /// <param name="outcome">True if the conversion is expected to succeed and match the reference date; otherwise, false.</param>
    [TestCase("2012-11-10", true)]
    [TestCase("2012/11/10", true)]
    [TestCase("10/11/2012", true)] // assuming your culture uses DD/MM/YYYY
    [TestCase("11/10/2012", false)] // assuming your culture uses DD/MM/YYYY
    [TestCase("Sat 10, Nov 2012", true)]
    [TestCase("Saturday 10, Nov 2012", true)]
    [TestCase("Sat 10, November 2012", true)]
    [TestCase("Saturday 10, November 2012", true)]
    [TestCase("2012-11-10 13:14:15", true)]
    [TestCase("2012-11-10T13:14:15Z", true)]
    public virtual void CanConvertStringToDateTime(string date, bool outcome)
    {
        var dateTime = new DateTime(2012, 11, 10, 13, 14, 15);

        var result = date.TryConvertTo<DateTime>();

        Assert.IsTrue(result.Success, date);
        Assert.AreEqual(DateTime.Equals(dateTime.Date, result.Result.Date), outcome, date);
    }

    /// <summary>
    /// Tests that an empty string can be converted to a null nullable DateTime.
    /// </summary>
    [Test]
    public virtual void CanConvertBlankStringToNullDateTime()
    {
        var result = string.Empty.TryConvertTo<DateTime?>();
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Result);
    }

    /// <summary>
    /// Tests that an empty string can be converted to a nullable boolean with a null result.
    /// </summary>
    [Test]
    public virtual void CanConvertBlankStringToNullBool()
    {
        var result = string.Empty.TryConvertTo<bool?>();
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Result);
    }

    /// <summary>
    /// Verifies that attempting to convert an empty string to a <see cref="DateTime"/> using <c>TryConvertTo&lt;DateTime&gt;</c> succeeds and returns <see cref="DateTime.MinValue"/>.
    /// </summary>
    [Test]
    public virtual void CanConvertBlankStringToDateTime()
    {
        var result = string.Empty.TryConvertTo<DateTime>();
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DateTime.MinValue, result.Result);
    }

    /// <summary>
    /// Tests that an object can be converted to a string using the ToString overload.
    /// </summary>
    [Test]
    public virtual void CanConvertObjectToString_Using_ToString_Overload()
    {
        var result = new MyTestObject().TryConvertTo<string>();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Hello world", result.Result);
    }

    /// <summary>
    /// Tests that an object can be converted to the same object type successfully.
    /// </summary>
    [Test]
    public virtual void CanConvertObjectToSameObject()
    {
        var obj = new MyTestObject();
        var result = obj.TryConvertTo<object>();

        Assert.AreEqual(obj, result.Result);
    }

    /// <summary>
    /// Tests the <c>TryConvertTo</c> method for converting various string and decimal representations
    /// of numbers to integers, including cases with decimal points and thousands separators.
    /// Verifies that the conversion truncates decimals and handles different formats correctly.
    /// </summary>
    [Test]
    public void ConvertToIntegerTest()
    {
        var conv = "100".TryConvertTo<int>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100, conv.Result);

        conv = "100.000".TryConvertTo<int>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100, conv.Result);

        conv = "100,000".TryConvertTo<int>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100, conv.Result);

        // oops
        conv = "100.001".TryConvertTo<int>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100, conv.Result);

        conv = 100m.TryConvertTo<int>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100, conv.Result);

        conv = 100.000m.TryConvertTo<int>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100, conv.Result);

        // oops
        conv = 100.001m.TryConvertTo<int>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100, conv.Result);
    }

    /// <summary>
    /// Tests the conversion of various objects to decimal using the TryConvertTo method.
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
    /// Tests the TryConvertTo method for nullable decimal conversions with various input formats.
    /// </summary>
    [Test]
    public void ConvertToNullableDecimalTest()
    {
        var conv = "100".TryConvertTo<decimal?>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100m, conv.Result);

        conv = "100.000".TryConvertTo<decimal?>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100m, conv.Result);

        conv = "100,000".TryConvertTo<decimal?>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100m, conv.Result);

        conv = "100.001".TryConvertTo<decimal?>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100.001m, conv.Result);

        conv = 100m.TryConvertTo<decimal?>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100m, conv.Result);

        conv = 100.000m.TryConvertTo<decimal?>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100m, conv.Result);

        conv = 100.001m.TryConvertTo<decimal?>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100.001m, conv.Result);

        conv = 100.TryConvertTo<decimal?>();
        Assert.IsTrue(conv);
        Assert.AreEqual(100m, conv.Result);
    }

    /// <summary>
    /// Verifies that the <c>TryConvertTo</c> extension method correctly converts a valid date string to a <see cref="DateTime"/> object.
    /// Ensures the conversion result is successful and matches the expected <see cref="DateTime"/> value.
    /// </summary>
    [Test]
    public void ConvertToDateTimeTest()
    {
        var conv = "2016-06-07".TryConvertTo<DateTime>();
        Assert.IsTrue(conv);
        Assert.AreEqual(new DateTime(2016, 6, 7), conv.Result);
    }

    /// <summary>
    /// Tests the TryConvertTo method for converting a string to a nullable DateTime.
    /// </summary>
    [Test]
    public void ConvertToNullableDateTimeTest()
    {
        var conv = "2016-06-07".TryConvertTo<DateTime?>();
        Assert.IsTrue(conv);
        Assert.AreEqual(new DateTime(2016, 6, 7), conv.Result);
    }

    /// <summary>
    /// Verifies that a string representation of a GUID can be successfully converted to a <see cref="Guid"/> using the TryConvertTo method.
    /// </summary>
    /// <param name="guidValue">The string representation of the GUID to test conversion for.</param>
    /// <remarks>
    /// This test asserts that the conversion result is successful and matches the expected <see cref="Guid"/> value.
    /// </remarks>
    [TestCase("d72f12a9-29db-42b4-9ffb-25a3ba4dcef5")]
    [TestCase("D72F12A9-29DB-42B4-9FFB-25A3BA4DCEF5")]
    public void CanConvertToGuid(string guidValue)
    {
        var conv = guidValue.TryConvertTo<Guid>();
        Assert.IsTrue(conv);
        Assert.AreEqual(Guid.Parse(guidValue), conv.Result);
    }

    /// <summary>
    /// Verifies that a string representation of a GUID can be successfully converted to a nullable <see cref="Guid"/>.
    /// </summary>
    /// <param name="guidValue">The string representation of the GUID to convert.</param>
    /// <remarks>
    /// The test asserts that the conversion result is successful and matches the expected <see cref="Guid"/> value.
    /// </remarks>
    [TestCase("d72f12a9-29db-42b4-9ffb-25a3ba4dcef5")]
    [TestCase("D72F12A9-29DB-42B4-9FFB-25A3BA4DCEF5")]
    public void CanConvertToNullableGuid(string guidValue)
    {
        var conv = guidValue.TryConvertTo<Guid?>();
        Assert.IsTrue(conv);
        Assert.AreEqual(Guid.Parse(guidValue), conv.Result);
    }

    /// <summary>
    /// Tests that string values can be converted to nullable Guid values.
    /// </summary>
    /// <param name="guidValue">The string representation of the Guid to convert.</param>
    [TestCase("d72f12a9-29db-42b4-9ffb-25a3ba4dcef5")]
    [TestCase("D72F12A9-29DB-42B4-9FFB-25A3BA4DCEF5")]
    public void CanConvertStringValuesToNullableGuid(string guidValue)
    {
        StringValues stringValues = guidValue;
        var conv = stringValues.TryConvertTo<Guid?>();
        Assert.IsTrue(conv);
        Assert.AreEqual(Guid.Parse(guidValue), conv.Result);
    }

    /// <summary>
    /// Verifies that string representations of integer values can be correctly converted back to integers.
    /// </summary>
    /// <param name="intValue">The integer value to be converted to a string and then parsed back to an integer.</param>
    [TestCase(10)]
    [TestCase(0)]
    [TestCase(-10)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void CanConvertStringValuesToInt(int intValue)
    {
        StringValues stringValues = intValue.ToString();
        var conv = stringValues.TryConvertTo<int>();
        Assert.IsTrue(conv);
        Assert.AreEqual(intValue, conv.Result);
    }

    /// <summary>
    /// Tests that StringValues can be converted to string using TryConvertTo.
    /// </summary>
    [Test]
    public void CanConvertStringValuesToString()
    {
        StringValues stringValues = "This is a string";
        var conv = stringValues.TryConvertTo<string>();
        Assert.IsTrue(conv);
        Assert.AreEqual("This is a string", conv.Result);
    }

    /// <summary>
    /// Verifies that a <see cref="DateTimeOffset"/> instance can be converted to a <see cref="DateTime"/> using the extension method,
    /// and that the resulting <see cref="DateTime"/> has the expected value and <see cref="DateTimeKind"/>.
    /// </summary>
    [Test]
    public void CanConvertDateTimeOffsetToDateTime()
    {
        var dateTimeOffset = new DateTimeOffset(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03), TimeSpan.Zero);
        var result = dateTimeOffset.TryConvertTo<DateTime>();
        Assert.IsTrue(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(new DateTime(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03)), result.Result);
            Assert.AreEqual(DateTimeKind.Utc, result.Result.Kind);
        });
    }

    /// <summary>
    /// Tests that a DateTime instance can be successfully converted to a DateTimeOffset.
    /// </summary>
    [Test]
    public void CanConvertDateTimeToDateTimeOffset()
    {
        var dateTime = new DateTime(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03), DateTimeKind.Utc);
        var result = dateTime.TryConvertTo<DateTimeOffset>();
        Assert.IsTrue(result.Success);
        Assert.AreEqual(new DateTimeOffset(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03), TimeSpan.Zero), result.Result);
    }

    /// <summary>
    /// Tests that converting a DateTimeOffset to DateTime discards the offset and preserves the date and time components.
    /// </summary>
    [Test]
    public void DiscardsOffsetWhenConvertingDateTimeOffsetToDateTime()
    {
        var dateTimeOffset = new DateTimeOffset(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03), TimeSpan.FromHours(2));
        var result = dateTimeOffset.TryConvertTo<DateTime>();
        Assert.IsTrue(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(new DateTime(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03)), result.Result);
            Assert.AreEqual(DateTimeKind.Utc, result.Result.Kind);
        });
    }

    /// <summary>
    /// Tests that the DateTimeKind is discarded when converting a DateTime to a DateTimeOffset.
    /// </summary>
    [Test]
    public void DiscardsDateTimeKindWhenConvertingDateTimeToDateTimeOffset()
    {
        var dateTime = new DateTime(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03), DateTimeKind.Local);
        var result = dateTime.TryConvertTo<DateTimeOffset>();
        Assert.IsTrue(result.Success);
        Assert.AreEqual(new DateTimeOffset(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03), TimeSpan.Zero), result.Result);
    }

    /// <summary>
    /// Tests that the value editor can convert a decimal value to the decimal CLR type.
    /// </summary>
    [Test]
    public void Value_Editor_Can_Convert_Decimal_To_Decimal_Clr_Type()
    {
        var valueEditor = MockedValueEditors.CreateDataValueEditor(ValueTypes.Decimal);

        var result = valueEditor.TryConvertValueToCrlType(12.34d);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(12.34d, result.Result);
    }

    /// <summary>
    /// Tests that the value editor can convert a DateTimeOffset value to a DateTime CLR type correctly.
    /// </summary>
    [Test]
    public void Value_Editor_Can_Convert_DateTimeOffset_To_DateTime_Clr_Type()
    {
        var valueEditor = MockedValueEditors.CreateDataValueEditor(ValueTypes.Date);

        var result = valueEditor.TryConvertValueToCrlType(new DateTimeOffset(new DateOnly(2024, 07, 05), new TimeOnly(12, 30), TimeSpan.Zero));
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Result is DateTime);

        var dateTime = (DateTime)result.Result;
        Assert.Multiple(() =>
        {
            Assert.AreEqual(new DateTime(new DateOnly(2024, 07, 05), new TimeOnly(12, 30)), dateTime);
            Assert.AreEqual(DateTimeKind.Utc, dateTime.Kind);
        });
    }

    private class MyTestObject
    {
    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
        public override string ToString() => "Hello world";
    }
}
