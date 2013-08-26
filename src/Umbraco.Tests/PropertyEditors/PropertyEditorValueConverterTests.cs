using System;
using NUnit.Framework;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
	public class PropertyEditorValueConverterTests
	{
		[TestCase("2012-11-10", true)]
		[TestCase("2012/11/10", true)]
		[TestCase("10/11/2012", true)]
		[TestCase("11/10/2012", false)]
		[TestCase("Sat 10, Nov 2012", true)]
		[TestCase("Saturday 10, Nov 2012", true)]
		[TestCase("Sat 10, November 2012", true)]
		[TestCase("Saturday 10, November 2012", true)]
		[TestCase("2012-11-10 13:14:15", true)]
		[TestCase("", false)]
		public void CanConvertDatePickerPropertyEditor(string date, bool expected)
		{
			var converter = new DatePickerPropertyEditorValueConverter();
			var dateTime = new DateTime(2012, 11, 10, 13, 14, 15);
			var result = converter.ConvertPropertyValue(date);

			Assert.IsTrue(result.Success);
			Assert.AreEqual(DateTime.Equals(dateTime.Date, ((DateTime) result.Result).Date), expected);
		}

		[TestCase("TRUE", true)]
		[TestCase("True", true)]
		[TestCase("true", true)]
		[TestCase("1", true)]
		[TestCase("FALSE", false)]
		[TestCase("False", false)]
		[TestCase("false", false)]
		[TestCase("0", false)]
		[TestCase("", false)]
		public void CanConvertYesNoPropertyEditor(string value, bool expected)
		{
			var converter = new YesNoPropertyEditorValueConverter();
			var result = converter.ConvertPropertyValue(value);

			Assert.IsTrue(result.Success);
			Assert.AreEqual(expected, result.Result);
		}
	}
}
