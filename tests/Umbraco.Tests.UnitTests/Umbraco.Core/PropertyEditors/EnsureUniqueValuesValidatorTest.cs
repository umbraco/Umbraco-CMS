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
public class EnsureUniqueValuesValidatorTest
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
        var validator = new ValueListUniqueValueValidator();
        var result =
            validator.Validate(
                "hello",
                null,
                new ColorPickerPropertyEditor(
                    Mock.Of<IDataValueEditorFactory>(),
                    Mock.Of<IIOHelper>(),
                    new JsonNetSerializer(),
                    Mock.Of<IEditorConfigurationParser>()));
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public void Only_Tests_On_JArray_Of_Item_JObject()
    {
        var validator = new ValueListUniqueValueValidator();
        var result =
            validator.Validate(
                new JArray("hello", "world"),
                null,
                ColorPickerPropertyEditor);
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public void Allows_Unique_Values()
    {
        var validator = new ValueListUniqueValueValidator();
        var result =
            validator.Validate(
                new JArray(
                    JObject.FromObject(new { value = "hello" }),
                    JObject.FromObject(new { value = "world" })),
                null,
                ColorPickerPropertyEditor);
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public void Does_Not_Allow_Multiple_Values()
    {
        var validator = new ValueListUniqueValueValidator();
        var result =
            validator.Validate(
                new JArray(
                    JObject.FromObject(new { value = "hello" }),
                    JObject.FromObject(new { value = "hello" })),
                null,
                ColorPickerPropertyEditor);
        Assert.AreEqual(1, result.Count());
    }

    [Test]
    public void Validates_Multiple_Duplicate_Values()
    {
        var validator = new ValueListUniqueValueValidator();
        var result =
            validator.Validate(
                new JArray(
                    JObject.FromObject(new { value = "hello" }),
                    JObject.FromObject(new { value = "hello" }),
                    JObject.FromObject(new { value = "world" }),
                    JObject.FromObject(new { value = "world" })),
                null,
                ColorPickerPropertyEditor);
        Assert.AreEqual(2, result.Count());
    }
}
