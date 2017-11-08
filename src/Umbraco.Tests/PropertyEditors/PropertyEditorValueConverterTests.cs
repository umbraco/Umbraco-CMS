using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core.PropertyEditors.ValueConverters;

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

        [TestCase("apples", new[] { "apples" })]
        [TestCase("apples,oranges", new[] { "apples", "oranges" })]
        [TestCase(" apples, oranges, pears ", new[] { "apples", "oranges", "pears" })]
        [TestCase("", new string[] { })]
        [TestCase(null, new string[] { })]
        public void CanConvertCheckboxListPropertyEditor(object value, IEnumerable<string> expected)
        {
            var converter = new CheckboxListValueConverter();
            var result = converter.ConvertSourceToObject(null, value, false);

            Assert.AreEqual(expected, result);
        }

        [TestCase("apples", new[] {"apples"})]
        [TestCase("apples,oranges", new[] {"apples", "oranges"})]
        [TestCase("apples , oranges, pears ", new[] {"apples", "oranges", "pears"})]
        [TestCase("", new string[] {})]
        [TestCase(null, new string[] {})]
	    public void CanConvertDropdownListMultiplePropertyEditor(object value, IEnumerable<string> expected)
	    {
	        var converter = new DropdownListMultipleValueConverter();
	        var result = converter.ConvertSourceToObject(null, value, false);

            Assert.AreEqual(expected, result);
	    }

        [TestCase("100", new[] {100})]
        [TestCase("100,200", new[] {100, 200})]
        [TestCase("100 , 200, 300 ", new[] {100, 200, 300})]
        [TestCase("", new int[] {})]
        [TestCase(null, new int[] {})]
	    public void CanConvertDropdownListMultipleWithKeysPropertyEditor(object value, IEnumerable<int> expected)
	    {
	        var converter = new DropdownListMultipleWithKeysValueConverter();
	        var result = converter.ConvertDataToSource(null, value, false);

            Assert.AreEqual(expected, result);
	    }

        [TestCase(null, "1", false, 1)]
        [TestCase(null, "1", true, 1)]
        [TestCase(null, "0", false, 0)]
        [TestCase(null, "0", true, 0)]
        [TestCase(null, null, false, 0)]
        [TestCase(null, null, true, 0)]
        [TestCase(null, "-1", false, -1)]
        [TestCase(null, "-1", true, -1)]
        [TestCase(null, "1.65", false, 1.65)]
        [TestCase(null, "1.65", true, 1.65)]
        [TestCase(null, "-1.65", false, -1.65)]
        [TestCase(null, "-1.65", true, -1.65)]
        public void CanConvertDecimalAliasPropertyEditor(Core.Models.PublishedContent.PublishedPropertyType propertyType, object value, bool preview, double expected)
        {
            var converter = new DecimalValueConverter();
            var result = converter.ConvertDataToSource(propertyType, value, preview);

            Assert.AreEqual(expected, result);
        }
    }
}
