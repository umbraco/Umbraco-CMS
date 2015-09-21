using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.UI.WebControls;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests
{
	[TestFixture]
	public class ObjectExtensionsTests
	{
        [TestFixtureSetUp]
		public void FixtureSetup()
		{
		}

        [Test]
        public void CanParseStringToUnit()
        {
            var stringUnit = "1234px";
            object objUnit = "1234px";
            var result = stringUnit.TryConvertTo<Unit>();
            var result2 = objUnit.TryConvertTo<Unit>();
            var unit = new Unit("1234px");
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result2.Success);
            Assert.AreEqual(unit, result.Result);
            Assert.AreEqual(unit, result2.Result);
        }

	    [Test]
	    public void Can_Convert_List_To_Enumerable()
	    {
	        var list = new List<string>() {"hello", "world", "awesome"};
	        var result = list.TryConvertTo<IEnumerable<string>>();
	        Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.Result.Count());
	    }

	    [Test]
		public void ObjectExtensions_Object_To_Dictionary()
		{
			//Arrange

			var obj = new { Key1 = "value1", Key2 = "value2", Key3 = "value3" };

			//Act

			var d = obj.ToDictionary<string>();

			//Assert

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
			var result = i.TryConvertTo<int?>();
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
					{"TRUE", true},
					{"True", true},
					{"true", true},
					{"1", true},
					{"FALSE", false},
					{"False", false},
					{"false", false},
					{"0", false},
					{"", false}
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
        [TestCase("10/11/2012", true)]// assuming your culture uses DD/MM/YYYY
        [TestCase("11/10/2012", false)]// assuming your culture uses DD/MM/YYYY
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
            var result = "".TryConvertTo<DateTime?>();
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.Result);
        }

        [Test]
        public virtual void CanConvertBlankStringToNullBool()
        {
            var result = "".TryConvertTo<bool?>();
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.Result);
        }

		[Test]
		public virtual void CanConvertBlankStringToDateTime()
		{
			var result = "".TryConvertTo<DateTime>();
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
		
		private CultureInfo savedCulture;

	    /// <summary>
		/// Run once before each test in derived test fixtures.
		/// </summary>
        [SetUp]
		public void TestSetup()
		{
			savedCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB"); // make sure the dates parse correctly
			return;
		}

		/// <summary>
		/// Run once after each test in derived test fixtures.
		/// </summary>
	    [TearDown]
		public void TestTearDown()
		{
			Thread.CurrentThread.CurrentCulture = savedCulture;
			return;
		}

        private class MyTestObject
        {
            public override string ToString()
            {
                return "Hello world";
            }
        }
	}
}