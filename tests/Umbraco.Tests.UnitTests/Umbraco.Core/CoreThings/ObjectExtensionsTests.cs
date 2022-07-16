// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Extensions;

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
    public void Can_Convert_List_To_Enumerable()
    {
        var list = new List<string> { "hello", "world", "awesome" };
        var result = list.TryConvertTo<IEnumerable<string>>();
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.Result.Count());
    }

    [Test]
    public void ObjectExtensions_Object_To_Dictionary()
    {
        // Arrange
        var obj = new { Key1 = "value1", Key2 = "value2", Key3 = "value3" };

        // Act
        var d = obj.ToDictionary<string>();

        // Assert
        Assert.IsTrue(d.Keys.Contains("Key1"));
        Assert.IsTrue(d.Keys.Contains("Key2"));
        Assert.IsTrue(d.Keys.Contains("Key3"));
        Assert.AreEqual(d["Key1"], "value1");
        Assert.AreEqual(d["Key2"], "value2");
        Assert.AreEqual(d["Key3"], "value3");
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

            Assert.IsTrue(result.Success, testCase.Key);
            Assert.AreEqual(testCase.Value, result.Result, testCase.Key);
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

        Assert.IsTrue(result.Success, date);
        Assert.AreEqual(DateTime.Equals(dateTime.Date, result.Result.Date), outcome, date);
    }

    [Test]
    public virtual void CanConvertBlankStringToNullDateTime()
    {
        var result = string.Empty.TryConvertTo<DateTime?>();
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Result);
    }

    [Test]
    public virtual void CanConvertBlankStringToNullBool()
    {
        var result = string.Empty.TryConvertTo<bool?>();
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Result);
    }

    [Test]
    public virtual void CanConvertBlankStringToDateTime()
    {
        var result = string.Empty.TryConvertTo<DateTime>();
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DateTime.MinValue, result.Result);
    }

    [Test]
    public virtual void CanConvertObjectToString_Using_ToString_Overload()
    {
        var result = new MyTestObject().TryConvertTo<string>();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Hello world", result.Result);
    }

    [Test]
    public virtual void CanConvertObjectToSameObject()
    {
        var obj = new MyTestObject();
        var result = obj.TryConvertTo<object>();

        Assert.AreEqual(obj, result.Result);
    }

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

    [Test]
    public void ConvertToDateTimeTest()
    {
        var conv = "2016-06-07".TryConvertTo<DateTime>();
        Assert.IsTrue(conv);
        Assert.AreEqual(new DateTime(2016, 6, 7), conv.Result);
    }

    [Test]
    public void ConvertToNullableDateTimeTest()
    {
        var conv = "2016-06-07".TryConvertTo<DateTime?>();
        Assert.IsTrue(conv);
        Assert.AreEqual(new DateTime(2016, 6, 7), conv.Result);
    }

    [Test]
    public void Value_Editor_Can_Convert_Decimal_To_Decimal_Clr_Type()
    {
        var valueEditor = MockedValueEditors.CreateDataValueEditor(ValueTypes.Decimal);

        var result = valueEditor.TryConvertValueToCrlType(12.34d);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(12.34d, result.Result);
    }

    private class MyTestObject
    {
        public override string ToString() => "Hello world";
    }
}
