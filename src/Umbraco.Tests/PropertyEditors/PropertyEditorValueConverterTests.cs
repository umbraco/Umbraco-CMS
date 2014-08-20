using System;
using NUnit.Framework;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
	public class PropertyEditorValueConverterTests
	{
        // see notes in the converter
        // only ONE date format is expected here

        //[TestCase("2012-11-10", true)]
        //[TestCase("2012/11/10", true)]
        //[TestCase("10/11/2012", true)]
        //[TestCase("11/10/2012", false)]
        //[TestCase("Sat 10, Nov 2012", true)]
        //[TestCase("Saturday 10, Nov 2012", true)]
        //[TestCase("Sat 10, November 2012", true)]
        //[TestCase("Saturday 10, November 2012", true)]
        //[TestCase("2012-11-10 13:14:15", true)]
        [TestCase("2012-11-10 13:14:15", true)]
        [TestCase("2012-11-10T13:14:15", true)]
        [TestCase("", false)]
		public void CanConvertDatePickerPropertyEditor(string date, bool expected)
		{
			var converter = new DatePickerValueConverter();
			var dateTime = new DateTime(2012, 11, 10, 13, 14, 15);
			var result = converter.ConvertDataToSource(null, date, false); // does not use type for conversion

		    if (expected)
		        Assert.AreEqual(dateTime.Date, ((DateTime) result).Date);
            else
                Assert.AreNotEqual(dateTime.Date, ((DateTime)result).Date);
        }

        [TestCase("TRUE", true)]
        [TestCase("True", true)]
        [TestCase("true", true)]
        [TestCase("1", true)]
        [TestCase(1, true)]
        [TestCase(true, true)]
        [TestCase("FALSE", false)]
        [TestCase("False", false)]
        [TestCase("false", false)]
        [TestCase("0", false)]
        [TestCase(0, false)]
        [TestCase(false, false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        [TestCase("blah", false)]
        public void CanConvertYesNoPropertyEditor(object value, bool expected)
        {
            var converter = new YesNoValueConverter();
            var result = converter.ConvertDataToSource(null, value, false); // does not use type for conversion

            Assert.AreEqual(expected, result);
        }
	}
}
