using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PropertyEditors.ValueConverters;

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
            var result = converter.ConvertSourceToIntermediate(null, null, date, false); // does not use type for conversion

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
            var result = converter.ConvertSourceToIntermediate(null, null, value, false); // does not use type for conversion

            Assert.AreEqual(expected, result);
        }

        [TestCase("[\"apples\"]", new[] { "apples" })]
        [TestCase("[\"apples\",\"oranges\"]", new[] { "apples", "oranges" })]
        [TestCase("[\"apples\",\"oranges\",\"pears\"]", new[] { "apples", "oranges", "pears" })]
        [TestCase("", new string[] { })]
        [TestCase(null, new string[] { })]
        public void CanConvertCheckboxListPropertyEditor(object value, IEnumerable<string> expected)
        {
            var converter = new CheckboxListValueConverter();
            var result = converter.ConvertIntermediateToObject(null, null, PropertyCacheLevel.Unknown, value, false);

            Assert.AreEqual(expected, result);
        }

        [TestCase("[\"apples\"]", new[] { "apples" })]
        [TestCase("[\"apples\",\"oranges\"]", new[] { "apples", "oranges" })]
        [TestCase("[\"apples\",\"oranges\",\"pears\"]", new[] { "apples", "oranges", "pears" })]
        [TestCase("", new string[] { })]
        [TestCase(null, new string[] { })]
        public void CanConvertDropdownListMultiplePropertyEditor(object value, IEnumerable<string> expected)
        {
            var mockPublishedContentTypeFactory = new Mock<IPublishedContentTypeFactory>();
            mockPublishedContentTypeFactory.Setup(x => x.GetDataType(123))
                .Returns(new PublishedDataType(123, "test", new Lazy<object>(() => new DropDownFlexibleConfiguration
                    {
                        Multiple = true
                    })));

            var publishedPropType = new PublishedPropertyType(
                new PublishedContentType(Guid.NewGuid(), 1234, "test", PublishedItemType.Content, Enumerable.Empty<string>(), Enumerable.Empty<PublishedPropertyType>(), ContentVariation.Nothing),
                new PropertyType("test", ValueStorageType.Nvarchar) { DataTypeId = 123 },
                new PropertyValueConverterCollection(Enumerable.Empty<IPropertyValueConverter>()),
                Mock.Of<IPublishedModelFactory>(), mockPublishedContentTypeFactory.Object);

            var converter = new FlexibleDropdownPropertyValueConverter();
            var inter = converter.ConvertSourceToIntermediate(null, publishedPropType, value, false);
            var result = converter.ConvertIntermediateToObject(null, publishedPropType, PropertyCacheLevel.Unknown, inter, false);

            Assert.AreEqual(expected, result);
        }

        [TestCase("1", 1)]
        [TestCase("1", 1)]
        [TestCase("0", 0)]
        [TestCase("0", 0)]
        [TestCase(null, 0)]
        [TestCase(null, 0)]
        [TestCase("-1", -1)]
        [TestCase("-1", -1)]
        [TestCase("1.65", 1.65)]
        [TestCase("1.65", 1.65)]
        [TestCase("-1.65", -1.65)]
        [TestCase("-1.65", -1.65)]
        public void CanConvertDecimalAliasPropertyEditor(object value, double expected)
        {
            var converter = new DecimalValueConverter();
            var inter = converter.ConvertSourceToIntermediate(null, null, value, false);
            var result = converter.ConvertIntermediateToObject(null, null, PropertyCacheLevel.Unknown, inter, false);

            Assert.AreEqual(expected, result);
        }
    }
}
