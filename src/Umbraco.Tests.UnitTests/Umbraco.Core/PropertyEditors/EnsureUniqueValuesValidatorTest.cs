using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.PropertyEditors
{
    [TestFixture]
    public class EnsureUniqueValuesValidatorTest
    {
        private ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;
        [Test]
        public void Only_Tests_On_JArray()
        {
            var validator = new ValueListUniqueValueValidator();
            var result = validator.Validate("hello", null, new ColorPickerPropertyEditor(_loggerFactory, Mock.Of<IDataTypeService>(), Mock.Of<ILocalizationService>(), Mock.Of<IIOHelper>(), Mock.Of<IShortStringHelper>(), Mock.Of<ILocalizedTextService>()));
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void Only_Tests_On_JArray_Of_Item_JObject()
        {
            var validator = new ValueListUniqueValueValidator();
            var result = validator.Validate(new JArray("hello", "world"), null, new ColorPickerPropertyEditor(_loggerFactory, Mock.Of<IDataTypeService>(), Mock.Of<ILocalizationService>(), Mock.Of<IIOHelper>(), Mock.Of<IShortStringHelper>(), Mock.Of<ILocalizedTextService>()));
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void Allows_Unique_Values()
        {
            var validator = new ValueListUniqueValueValidator();
            var result = validator.Validate(new JArray(JObject.FromObject(new { value = "hello" }), JObject.FromObject(new { value = "world" })), null, new ColorPickerPropertyEditor(_loggerFactory, Mock.Of<IDataTypeService>(), Mock.Of<ILocalizationService>(), Mock.Of<IIOHelper>(), Mock.Of<IShortStringHelper>(), Mock.Of<ILocalizedTextService>()));
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void Does_Not_Allow_Multiple_Values()
        {
            var validator = new ValueListUniqueValueValidator();
            var result = validator.Validate(new JArray(JObject.FromObject(new { value = "hello" }), JObject.FromObject(new { value = "hello" })),
                                            null, new ColorPickerPropertyEditor(_loggerFactory, Mock.Of<IDataTypeService>(), Mock.Of<ILocalizationService>(), Mock.Of<IIOHelper>(), Mock.Of<IShortStringHelper>(), Mock.Of<ILocalizedTextService>()));
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void Validates_Multiple_Duplicate_Values()
        {
            var validator = new ValueListUniqueValueValidator();
            var result = validator.Validate(new JArray(
                                                JObject.FromObject(new { value = "hello" }),
                                                JObject.FromObject(new { value = "hello" }),
                                                JObject.FromObject(new { value = "world" }),
                                                JObject.FromObject(new { value = "world" })),
                                            null, new ColorPickerPropertyEditor(_loggerFactory, Mock.Of<IDataTypeService>(), Mock.Of<ILocalizationService>(), Mock.Of<IIOHelper>(), Mock.Of<IShortStringHelper>(), Mock.Of<ILocalizedTextService>()));
            Assert.AreEqual(2, result.Count());
        }
    }
}
