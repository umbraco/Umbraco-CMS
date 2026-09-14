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

[TestFixture]
public class ObjectExtensionsTests
{
    [SetUp]
    public void TestSetup()
    {
        _savedCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture =
            CultureInfo.GetCultureInfo("en-GB"); // make sure the dates parse correctly
    }

    [TearDown]
    public void TestTearDown() => Thread.CurrentThread.CurrentCulture = _savedCulture;

    private CultureInfo _savedCulture;

    [Test]
    public void Can_Create_Enumerable_Of_One()
    {
        var input = "hello";
#pragma warning disable CS0618 // Type or member is obsolete
        var result = input.AsEnumerableOfOne<string>();
#pragma warning restore CS0618 // Type or member is obsolete
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First(), Is.EqualTo("hello"));
    }

    [Test]
    public void Can_Convert_List_To_Enumerable()
    {
        var list = new List<string> { "hello", "world", "awesome" };
        var result = list.TryConvertTo<IEnumerable<string>>();
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result.Count(), Is.EqualTo(3));
    }

    [Test]
    public void CanConvertIntToNullableInt()
    {
        var i = 1;
        var result = i.TryConvertTo<int>();
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public void CanConvertNullableIntToInt()
    {
        int? i = 1;
        var result = i.TryConvertTo<int>();
        Assert.That(result.Success, Is.True);
    }

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

            Assert.That(result.Success, Is.True, testCase.Key);
            Assert.That(result.Result, Is.EqualTo(testCase.Value), testCase.Key);
        }
    }

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

        Assert.That(result.Success, Is.True, date);
        Assert.That(outcome, Is.EqualTo(DateTime.Equals(dateTime.Date, result.Result.Date)), date);
    }

    [Test]
    public virtual void CanConvertBlankStringToNullDateTime()
    {
        var result = string.Empty.TryConvertTo<DateTime?>();
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Null);
    }

    [Test]
    public virtual void CanConvertBlankStringToNullBool()
    {
        var result = string.Empty.TryConvertTo<bool?>();
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Null);
    }

    [Test]
    public virtual void CanConvertBlankStringToDateTime()
    {
        var result = string.Empty.TryConvertTo<DateTime>();
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(DateTime.MinValue));
    }

    [Test]
    public virtual void CanConvertObjectToString_Using_ToString_Overload()
    {
        var result = new MyTestObject().TryConvertTo<string>();

        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo("Hello world"));
    }

    [Test]
    public virtual void CanConvertObjectToSameObject()
    {
        var obj = new MyTestObject();
        var result = obj.TryConvertTo<object>();

        Assert.That(result.Result, Is.EqualTo(obj));
    }

    [Test]
    public void ConvertToIntegerTest()
    {
        var conv = "100".TryConvertTo<int>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100));

        conv = "100.000".TryConvertTo<int>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100));

        conv = "100,000".TryConvertTo<int>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100));

        // oops
        conv = "100.001".TryConvertTo<int>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100));

        conv = 100m.TryConvertTo<int>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100));

        conv = 100.000m.TryConvertTo<int>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100));

        // oops
        conv = 100.001m.TryConvertTo<int>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100));
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
    public void ConvertToNullableDecimalTest()
    {
        var conv = "100".TryConvertTo<decimal?>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100m));

        conv = "100.000".TryConvertTo<decimal?>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100m));

        conv = "100,000".TryConvertTo<decimal?>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100m));

        conv = "100.001".TryConvertTo<decimal?>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100.001m));

        conv = 100m.TryConvertTo<decimal?>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100m));

        conv = 100.000m.TryConvertTo<decimal?>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100m));

        conv = 100.001m.TryConvertTo<decimal?>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(100.001m));

        conv = 100.TryConvertTo<decimal?>();
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

    [Test]
    public void ConvertToNullableDateTimeTest()
    {
        var conv = "2016-06-07".TryConvertTo<DateTime?>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(new DateTime(2016, 6, 7)));
    }

    [TestCase("d72f12a9-29db-42b4-9ffb-25a3ba4dcef5")]
    [TestCase("D72F12A9-29DB-42B4-9FFB-25A3BA4DCEF5")]
    public void CanConvertToGuid(string guidValue)
    {
        var conv = guidValue.TryConvertTo<Guid>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(Guid.Parse(guidValue)));
    }

    [TestCase("d72f12a9-29db-42b4-9ffb-25a3ba4dcef5")]
    [TestCase("D72F12A9-29DB-42B4-9FFB-25A3BA4DCEF5")]
    public void CanConvertToNullableGuid(string guidValue)
    {
        var conv = guidValue.TryConvertTo<Guid?>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(Guid.Parse(guidValue)));
    }

    [TestCase("d72f12a9-29db-42b4-9ffb-25a3ba4dcef5")]
    [TestCase("D72F12A9-29DB-42B4-9FFB-25A3BA4DCEF5")]
    public void CanConvertStringValuesToNullableGuid(string guidValue)
    {
        StringValues stringValues = guidValue;
        var conv = stringValues.TryConvertTo<Guid?>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(Guid.Parse(guidValue)));
    }

    [TestCase(10)]
    [TestCase(0)]
    [TestCase(-10)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void CanConvertStringValuesToInt(int intValue)
    {
        StringValues stringValues = intValue.ToString();
        var conv = stringValues.TryConvertTo<int>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo(intValue));
    }

    [Test]
    public void CanConvertStringValuesToString()
    {
        StringValues stringValues = "This is a string";
        var conv = stringValues.TryConvertTo<string>();
        Assert.That((bool)conv, Is.True);
        Assert.That(conv.Result, Is.EqualTo("This is a string"));
    }

    [Test]
    public void CanConvertDateTimeOffsetToDateTime()
    {
        var dateTimeOffset = new DateTimeOffset(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03), TimeSpan.Zero);
        var result = dateTimeOffset.TryConvertTo<DateTime>();
        Assert.That(result.Success, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.EqualTo(new DateTime(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03))));
            Assert.That(result.Result.Kind, Is.EqualTo(DateTimeKind.Utc));
        });
    }

    [Test]
    public void CanConvertDateTimeToDateTimeOffset()
    {
        var dateTime = new DateTime(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03), DateTimeKind.Utc);
        var result = dateTime.TryConvertTo<DateTimeOffset>();
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(new DateTimeOffset(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03), TimeSpan.Zero)));
    }

    [Test]
    public void DiscardsOffsetWhenConvertingDateTimeOffsetToDateTime()
    {
        var dateTimeOffset = new DateTimeOffset(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03), TimeSpan.FromHours(2));
        var result = dateTimeOffset.TryConvertTo<DateTime>();
        Assert.That(result.Success, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.EqualTo(new DateTime(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03))));
            Assert.That(result.Result.Kind, Is.EqualTo(DateTimeKind.Utc));
        });
    }

    [Test]
    public void DiscardsDateTimeKindWhenConvertingDateTimeToDateTimeOffset()
    {
        var dateTime = new DateTime(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03), DateTimeKind.Local);
        var result = dateTime.TryConvertTo<DateTimeOffset>();
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(new DateTimeOffset(new DateOnly(2024, 07, 05), new TimeOnly(12, 30, 01, 02, 03), TimeSpan.Zero)));
    }

    [Test]
    public void Value_Editor_Can_Convert_Decimal_To_Decimal_Clr_Type()
    {
        var valueEditor = MockedValueEditors.CreateDataValueEditor(ValueTypes.Decimal);

        var result = valueEditor.TryConvertValueToCrlType(12.34d);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(12.34d));
    }

    [Test]
    public void Value_Editor_Can_Convert_DateTimeOffset_To_DateTime_Clr_Type()
    {
        var valueEditor = MockedValueEditors.CreateDataValueEditor(ValueTypes.Date);

        var result = valueEditor.TryConvertValueToCrlType(new DateTimeOffset(new DateOnly(2024, 07, 05), new TimeOnly(12, 30), TimeSpan.Zero));
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result is DateTime, Is.True);

        var dateTime = (DateTime)result.Result;
        Assert.Multiple(() =>
        {
            Assert.That(dateTime, Is.EqualTo(new DateTime(new DateOnly(2024, 07, 05), new TimeOnly(12, 30))));
            Assert.That(dateTime.Kind, Is.EqualTo(DateTimeKind.Utc));
        });
    }

    private class MyTestObject
    {
        public override string ToString() => "Hello world";
    }
}
