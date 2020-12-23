// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core.IO;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.PropertyEditors
{
    [TestFixture]
    public class ColorListValidatorTest
    {
        private readonly ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

        [Test]
        public void Only_Tests_On_JArray()
        {
            var validator = new ColorPickerConfigurationEditor.ColorListValidator();
            IEnumerable<ValidationResult> result =
                validator.Validate(
                    "hello",
                    null,
                    new ColorPickerPropertyEditor(
                        _loggerFactory,
                        Mock.Of<IDataTypeService>(),
                        Mock.Of<ILocalizationService>(),
                        Mock.Of<IIOHelper>(),
                        Mock.Of<IShortStringHelper>(),
                        Mock.Of<ILocalizedTextService>(),
                        new JsonNetSerializer()));
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void Only_Tests_On_JArray_Of_Item_JObject()
        {
            var validator = new ColorPickerConfigurationEditor.ColorListValidator();
            IEnumerable<ValidationResult> result =
                validator.Validate(
                    new JArray("hello", "world"),
                    null,
                    new ColorPickerPropertyEditor(
                        _loggerFactory,
                        Mock.Of<IDataTypeService>(),
                        Mock.Of<ILocalizationService>(),
                        Mock.Of<IIOHelper>(),
                        Mock.Of<IShortStringHelper>(),
                        Mock.Of<ILocalizedTextService>(),
                        new JsonNetSerializer()));
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void Validates_Color_Vals()
        {
            var validator = new ColorPickerConfigurationEditor.ColorListValidator();
            IEnumerable<ValidationResult> result =
                validator.Validate(
                    new JArray(
                        JObject.FromObject(new { value = "CC0000" }),
                        JObject.FromObject(new { value = "zxcvzxcvxzcv" }),
                        JObject.FromObject(new { value = "ABC" }),
                        JObject.FromObject(new { value = "1234567" })),
                    null,
                    new ColorPickerPropertyEditor(
                        _loggerFactory,
                        Mock.Of<IDataTypeService>(),
                        Mock.Of<ILocalizationService>(),
                        Mock.Of<IIOHelper>(),
                        Mock.Of<IShortStringHelper>(),
                        Mock.Of<ILocalizedTextService>(),
                        new JsonNetSerializer()));
            Assert.AreEqual(2, result.Count());
        }
    }
}
