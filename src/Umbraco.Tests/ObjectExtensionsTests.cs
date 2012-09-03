using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Tests.PartialTrust;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests
{
	[TestFixture]
	public class ObjectExtensionsTests : AbstractPartialTrustFixture<ObjectExtensionsTests>
	{
		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			TestHelper.SetupLog4NetForTests();
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
		[TestOnlyInFullTrust]
		public void CanConvertIntToNullableInt()
		{
			var i = 1;
			var result = i.TryConvertTo<int?>();
			Assert.That(result.Success, Is.True);
		}

		[Test]
		[TestOnlyInFullTrust]
		public void CanConvertNullableIntToInt()
		{
			int? i = 1;
			var result = i.TryConvertTo<int>();
			Assert.That(result.Success, Is.True);
		}

		[Test]
		[TestOnlyInFullTrust]
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
					{"0", false}
				};

			foreach (var testCase in testCases)
			{
				var result = testCase.Key.TryConvertTo<bool>();

				Assert.IsTrue(result.Success);
				Assert.AreEqual(testCase.Value, result.Result);
			}
		}

		/// <summary>
		/// Run once before each test in derived test fixtures.
		/// </summary>
		public override void TestSetup()
		{
			return;
		}

		/// <summary>
		/// Run once after each test in derived test fixtures.
		/// </summary>
		public override void TestTearDown()
		{
			return;
		}
	}
}