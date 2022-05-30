// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class ColorListValidatorTest
{
    private readonly ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

    private ColorPickerPropertyEditor ColorPickerPropertyEditor => new(
        Mock.Of<IDataValueEditorFactory>(),
        Mock.Of<IIOHelper>(),
        new JsonNetSerializer(),
        Mock.Of<IEditorConfigurationParser>());

    [Test]
    public void Only_Tests_On_JArray()
    {
        var validator = new ColorPickerConfigurationEditor.ColorListValidator();
        var result =
            validator.Validate(
                "hello",
                null,
                ColorPickerPropertyEditor);
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public void Only_Tests_On_JArray_Of_Item_JObject()
    {
        var validator = new ColorPickerConfigurationEditor.ColorListValidator();
        var result =
            validator.Validate(
                new JArray("hello", "world"),
                null,
                ColorPickerPropertyEditor);
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public void Validates_Color_Vals()
    {
        var validator = new ColorPickerConfigurationEditor.ColorListValidator();
        var result =
            validator.Validate(
                new JArray(
                    JObject.FromObject(new { value = "CC0000" }),
                    JObject.FromObject(new { value = "zxcvzxcvxzcv" }),
                    JObject.FromObject(new { value = "ABC" }),
                    JObject.FromObject(new { value = "1234567" })),
                null,
                ColorPickerPropertyEditor);
        Assert.AreEqual(2, result.Count());
    }
}
